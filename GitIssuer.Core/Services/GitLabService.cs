using GitIssuer.Core.Dto.Responses;
using GitIssuer.Core.Services.Bases;

namespace GitIssuer.Core.Services;

public class GitLabService(IHttpClientFactory httpClientFactory) : GitServiceBase<GitLabResponseDto>(httpClientFactory)
{
    protected override string PersonalAccessToken => "glpat-KGKv13Qz5juRXGf6vyos";
    protected override string ApiUrl => "https://gitlab.com/api/v4/";
    protected override string ProviderName => "GitLab";

    /// <inheritdoc/>
    protected override object CreateAddIssueRequestBody(string name, string description)
        => new
        {
            title = name,
            description
        };

    /// <inheritdoc/>
    protected override object CreateModifyIssueRequestBody(string? name, string? description)
        => new
        {
            title = name,
            description
        };

    /// <inheritdoc/>
    protected override object CreateCloseIssueRequestBody()
        => new
        {
            state_event = "close"
        };

    /// <inheritdoc/>
    protected override void AddCustomRequestHeaders(HttpClient httpClient) { }

    /// <inheritdoc/>
    protected override HttpMethod GetModifyIssueHttpMethod()
        => HttpMethod.Put;

    /// <inheritdoc/>
    protected override string GetBaseIssuesUrl(string repositoryOwner, string repositoryName) 
        => $"projects/{Uri.EscapeDataString($"{repositoryOwner}/{repositoryName}")}/issues";

    /// <inheritdoc/>
    protected override string ExtractUrl(GitLabResponseDto response)
        => response.WebUrl!;
}