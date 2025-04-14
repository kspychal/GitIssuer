using GitIssuer.Core.Dto.Responses;
using GitIssuer.Core.Services.Bases;
using System.Net.Http.Headers;

namespace GitIssuer.Core.Services;

public class GitHubService(IHttpClientFactory httpClientFactory, string personalAccessToken) : GitServiceBase<GitHubIssueResponseDto>(httpClientFactory, personalAccessToken)
{
    protected override string ApiBaseUrl => "https://api.github.com/";
    protected override string ProviderName => "GitHub";

    /// <inheritdoc/>
    protected override object CreateAddIssueRequestBody(string name, string description)
        => new
        {
            title = name,
            body = description
        };

    /// <inheritdoc/>
    protected override object CreateModifyIssueRequestBody(string? name, string? description)
        => new
        {
            title = name,
            body = description
        };

    /// <inheritdoc/>
    protected override object CreateCloseIssueRequestBody()
        => new
        {
            state = "closed"
        };

    /// <inheritdoc/>
    protected override void AddCustomRequestHeaders(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("GitIssuer");
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
    }

    /// <inheritdoc/>
    protected override HttpMethod GetModifyIssueHttpMethod()
        => HttpMethod.Patch;

    /// <inheritdoc/>
    protected override string BuildIssuesApiUrl(string repositoryOwner, string repositoryName)
        => $"repos/{repositoryOwner}/{repositoryName}/issues";
}