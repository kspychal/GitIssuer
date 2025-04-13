﻿using GitIssuer.Core.Services.Bases.Interfaces;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GitIssuer.Core.Exceptions;

namespace GitIssuer.Core.Services.Bases;

public abstract class GitServiceBase<TResponse>(IHttpClientFactory httpClientFactory) : IGitService
{
    public abstract string PersonalAccessToken { get; }
    public abstract string ApiUrl { get; }
    public abstract string ProviderName { get; }

    public async Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description)
    {
        var requestBody = CreateAddIssueRequestBody(name, description);
        var issuesUrl = GetIssuesUrl(repositoryOwner, repositoryName);
        return await SendIssueRequestAsync(issuesUrl, requestBody, HttpMethod.Post);
    }

    public async Task<string> ModifyIssueAsync(string repositoryOwner, string repositoryName, int issueId, string? name, string? description)
    {
        var requestBody = CreateModifyIssueRequestBody(name, description);
        var issueUrl = $"{GetIssuesUrl(repositoryOwner, repositoryName)}/{issueId}";
        var httpMethod = GetModifyIssueHttpMethod(); 
        return await SendIssueRequestAsync(issueUrl, requestBody, httpMethod);
    }

    public async Task<string> CloseIssueAsync(string repositoryOwner, string repositoryName, int issueId)
    {
        var requestBody = CreateCloseIssueRequestBody();
        var issueUrl = $"{GetIssuesUrl(repositoryOwner, repositoryName)}/{issueId}";
        var httpMethod = GetModifyIssueHttpMethod();
        return await SendIssueRequestAsync(issueUrl, requestBody, httpMethod);
    }

    private async Task<string> SendIssueRequestAsync(string url, object requestBody, HttpMethod method)
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

    public abstract object CreateAddIssueRequestBody(string name, string description);
    public abstract object CreateModifyIssueRequestBody(string? name, string? description);
    public abstract object CreateCloseIssueRequestBody();

    public abstract void AddCustomRequestHeaders(HttpClient httpClient);

    public abstract HttpMethod GetModifyIssueHttpMethod();

    public abstract string GetIssuesUrl(string repositoryOwner, string repositoryName);
    protected abstract string ExtractUrl(TResponse response);
}