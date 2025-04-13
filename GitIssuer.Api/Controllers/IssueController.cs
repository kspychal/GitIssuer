using GitIssuer.Api.Models;
using GitIssuer.Core.Dto.Requests;
using GitIssuer.Core.Exceptions;
using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services.Bases.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GitIssuer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssueController(IGitServiceFactory gitServiceFactory, ILogger<IssueController> logger) : ControllerBase
{
    protected readonly ILogger<IssueController> Logger = logger;

    /// <summary>
    /// Adds a new issue to the specified repository on the given Git provider.
    /// </summary>
    /// <param name="gitProviderName">The name of the Git provider (e.g., GitHub, GitLab).</param>
    /// <param name="repositoryOwner">The owner of the repository.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="issue">The issue data including title and description.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPost("{gitProviderName}/{repositoryOwner}/{repositoryName}/add")]
    public Task<IActionResult> AddIssue(string gitProviderName, string repositoryOwner, string repositoryName, [FromBody] AddIssueRequestDto issue) 
        => ExecuteGitServiceActionAsync(gitProviderName,
            gitService => gitService.AddIssueAsync(repositoryOwner, repositoryName, issue.Title, issue.Description), 
            CreatedResponse);

    /// <summary>
    /// Modifies an existing issue in the specified repository on the given Git provider.
    /// </summary>
    /// <param name="gitProviderName">The name of the Git provider (e.g., GitHub, GitLab).</param>
    /// <param name="repositoryOwner">The owner of the repository.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="issueId">The ID of the issue to be modified.</param>
    /// <param name="issue">The updated issue data including title and description.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPut("{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/modify")]
    public Task<IActionResult> ModifyIssue(string gitProviderName, string repositoryOwner, string repositoryName, int issueId, [FromBody] ModifyIssueRequestDto issue) 
        => ExecuteGitServiceActionAsync(gitProviderName, 
            gitService => gitService.ModifyIssueAsync(repositoryOwner, repositoryName, issueId, issue.Title, issue.Description), 
            OkResponse);

    /// <summary>
    /// Closes an existing issue in the specified repository on the given Git provider.
    /// </summary>
    /// <param name="gitProviderName">The name of the Git provider (e.g., GitHub, GitLab).</param>
    /// <param name="repositoryOwner">The owner of the repository.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="issueId">The ID of the issue to be closed.</param>
    /// <returns>An <see cref="IActionResult"/> indicating the result of the operation.</returns>
    [HttpPatch("{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/close")]
    public Task<IActionResult> CloseIssue(string gitProviderName, string repositoryOwner, string repositoryName, int issueId)
        => ExecuteGitServiceActionAsync(gitProviderName,
            gitService => gitService.CloseIssueAsync(repositoryOwner, repositoryName, issueId),
            OkResponse);

    /// <summary>
    /// Executes a Git service action with appropriate error handling and logging.
    /// </summary>
    /// <param name="gitProviderName">The name of the Git provider.</param>
    /// <param name="gitServiceAction">The method that executes the Git service action.</param>
    /// <param name="createSuccessResponseAction">A method to generate a success response.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    private async Task<IActionResult> ExecuteGitServiceActionAsync(string gitProviderName, Func<IGitService, Task<string>> gitServiceAction, Func<string, IActionResult> createSuccessResponseAction)
    {
        var (gitService, actionResult) = TryGetGitService(gitProviderName);
        if (gitService == null)
            return actionResult!;

        try
        {
            var issueUrl = await gitServiceAction(gitService);
            var message = $"Successfully handled {issueUrl} request.";
            Logger.LogInformation(message);
            return createSuccessResponseAction(issueUrl);
        }
        catch (GitException exception)
        {
            Logger.LogInformation(exception.Message);
            return ServiceUnavailableResponse(exception.Message, exception.InnerMessage);
        }
        catch (Exception exception)
        {
            const string error = "An unexpected error occurred.";
            var details = exception.Message;
            Logger.LogCritical("{Error} {Details}", error, details);
            return InternalServerErrorResponse(error, details);
        }
    }

    /// <summary>
    /// Attempts to retrieve an <see cref="IGitService"/> instance for the specified Git provider name.
    /// </summary>
    /// <param name="gitProviderName">The name of the Git provider.</param>
    /// <returns>A tuple containing either the <see cref="IGitService"/> instance or an <see cref="IActionResult"/> in case of failure.</returns>
    private (IGitService? Service, IActionResult? Result) TryGetGitService(string gitProviderName)
    {
        try
        {
            var gitService = gitServiceFactory.GetService(gitProviderName);
            if (gitService != null) return (gitService, null);

            var message = $"Failed to create {gitProviderName}Service object.";
            Logger.LogCritical(message);
            return (null, InternalServerErrorResponse(message));
        }
        catch (NotSupportedException)
        {
            var validGitProviderNames = string.Join(", ", gitServiceFactory.GetValidGitProviderNames());
            var message = $"Provided GIT provider name ({gitProviderName}) is not supported.";
            var details = $"Valid platforms: {validGitProviderNames}.";

            Logger.LogInformation("{Message} {Details}", message, details);

            return (null, BadRequestResponse(message, details));
        }
        catch (Exception exception)
        {
            var error = $"An unexpected error occurred while creating GitService for {gitProviderName}.";
            Logger.LogCritical("{Error} {Details}", error, exception.Message);
            return (null, InternalServerErrorResponse(error));
        }
    }

    #region Responses

    protected IActionResult OkResponse(string url)
        => StatusCode(200, new SuccessApiResponseBody(url));

    protected IActionResult CreatedResponse(string url)
        => StatusCode(201, new SuccessApiResponseBody(url));

    protected IActionResult BadRequestResponse(string error, string? details = null)
        => StatusCode(500, new ErrorApiResponseBody(error, details));

    protected IActionResult InternalServerErrorResponse(string error, string? details = null)
        => StatusCode(500, new ErrorApiResponseBody(error, details));

    protected IActionResult ServiceUnavailableResponse(string error, string? details = null)
        => StatusCode(503, new ErrorApiResponseBody(error, details));

    #endregion
}