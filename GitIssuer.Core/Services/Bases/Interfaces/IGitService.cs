namespace GitIssuer.Core.Services.Bases.Interfaces;

public interface IGitService
{
    public Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description);
    public Task<string> ModifyIssueAsync(string repositoryOwner, string repositoryName, int issueNumber, string? name, string? description);
    public Task<string> CloseIssueAsync(string repositoryOwner, string repositoryName, int issueId);
}