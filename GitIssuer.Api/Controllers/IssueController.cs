using GitIssuer.Api.Models;
using GitIssuer.Core.Dto.Requests;
using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services.Bases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GitIssuer.Core.Exceptions;

namespace GitIssuer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssueController(IGitServiceFactory gitServiceFactory, ILogger<IssueController> logger) : ControllerBase
{
    protected readonly ILogger<IssueController> Logger = logger;

    [HttpPost("{gitProviderName}/{repositoryOwner}/{repositoryName}/add")]
    public Task<IActionResult> AddIssue(string gitProviderName, string repositoryOwner, string repositoryName, [FromBody] AddIssueRequestDto issue) 
        => ExecuteGitServiceActionAsync(gitProviderName,
            gitService => gitService.AddIssueAsync(repositoryOwner, repositoryName, issue.Title, issue.Description), 
            CreatedResponse);
    

    [HttpPut("{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/modify")]
    public Task<IActionResult> ModifyIssue(string gitProviderName, string repositoryOwner, string repositoryName, int issueId, [FromBody] ModifyIssueRequestDto issue) 
        => ExecuteGitServiceActionAsync(gitProviderName, 
            gitService => gitService.ModifyIssueAsync(repositoryOwner, repositoryName, issueId, issue.Title, issue.Description), 
            OkResponse);

    [HttpPatch("{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/close")]
    public Task<IActionResult> CloseIssue(string gitProviderName, string repositoryOwner, string repositoryName, int issueId)
        => ExecuteGitServiceActionAsync(gitProviderName,
            gitService => gitService.CloseIssueAsync(repositoryOwner, repositoryName, issueId),
            OkResponse);

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
        catch (Exception exception) when (exception is GitException)
        {
            Logger.LogInformation(exception.Message);
            return ServiceUnavailableResponse(exception.Message);
        }
        catch (Exception exception) when (exception is ArgumentException or HttpRequestException or JsonException or ArgumentException)
        {
            var message = $"Failed to process issue. ({exception.Message})";
            Logger.LogError(message);
            return ServiceUnavailableResponse(message);
        }
        catch (Exception exception)
        {
            var message = $"An unexpected error occurred. ({exception.Message})";
            Logger.LogCritical(message);
            return InternalServerErrorResponse(message);
        }
    }

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
            var message = $"Provided GIT provider name ({gitProviderName}) is not supported. Valid platforms: {validGitProviderNames}.";
            Logger.LogInformation(message);
            return (null, BadRequestResponse(message));
        }
        catch (Exception)
        {
            var message = $"An unexpected error occurred while creating GitService for {gitProviderName}.";
            Logger.LogCritical(message);
            return (null, InternalServerErrorResponse(message));
        }
    }

    #region Responses

    protected IActionResult OkResponse(string url)
        => StatusCode(200, SuccessApiResponse(url));

    protected IActionResult CreatedResponse(string url)
        => StatusCode(201, SuccessApiResponse(url));

    protected IActionResult BadRequestResponse(string message)
        => StatusCode(500, ErrorApiResponse(message));

    protected IActionResult InternalServerErrorResponse(string message)
        => StatusCode(500, ErrorApiResponse(message));

    protected IActionResult ServiceUnavailableResponse(string message)
        => StatusCode(503, ErrorApiResponse(message));

    private static ApiResponseBody SuccessApiResponse(string url)
        => new() { Success = true, Url = url };

    private static ApiResponseBody ErrorApiResponse(string message)
        => new() { Success = false, Error = message };

    #endregion
}