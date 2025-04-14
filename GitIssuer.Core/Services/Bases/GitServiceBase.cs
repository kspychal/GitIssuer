using GitIssuer.Core.Services.Bases.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GitIssuer.Core.Exceptions;

namespace GitIssuer.Core.Services.Bases;

public abstract class GitServiceBase<TResponse>(IHttpClientFactory httpClientFactory) : IGitService
{
    protected abstract string PersonalAccessToken { get; }
    protected abstract string ApiUrl { get; }
    protected abstract string ProviderName { get; }

    /// <inheritdoc/>
    public async Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string issueName, string issueDescription)
    {
        var requestBody = CreateAddIssueRequestBody(issueName, issueDescription);
        var issuesUrl = GetBaseIssuesUrl(repositoryOwner, repositoryName);
        return await SendIssueRequestAsync(issuesUrl, requestBody, HttpMethod.Post);
    }

    /// <inheritdoc/>
    public async Task<string> ModifyIssueAsync(string repositoryOwner, string repositoryName, int issueId, string? issueName, string? issueDescription)
    {
        var requestBody = CreateModifyIssueRequestBody(issueName, issueDescription);
        var baseIssuesUrl = GetBaseIssuesUrl(repositoryOwner, repositoryName);
        var issueUrl = $"{baseIssuesUrl}/{issueId}";
        var httpMethod = GetModifyIssueHttpMethod(); 
        return await SendIssueRequestAsync(issueUrl, requestBody, httpMethod);
    }

    /// <inheritdoc/>
    public async Task<string> CloseIssueAsync(string repositoryOwner, string repositoryName, int issueId)
    {
        var requestBody = CreateCloseIssueRequestBody();
        var baseIssuesUrl = GetBaseIssuesUrl(repositoryOwner, repositoryName);
        var issueUrl = $"{baseIssuesUrl}/{issueId}";
        var httpMethod = GetModifyIssueHttpMethod();
        return await SendIssueRequestAsync(issueUrl, requestBody, httpMethod);
    }

    /// <summary>
    /// Sends the HTTP request to interact with the issue (create, modify, or close).
    /// </summary>
    /// <param name="url">The URL for the issue endpoint.</param>
    /// <param name="requestBody">The body of the request, containing the issue details.</param>
    /// <param name="method">The HTTP method (POST, PUT, or PATCH) to use for the request.</param>
    /// <returns>A task that represents the asynchronous operation, containing the URL of the affected issue.</returns>
    /// <exception cref="GitException">Thrown when the response is not successful.</exception>
    protected virtual async Task<string> SendIssueRequestAsync(string url, object requestBody, HttpMethod method)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(ApiUrl);

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PersonalAccessToken);
        AddCustomRequestHeaders(httpClient);

        var response = method.Method switch
        {
            "POST" => await httpClient.PostAsync(url, content),
            "PATCH" => await httpClient.PatchAsync(url, content),
            "PUT" => await httpClient.PutAsync(url, content),
            _ => throw new ArgumentException("Invalid HttpMethod. Only Post, Patch, and Put are supported.")
        };

        if (response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<TResponse>(responseContent);
            return ExtractUrl(responseData!);
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        throw new GitException($"{ProviderName} API responded with a non-success status code.", errorContent);
    }

    /// <summary>
    /// Creates the request body for adding a new issue on the Git platform.
    /// </summary>
    /// <param name="name">The title of the issue.</param>
    /// <param name="description">The description of the issue.</param>
    /// <returns>A JSON-serializable object containing the issue title and body.</returns>
    protected abstract object CreateAddIssueRequestBody(string name, string description);

    /// <summary>
    /// Creates the request body for modifying an existing issue on the Git platform.
    /// </summary>
    /// <param name="name">The new title of the issue</param>
    /// <param name="description">The new description of the issue</param>
    /// <returns>An JSON-serializable object containing the updated issue title and body.</returns>
    protected abstract object CreateModifyIssueRequestBody(string? name, string? description);

    /// <summary>
    /// Creates the request body to close an issue on the Git platform.
    /// </summary>
    /// <returns>An anonymous object indicating that the issue should be closed.</returns>
    protected abstract object CreateCloseIssueRequestBody();

    /// <summary>
    /// Adds custom headers to the HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client to which headers will be added.</param>
    protected abstract void AddCustomRequestHeaders(HttpClient httpClient);

    /// <summary>
    /// Gets the HTTP method used for modifying an issue.
    /// </summary>
    protected abstract HttpMethod GetModifyIssueHttpMethod();

    /// <summary>
    /// Constructs the relative URL for accessing issues in a specific Git repository.
    /// </summary>
    /// <param name="repositoryOwner">The owner of the repository.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <returns>The relative URL to the issues endpoint.</returns>
    protected abstract string GetBaseIssuesUrl(string repositoryOwner, string repositoryName);

    /// <summary>
    /// Extracts the issue URL from the specific Git API response.
    /// </summary>
    /// <param name="response">The deserialized response.</param>
    /// <returns>The HTML URL of the affected issue.</returns>
    protected abstract string ExtractUrl(TResponse response);
}