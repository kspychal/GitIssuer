using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services;
using GitIssuer.Core.Services.Bases.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace GitIssuer.Core.Factories;

public class GitServiceFactory(IServiceProvider provider) : IGitServiceFactory
{
    private const string GitHubPlatform = "github";
    private const string OtherPlatform = "other";

    private static readonly string[] ValidGitProviderNames = [GitHubPlatform];

    public IGitService? GetService(string gitProviderName)
        => gitProviderName.ToLower() switch
        {
            GitHubPlatform => provider.GetService<GitHubService>(),
            OtherPlatform => provider.GetService<GitHubService>(), 
            _ => throw new NotSupportedException($"Platform '{gitProviderName}' is not supported.")
        };

    public IEnumerable<string> GetValidGitProviderNames() => ValidGitProviderNames;
}