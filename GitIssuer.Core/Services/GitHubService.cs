using GitIssuer.Core.Services.Bases;

namespace GitIssuer.Core.Services;

public class GitHubService(IHttpClientFactory httpClientFactory) : GitServiceBase(httpClientFactory);