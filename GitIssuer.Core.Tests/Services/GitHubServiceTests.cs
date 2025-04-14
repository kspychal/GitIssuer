using GitIssuer.Core.Services;
using Moq;
using System.Reflection;

namespace GitIssuer.Core.Tests.Services;

public class GitHubServiceTests
{
    [TestCase(TestName = "Constructor_Invoked_PropertiesInitialized")]

    public void ConstructorTest()
    {
        const string expectedApiBaseUrl = "https://api.github.com/";
        const string expectedProviderName = "GitHub";

        var testedService = new GitHubService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualApiBaseUrl = typeof(GitHubService)
            .GetProperty("ApiBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(testedService);

        var actualProviderName = typeof(GitHubService)
            .GetProperty("ProviderName", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(testedService);

        Assert.Multiple(() =>
        {
            Assert.That(actualApiBaseUrl, Is.EqualTo(expectedApiBaseUrl));
            Assert.That(actualProviderName, Is.EqualTo(expectedProviderName));
        });
    }

    [TestCase("Title", "Description", TestName = "CreateAddIssueRequestBody_NotEmptyTitleAndDescription_ReturnsRequestBody")]
    [TestCase("Title", "", TestName = "CreateAddIssueRequestBody_EmptyDescription_ReturnsRequestBodyWithEmptyDescription")]
    [TestCase("", "Description", TestName = "CreateAddIssueRequestBody_EmptyTitle_ReturnsRequestBodyWithEmptyTitle")]
    public void CreateAddIssueRequestBodyTest(string expectedTitle, string expectedDescription)
    {
        var testedService = new GitHubService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualRequestBody = typeof(GitHubService)
            .GetMethod("CreateAddIssueRequestBody", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [expectedTitle, expectedDescription])!;

        var actualTitleProperty = actualRequestBody.GetType().GetProperty("title");
        var actualBodyProperty = actualRequestBody.GetType().GetProperty("body");

        Assert.Multiple(() =>
        {
            Assert.That(actualRequestBody, Is.Not.Null);

            Assert.That(actualTitleProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedTitle));
            Assert.That(actualBodyProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedDescription));
        });
    }

    [TestCase("Title", "Description", TestName = "CreateModifyIssueRequestBody_NotEmptyTitleAndDescription_ReturnsRequestBody")]
    [TestCase("Title", null, TestName = "CreateModifyIssueRequestBody_NullDescription_ReturnsRequestBodyWithNullDescription")]
    [TestCase(null, "Description", TestName = "CreateModifyIssueRequestBody_NullTitle_ReturnsRequestBodyWithNullTitle")]
    [TestCase(null, null, TestName = "CreateModifyIssueRequestBody_NullTitleAndDescription_ReturnsRequestBodyWithNullTitleAndDescription")]
    [TestCase("Title", "", TestName = "CreateModifyIssueRequestBody_EmptyDescription_ReturnsRequestBodyWithEmptyDescription")]
    [TestCase("", "Description", TestName = "CreateModifyIssueRequestBody_EmptyTitle_ReturnsRequestBodyWithEmptyTitle")]
    [TestCase("", "", TestName = "CreateModifyIssueRequestBody_EmptyTitleAndDescription_ReturnsRequestBodyWithEmptyTitleAndDescription")]
    public void CreateModifyIssueRequestBodyTest(string? expectedTitle, string? expectedDescription)
    {
        var testedService = new GitHubService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualRequestBody = typeof(GitHubService)
            .GetMethod("CreateModifyIssueRequestBody", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [expectedTitle, expectedDescription])!;

        var actualTitleProperty = actualRequestBody.GetType().GetProperty("title");
        var actualBodyProperty = actualRequestBody.GetType().GetProperty("body");

        Assert.Multiple(() =>
        {
            Assert.That(actualRequestBody, Is.Not.Null);

            Assert.That(actualTitleProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedTitle));
            Assert.That(actualBodyProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedDescription));
        });
    }

    [TestCase(TestName = "CreateCloseIssueRequestBody_Invoked_ReturnsRequestBody")]
    public void CreateCloseIssueRequestBodyTest()
    {
        const string expectedStateProperty = "closed";

        var testedService = new GitHubService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualRequestBody = typeof(GitHubService)
            .GetMethod("CreateCloseIssueRequestBody", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [])!;

        var actualStateProperty = actualRequestBody.GetType().GetProperty("state");

        Assert.Multiple(() =>
        {
            Assert.That(actualRequestBody, Is.Not.Null);
            Assert.That(actualStateProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedStateProperty));
        });
    }

    [TestCase(TestName = "AddCustomRequestHeaders_Invoked_AddedRequestHeadersToHttpClient")]
    public void AddCustomRequestHeadersTest()
    {
        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(httpClient);

        var testedService = new GitHubService(httpClientFactoryMock.Object, string.Empty);

        typeof(GitHubService)
            .GetMethod("AddCustomRequestHeaders", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [httpClient]);

        Assert.Multiple(() =>
        {
            Assert.That(httpClient.DefaultRequestHeaders.Accept.Count, Is.EqualTo(1));
            Assert.That(httpClient.DefaultRequestHeaders.Accept.First().MediaType, Is.EqualTo("application/vnd.github+json"));

            Assert.That(httpClient.DefaultRequestHeaders.UserAgent.ToString(), Contains.Substring("GitIssuer"));

            Assert.That(httpClient.DefaultRequestHeaders.Contains("X-GitHub-Api-Version"), Is.True);
            Assert.That(httpClient.DefaultRequestHeaders.GetValues("X-GitHub-Api-Version").First(), Is.EqualTo("2022-11-28"));
        });
    }

    [TestCase(TestName = "GetModifyIssueHttpMethod_Invoked_ReturnsHttpMethodPatch")]
    public void GetModifyIssueHttpMethodTest()
    {
        var testedService = new GitHubService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualModifyIssueHttpMethod = typeof(GitHubService)
            .GetMethod("GetModifyIssueHttpMethod", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, []);

        Assert.That(actualModifyIssueHttpMethod, Is.EqualTo(HttpMethod.Patch));
    }

    [TestCase(TestName = "BuildIssuesApiUrl_Invoked_ReturnsCorrectUrl")]
    public void BuildIssuesApiUrlTest()
    {
        const string repositoryOwner = "owner";
        const string repositoryName = "name";
        const string expectedIssuesApiUrl = $"repos/{repositoryOwner}/{repositoryName}/issues";

        var testedService = new GitHubService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualIssuesApiUrl = typeof(GitHubService)
            .GetMethod("BuildIssuesApiUrl", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [repositoryOwner, repositoryName]);

        Assert.That(actualIssuesApiUrl, Is.EqualTo(expectedIssuesApiUrl));
    }
}