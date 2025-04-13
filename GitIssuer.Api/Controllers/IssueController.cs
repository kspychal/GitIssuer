using GitIssuer.Api.Models;
using GitIssuer.Core.Dto.Requests;
using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services.Bases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GitIssuer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssueController(IGitServiceFactory gitServiceFactory) : ControllerBase
{
    [HttpPost("{gitProviderName}/{repositoryOwner}/{repositoryName}/add")]
    public Task<IActionResult> AddIssue(string gitProviderName, string repositoryOwner, string repositoryName, [FromBody] AddIssueRequestDto issue) 
        => ExecuteGitServiceActionAsync(gitProviderName,
            gitService => gitService.AddIssueAsync(repositoryOwner, repositoryName, issue.Title, issue.Description), 
            CreatedResponse
        );
    

    [HttpPut("{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/modify")]
    public Task<IActionResult> ModifyIssue(string gitProviderName, string repositoryOwner, string repositoryName, int issueId, [FromBody] ModifyIssueRequestDto issue) 
        => ExecuteGitServiceActionAsync(gitProviderName, 
            gitService => gitService.ModifyIssueAsync(repositoryOwner, repositoryName, issueId, issue.Title, issue.Description), 
            OkResponse
        );

    [HttpPatch("{gitProviderName}/{repositoryOwner}/{repositoryName}/{issueId}/close")]
    public Task<IActionResult> CloseIssue(string gitProviderName, string repositoryOwner, string repositoryName, int issueId)
        => ExecuteGitServiceActionAsync(gitProviderName,
            gitService => gitService.CloseIssueAsync(repositoryOwner, repositoryName, issueId),
            OkResponse
        );

    private async Task<IActionResult> ExecuteGitServiceActionAsync(string gitProviderName, Func<IGitService, Task<string>> gitServiceAction, Func<string, IActionResult> createResponseFromResult)
    {
        var (gitService, actionResult) = TryGetGitService(gitProviderName);
        if (gitService == null)
            return actionResult!;

        try
        {
            var result = await gitServiceAction(gitService);
            return createResponseFromResult(result);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            return ServiceUnavailableResponse($"Failed to process issue. ({exception.Message})");
        }
        catch (Exception exception)
        {
            return InternalServerErrorResponse($"An unexpected error occurred. ({exception.Message})");
        }
    }

    private (IGitService? Service, IActionResult? Result) TryGetGitService(string gitProviderName)
    {
        try
        {
            var gitService = gitServiceFactory.GetService(gitProviderName);
            if (gitService == null)
                return (null, InternalServerErrorResponse($"Failed to create {gitProviderName}Service object."));
            return (gitService, null);
        }
        catch (NotSupportedException)
        {
            var validGitProviderNames = string.Join(", ", gitServiceFactory.GetValidGitProviderNames());
            return (null, BadRequestResponse($"Provided GIT provider name ({gitProviderName}) is not supported. Valid platforms: {validGitProviderNames}."));
        }
        catch (Exception)
        {
            return (null, InternalServerErrorResponse($"An unexpected error occurred while creating GitService for {gitProviderName}."));
        }
    }

    #region Responses

    protected IActionResult OkResponse(string message)
        => StatusCode(200, new ApiResponseBody { Success = true, Url = message });

    protected IActionResult CreatedResponse(string message)
        => StatusCode(201, new ApiResponseBody { Success = true, Url = message });

    protected IActionResult BadRequestResponse(string message)
        => StatusCode(500, ErrorApiResponse(message));

    protected IActionResult InternalServerErrorResponse(string message)
        => StatusCode(500, ErrorApiResponse(message));

    protected IActionResult ServiceUnavailableResponse(string message)
        => StatusCode(503, ErrorApiResponse(message));

    private static ApiResponseBody ErrorApiResponse(string message)
        => new() { Success = false, Error = message };

    #endregion
}