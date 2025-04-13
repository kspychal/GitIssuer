using GitIssuer.Core.Services.Bases.Interfaces;

namespace GitIssuer.Core.Factories.Interfaces;

public interface IGitServiceFactory
{
    /// <summary>
    /// Gets an instance of the appropriate <see cref="IGitService"/> based on the specified Git provider name.
    /// </summary>
    /// <param name="gitProviderName">The name of the Git provider (e.g., "GitHub", "GitLab").</param>
    /// <returns>An instance of <see cref="IGitService"/> for the specified provider, or throws a <see cref="NotSupportedException"/> if the provider is not supported.</returns>
    public IGitService? GetService(string gitProviderName);

    /// <summary>
    /// Returns a list of valid Git provider names that <see cref="IGitServiceFactory"/> supports.
    /// </summary>
    /// <returns>A collection of valid Git provider names.</returns>
    public IEnumerable<string> GetValidGitProviderNames();
}