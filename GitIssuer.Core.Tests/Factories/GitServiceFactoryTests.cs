using GitIssuer.Core.Factories;
using GitIssuer.Core.Services;
using GitIssuer.Core.Services.Bases.Interfaces;
using Moq;

namespace GitIssuer.Core.Tests.Factories;

public class GitServiceFactoryTests
{
    [TestCase("GitHub", TestName = "GetService_GitProviderNameEqualsGitHub_ReturnsGitHubService")]
    [TestCase("GitLab", TestName = "GetService_GitProviderNameEqualsGitLab_ReturnsGitLabService")]
    public void GetServiceTest(string gitProviderName)
    {
        var gitHubService = new GitHubService(new Mock<IHttpClientFactory>().Object, string.Empty);
        var gitLabService = new GitLabService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var serviceProviderMock = new Mock<IServiceProvider>();
        serviceProviderMock
            .Setup(serviceProvider => serviceProvider.GetService(typeof(GitHubService)))
            .Returns(gitHubService);
        serviceProviderMock
            .Setup(serviceProvider => serviceProvider.GetService(typeof(GitLabService)))
            .Returns(gitLabService);

        var testedService = new GitServiceFactory(serviceProviderMock.Object);

        var actualService = testedService.GetService(gitProviderName);

        var expectedMap = new Dictionary<string, IGitService>
        {
            { "GitHub", gitHubService },
            { "GitLab", gitLabService }
        };

        Assert.That(actualService, Is.EqualTo(expectedMap[gitProviderName]));
    }

    [TestCase(TestName = "GetService_NotSupportedGitProviderName_ThrowsNotSupportedException")]
    public void GetServiceExceptionTest()
    {
        const string gitProviderName = "SomeGit";

        var testedService = new GitServiceFactory(new Mock<IServiceProvider>().Object);

        void ActionUnderTest()
            => testedService.GetService(gitProviderName);

        Assert.Multiple(() =>
        {
            var actualException = Assert.Throws<NotSupportedException>(ActionUnderTest);
            Assert.That(actualException!.Message, Is.EqualTo($"Provider '{gitProviderName}' is not supported."));
        });
    }
}