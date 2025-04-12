using GitIssuer.Core.Services.Bases.Interfaces;

namespace GitIssuer.Core.Services.Bases;

public abstract class GitServiceBase(IHttpClientFactory httpClientFactory) : IGitService
{
    public abstract Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description);
}