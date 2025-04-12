using GitIssuer.Core.Dto.Responses;
using GitIssuer.Core.Services.Bases;

namespace GitIssuer.Core.Services;

public class GitLabService(IHttpClientFactory httpClientFactory) : GitServiceBase<GitLabAddIssueResponseDto>(httpClientFactory)
{
    public override string PersonalAccessToken => "glpat-KGKv13Qz5juRXGf6vyos";
    public override string ApiUrl => "https://gitlab.com/api/v4/";
    public override string ProviderName => "GitLab";

    public override object CreateAddIssueRequestBody(string name, string description)
        => new
        {
            title = name,
            description
        };

    public override void AddCustomRequestHeaders(HttpClient httpClient) { }

    public override string GetIssuesUrl(string repositoryOwner, string repositoryName) 
        => $"projects/{Uri.EscapeDataString($"{repositoryOwner}/{repositoryName}")}/issues";

    protected override string ExtractUrl(GitLabAddIssueResponseDto response)
        => response.WebUrl!;
}