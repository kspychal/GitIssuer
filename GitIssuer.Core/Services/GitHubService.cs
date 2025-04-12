using GitIssuer.Core.Dto.Responses;
using GitIssuer.Core.Services.Bases;
using System.Net.Http.Headers;

namespace GitIssuer.Core.Services;

public class GitHubService(IHttpClientFactory httpClientFactory) : GitServiceBase<GitHubAddIssueResponseDto>(httpClientFactory)
{
    public override string PersonalAccessToken => "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";
    public override string ApiUrl => "https://api.github.com/";
    public override string ProviderName => "GitHub";

    public override object CreateAddIssueRequestBody(string name, string description)
        => new
        {
            title = name,
            body = description
        };

    public override object CreateModifyIssueRequestBody(string? name, string? description)
        => new
        {
            title = name,
            body = description
        };

    public override void AddCustomRequestHeaders(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("GitIssuer");
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
    }

    public override async Task<HttpResponseMessage> SendIssueUpdateRequestAsync(HttpClient httpClient, string issueUrl, StringContent content) 
        => await httpClient.PatchAsync(issueUrl, content);
    
    public override string GetIssuesUrl(string repositoryOwner, string repositoryName)
        => $"repos/{repositoryOwner}/{repositoryName}/issues";

    protected override string ExtractUrl(GitHubAddIssueResponseDto response)
        => response.HtmlUrl!;
}