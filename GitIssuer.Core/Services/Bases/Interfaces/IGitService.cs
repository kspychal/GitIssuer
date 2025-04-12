namespace GitIssuer.Core.Services.Bases.Interfaces;

public interface IGitService
{
    public Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description);
}