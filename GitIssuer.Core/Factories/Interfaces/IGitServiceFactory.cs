using GitIssuer.Core.Services.Bases.Interfaces;

namespace GitIssuer.Core.Factories.Interfaces;

public interface IGitServiceFactory
{
    public IGitService? GetService(string platform);
    public IEnumerable<string> GetValidGitProviderNames();
}