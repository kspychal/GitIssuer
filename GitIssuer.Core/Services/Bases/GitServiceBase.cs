using GitIssuer.Core.Services.Bases.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace GitIssuer.Core.Services.Bases;

public abstract class GitServiceBase(IHttpClientFactory httpClientFactory) : IGitService
{
    public async Task<string> AddIssue(string owner, string repo, string title, string body = null)
    {
        var httpClient = httpClientFactory.CreateClient();
        var personalAccessToken = "xx";
        var apiUrl = $"https://api.github.com/repos/{owner}/{repo}/issues";

        var requestBody = new
        {
            title = title,
            body = body
        };

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("GitIssuer");
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        try
        {
            var response = await httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return responseContent;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorContent);
        }
        catch (HttpRequestException exception)
        {
            throw new HttpRequestException($"An error occurred while communicating with GitHub ({exception.Message}).");
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
}