using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services;
using GitIssuer.Core.Services.Bases.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GitIssuer.Core.Factories;

public class GitServiceFactory(IServiceProvider serviceProvider) : IGitServiceFactory
{
    private const string GitHub = "github";
    private const string GitLab = "gitlab";

    private static readonly string[] ValidGitProviderNames = [GitHub, GitLab];

    public IGitService? GetService(string gitProviderName)
        => gitProviderName.ToLower() switch
        {
            GitHub => serviceProvider.GetService<GitHubService>(),
            GitLab => serviceProvider.GetService<GitLabService>(), 
            _ => throw new NotSupportedException($"Provider '{gitProviderName}' is not supported.")
        };

    public IEnumerable<string> GetValidGitProviderNames() => ValidGitProviderNames;
}