using GitIssuer.Core.Dto.Responses;
using GitIssuer.Core.Services.Bases;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using System.Web;

namespace GitIssuer.Core.Services;

public class GitLabService(IHttpClientFactory httpClientFactory) : GitServiceBase(httpClientFactory)
{
    public override async Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description)
    {
        const string personalAccessToken = "glpat-KGKv13Qz5juRXGf6vyos";
        const string apiUrl = "https://gitlab.com/api/v4/";

        var httpClient = httpClientFactory.CreateClient();
        
        httpClient.BaseAddress = new Uri(apiUrl);
        var requestBody = new
        {
            title = name,
            description = description
        };

        var jsonRequestBody = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", personalAccessToken);

        try
        {
            var url = $"projects/{Uri.EscapeDataString($"{repositoryOwner}/{repositoryName}")}/issues";
            var response = await httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<GitLabAddIssueResponseDto>(responseContent);

                return responseData!.WebUrl!;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(errorContent);
        }
        catch (HttpRequestException exception)
        {
            throw new HttpRequestException($"An error occurred while communicating with GitLab ({exception.Message}).");
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