using GitIssuer.Core.Dto.Responses;
using GitIssuer.Core.Services.Bases;

namespace GitIssuer.Core.Services;

public class GitLabService(IHttpClientFactory httpClientFactory, string personalAccessToken) : GitServiceBase<GitLabIssueResponseDto>(httpClientFactory, personalAccessToken)
{
    protected override string ApiBaseUrl => "https://gitlab.com/api/v4/";
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
    protected override string BuildIssuesApiUrl(string repositoryOwner, string repositoryName) 
        => $"projects/{Uri.EscapeDataString($"{repositoryOwner}/{repositoryName}")}/issues";
}