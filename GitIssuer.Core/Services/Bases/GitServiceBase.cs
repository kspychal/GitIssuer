using GitIssuer.Core.Services.Bases.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GitIssuer.Core.Services.Bases;

public abstract class GitServiceBase<TResponse>(IHttpClientFactory httpClientFactory) : IGitService
{
    public abstract string PersonalAccessToken { get; }
    public abstract string ApiUrl { get; }
    public abstract string ProviderName { get; }

    public async Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description)
    {
        var httpClient = httpClientFactory.CreateClient();

        httpClient.BaseAddress = new Uri(ApiUrl);
        var requestBody = CreateAddIssueRequestBody(name, description);

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PersonalAccessToken);
        AddCustomRequestHeaders(httpClient);

        try
        {
            var issuesUrl = GetIssuesUrl(repositoryOwner, repositoryName);
            var response = await httpClient.PostAsync(issuesUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<TResponse>(responseContent);

                return ExtractUrl(responseData!);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorContent);
        }
        catch (HttpRequestException exception)
        {
            throw new HttpRequestException($"An error occurred while communicating with {ProviderName} ({exception.Message}).");
        }
        catch (JsonException exception)
        {
            throw new JsonException($"An error occurred while processing JSON data ({exception.Message}).");
        }
        catch (Exception exception)
        {
            throw new Exception($"An unexpected error occurred ({exception.Message}).");
        }
    }

    public async Task<string> ModifyIssueAsync(string repositoryOwner, string repositoryName, int issueId, string? name, string? description)
    {
        var httpClient = httpClientFactory.CreateClient();

        httpClient.BaseAddress = new Uri(ApiUrl);
        var requestBody = CreateModifyIssueRequestBody(name, description);

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", PersonalAccessToken);
        AddCustomRequestHeaders(httpClient);

        try
        {
            var issueUrl = $"{GetIssuesUrl(repositoryOwner, repositoryName)}/{issueId}";
            var response = await SendIssueUpdateRequestAsync(httpClient, issueUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<TResponse>(responseContent);

                return ExtractUrl(responseData!);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorContent);
        }
        catch (HttpRequestException exception)
        {
            throw new HttpRequestException($"An error occurred while communicating with {ProviderName} ({exception.Message}).");
        }
        catch (JsonException exception)
        {
            throw new JsonException($"An error occurred while processing JSON data ({exception.Message}).");
        }
        catch (Exception exception)
        {
            throw new Exception($"An unexpected error occurred ({exception.Message}).");
        }
    }


    public abstract object CreateAddIssueRequestBody(string name, string description);
    public abstract object CreateModifyIssueRequestBody(string? name, string? description);
    
    public abstract void AddCustomRequestHeaders(HttpClient httpClient);

    public abstract Task<HttpResponseMessage> SendIssueUpdateRequestAsync(HttpClient httpClient, string issueUrl, StringContent content);

    public abstract string GetIssuesUrl(string repositoryOwner, string repositoryName);
    protected abstract string ExtractUrl(TResponse response);
}