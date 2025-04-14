using GitIssuer.Core.Dto.Responses.Interfaces;
using GitIssuer.Core.Exceptions;
using GitIssuer.Core.Services.Bases.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GitIssuer.Core.Services.Bases;

public abstract class GitServiceBase<TResponse>(IHttpClientFactory httpClientFactory, string personalAccessToken) : IGitService 
    where TResponse : IIssueResponse
{
    protected string PersonalAccessToken => personalAccessToken;
    protected abstract string ApiBaseUrl { get; }
    protected abstract string ProviderName { get; }

    /// <inheritdoc/>
    public async Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string issueName, string issueDescription)
    {
        var requestBody = CreateAddIssueRequestBody(issueName, issueDescription);
        var issuesUrl = BuildIssuesApiUrl(repositoryOwner, repositoryName);
        return await SendIssueRequestAsync(issuesUrl, requestBody, HttpMethod.Post);
    }

    /// <inheritdoc/>
    public async Task<string> ModifyIssueAsync(string repositoryOwner, string repositoryName, int issueId, string? issueName, string? issueDescription)
    {
        var requestBody = CreateModifyIssueRequestBody(issueName, issueDescription);
        var issuesUrl = BuildIssuesApiUrl(repositoryOwner, repositoryName);
        var issueUrl = $"{issuesUrl}/{issueId}";
        var httpMethod = GetModifyIssueHttpMethod(); 
        return await SendIssueRequestAsync(issueUrl, requestBody, httpMethod);
    }

    /// <inheritdoc/>
    public async Task<string> CloseIssueAsync(string repositoryOwner, string repositoryName, int issueId)
    {
        var requestBody = CreateCloseIssueRequestBody();
        var baseIssuesUrl = BuildIssuesApiUrl(repositoryOwner, repositoryName);
        var issueUrl = $"{baseIssuesUrl}/{issueId}";
        var httpMethod = GetModifyIssueHttpMethod();
        return await SendIssueRequestAsync(issueUrl, requestBody, httpMethod);
    }

    /// <summary>
    /// Sends the HTTP request to interact with the issue (create, modify, or close).
    /// </summary>
    /// <param name="endpoint">The relative URL for the issue API endpoint (e.g., "owner/repo/issues").</param>
    /// <param name="requestBody">The body of the request, containing the issue details.</param>
    /// <param name="method">The HTTP method (POST, PUT, or PATCH) to use for the request.</param>
    /// <returns>A task that represents the asynchronous operation, containing the URL of the affected issue.</returns>
    /// <exception cref="GitException">Thrown when the response is not successful.</exception>
    protected virtual async Task<string> SendIssueRequestAsync(string endpoint, object requestBody, HttpMethod method)
    {
        var httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(ApiBaseUrl);

        var content = CreateJsonStringContent(requestBody);

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PersonalAccessToken);
        AddCustomRequestHeaders(httpClient);

        var response = method.Method switch
        {
            "POST" => await PostAsync(httpClient, endpoint, content),
            "PATCH" => await PatchAsync(httpClient, endpoint, content),
            "PUT" => await PutAsync(httpClient, endpoint, content),
            _ => throw new ArgumentException("Invalid HttpMethod. Only Post, Patch, and Put are supported.")
        };

        if (response.IsSuccessStatusCode)
        {
            var responseData = await DeserializeResponseAsync(response);
            return GetIssueUrlFromResponse(responseData);
        }

        var errorContent = await response.Content.ReadAsStringAsync();
        throw new GitException($"{ProviderName} API responded with a non-success status code.", errorContent);
    }

    /// <summary>
    /// Creates a StringContent object containing the JSON representation of the provided request body,
    /// with UTF-8 encoding and "application/json" content type.
    /// </summary>
    protected virtual StringContent CreateJsonStringContent(object requestBody)
    {
        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        return new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
    }

    /// <summary>
    /// Asynchronously reads the content of an HTTP response message as a string and
    /// deserializes it into an object of the specified type <typeparamref name="TResponse"/>.
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response content into.</typeparam>
    /// <param name="response">The HTTP response message to deserialize.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the deserialized object of type <typeparamref name="TResponse"/>.</returns>
    protected virtual async Task<TResponse> DeserializeResponseAsync(HttpResponseMessage response)
    {
        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(responseContent)!;
    }

    /// <summary>
    /// Extracts the issue URL from the specific Git API response.
    /// </summary>
    /// <param name="response">The deserialized response.</param>
    /// <returns>The HTML URL of the affected issue.</returns>
    protected virtual string GetIssueUrlFromResponse(TResponse response)
        => response.IssueUrl!;

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
    protected abstract string BuildIssuesApiUrl(string repositoryOwner, string repositoryName);

    /// <summary>
    /// Created only for mocking purposes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected virtual Task<HttpResponseMessage> PostAsync(HttpClient httpClient, string endpoint, StringContent content)
        => httpClient.PostAsync(endpoint, content);

    /// <summary>
    /// Created only for mocking purposes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected virtual Task<HttpResponseMessage> PatchAsync(HttpClient httpClient, string endpoint, StringContent content)
        => httpClient.PatchAsync(endpoint, content);

    /// <summary>
    /// Created only for mocking purposes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    protected virtual Task<HttpResponseMessage> PutAsync(HttpClient httpClient, string endpoint, StringContent content)
        => httpClient.PutAsync(endpoint, content);
}