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
    public async Task<IActionResult> AddIssue(string gitProviderName, string repositoryOwner, string repositoryName, [FromBody] AddIssueRequestDto issue)
    {
        IGitService? gitService;
        try
        {
            gitService = gitServiceFactory.GetService(gitProviderName);
        }
        catch (NotSupportedException)
        {
            var validGitProviderNames = string.Join(", ", gitServiceFactory.GetValidGitProviderNames());
            return BadRequestResponse($"Provided GIT provider name ({gitProviderName}) is not supported. Valid platforms: {validGitProviderNames}.");
        }
        catch (Exception)
        {
            return InternalServerErrorResponse($"An unexpected error occurred while creating GitService for gitProviderName {gitProviderName}.");
        }

        if (gitService == null)
        {
            return InternalServerErrorResponse($"Failed to create {gitProviderName}Service object.");
        }

        try
        {
            var result = await gitService.AddIssueAsync(repositoryOwner, repositoryName, issue.Title, issue.Description);
            return CreatedResponse(result);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            return ServiceUnavailableResponse($"Failed to create GitHub issue. ({exception.Message})");
        }
        catch (Exception exception)
        {
            return InternalServerErrorResponse($"An unexpected error occurred while adding the issue. ({exception.Message})");
        }
    }

    #region Responses

    protected IActionResult CreatedResponse(string message)
        => StatusCode(200, new ApiResponseBody { Success = true, Url = message });

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