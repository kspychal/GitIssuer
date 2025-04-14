namespace GitIssuer.Core.Configuration;

public record GitTokensConfiguration
{
    public string GitHubToken { get; set; } = string.Empty;
    public string GitLabToken { get; set; } = string.Empty;
}