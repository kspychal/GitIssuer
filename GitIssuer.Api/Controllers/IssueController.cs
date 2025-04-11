using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services.Bases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace GitIssuer.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class IssueController(IGitServiceFactory gitServiceFactory) : ControllerBase
{
    [HttpPost("{gitProviderName}/add")]
    public async Task<IActionResult> AddIssue(string gitProviderName, string title, string body)
    {
        IGitService? gitService;
        try
        {
            gitService = gitServiceFactory.GetService(gitProviderName);
        }
        catch (NotSupportedException)
        {
            var validGitProviderNames = string.Join(", ", gitServiceFactory.GetValidGitProviderNames());
            return BadRequest($"Provided GIT provider name ({gitProviderName}) is not supported. Valid platforms: {validGitProviderNames}.");
        }
        catch (Exception)
        {
            return StatusCode(500, new { Error = $"An unexpected error occurred while creating GitService for gitProviderName {gitProviderName}." });
        }

        if (gitService == null)
        {
            return StatusCode(500, new { Error = $"Failed to create {gitProviderName}Service object." });
        }

        try
        {
            var result = await gitService.AddIssue("owner", "repo", title, body); 
            return Ok(result);
        }
        catch (Exception exception) when (exception is HttpRequestException or JsonException)
        {
            return StatusCode(503, new { Error = "Failed to create GitHub issue.", Details = exception.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = "An unexpected error occurred while adding the issue.", Details = ex.Message });
        }
    }
}