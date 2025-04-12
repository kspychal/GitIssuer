using GitIssuer.Core.Dto.Responses;
using GitIssuer.Core.Services.Bases;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace GitIssuer.Core.Services;

public class GitHubService(IHttpClientFactory httpClientFactory) : GitServiceBase(httpClientFactory)
{
    public override async Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description)
    {
        const string personalAccessToken = "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";
        const string apiUrl = "https://api.github.com/";

        var httpClient = httpClientFactory.CreateClient();

        httpClient.BaseAddress = new Uri(apiUrl);
        var requestBody = new
        {
            title = name,
            body = description
        };

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("GitIssuer");
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        try
        {
            var url = $"repos/{repositoryOwner}/{repositoryName}/issues";
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<GitHubAddIssueResponseDto>(responseContent);

                return responseData!.HtmlUrl!;
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