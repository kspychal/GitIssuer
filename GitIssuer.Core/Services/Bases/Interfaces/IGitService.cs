namespace GitIssuer.Core.Services.Bases.Interfaces;

public interface IGitService
{
    public Task<string> AddIssue(string owner, string repo, string title, string body = null);
}