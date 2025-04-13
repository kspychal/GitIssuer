namespace GitIssuer.Core.Services.Bases.Interfaces;

public interface IGitService
{
    /// <summary>
    /// Adds a new issue to the specified repository.
    /// </summary>
    /// <param name="repositoryOwner">The owner of the repository.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="name">The title of the issue.</param>
    /// <param name="description">The description of the issue.</param>
    /// <returns>A task that represents the asynchronous operation, containing the URL of the created issue.</returns>
    public Task<string> AddIssueAsync(string repositoryOwner, string repositoryName, string name, string description);

    /// <summary>
    /// Modifies an existing issue in the specified repository.
    /// </summary>
    /// <param name="repositoryOwner">The owner of the repository.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="issueId">The ID of the issue to modify.</param>
    /// <param name="name">The updated title of the issue</param>
    /// <param name="description">The updated description of the issue</param>
    /// <returns>A task that represents the asynchronous operation, containing the URL of the modified issue.</returns>
    public Task<string> ModifyIssueAsync(string repositoryOwner, string repositoryName, int issueId, string? name, string? description);

    /// <summary>
    /// Closes an existing issue in the specified repository.
    /// </summary>
    /// <param name="repositoryOwner">The owner of the repository.</param>
    /// <param name="repositoryName">The name of the repository.</param>
    /// <param name="issueId">The ID of the issue to close.</param>
    /// <returns>A task that represents the asynchronous operation, containing the URL of the closed issue.</returns>
    public Task<string> CloseIssueAsync(string repositoryOwner, string repositoryName, int issueId);
}