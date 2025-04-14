using GitIssuer.Core.Services;
using Moq;
using System.Reflection;

namespace GitIssuer.Core.Tests.Services;

public class GitLabServiceTests
{
    [TestCase(TestName = "Constructor_Invoked_PropertiesInitialized")]
    public void ConstructorTest()
    {
        const string expectedApiBaseUrl = "https://gitlab.com/api/v4/";
        const string expectedProviderName = "GitLab";

        var testedService = new GitLabService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualApiBaseUrl = typeof(GitLabService)
            .GetProperty("ApiBaseUrl", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(testedService);

        var actualProviderName = typeof(GitLabService)
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
        var testedService = new GitLabService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualRequestBody = typeof(GitLabService)
            .GetMethod("CreateAddIssueRequestBody", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [expectedTitle, expectedDescription])!;

        var actualTitleProperty = actualRequestBody.GetType().GetProperty("title");
        var actualDescriptionProperty = actualRequestBody.GetType().GetProperty("description");

        Assert.Multiple(() =>
        {
            Assert.That(actualRequestBody, Is.Not.Null);

            Assert.That(actualTitleProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedTitle));
            Assert.That(actualDescriptionProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedDescription));
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
        var testedService = new GitLabService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualRequestBody = typeof(GitLabService)
            .GetMethod("CreateModifyIssueRequestBody", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [expectedTitle, expectedDescription])!;

        var actualTitleProperty = actualRequestBody.GetType().GetProperty("title");
        var actualDescriptionProperty = actualRequestBody.GetType().GetProperty("description");

        Assert.Multiple(() =>
        {
            Assert.That(actualRequestBody, Is.Not.Null);

            Assert.That(actualTitleProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedTitle));
            Assert.That(actualDescriptionProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedDescription));
        });
    }

    [TestCase(TestName = "CreateCloseIssueRequestBody_Invoked_ReturnsRequestBody")]
    public void CreateCloseIssueRequestBodyTest()
    {
        const string expectedStateEventProperty = "close";

        var testedService = new GitLabService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualRequestBody = typeof(GitLabService)
            .GetMethod("CreateCloseIssueRequestBody", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [])!;

        var actualStateEventProperty = actualRequestBody.GetType().GetProperty("state_event");

        Assert.Multiple(() =>
        {
            Assert.That(actualRequestBody, Is.Not.Null);
            Assert.That(actualStateEventProperty?.GetValue(actualRequestBody), Is.EqualTo(expectedStateEventProperty));
        });
    }

    [TestCase(TestName = "GetModifyIssueHttpMethod_Invoked_ReturnsHttpMethodPut")]
    public void GetModifyIssueHttpMethodTest()
    {
        var testedService = new GitLabService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualModifyIssueHttpMethod = typeof(GitLabService)
            .GetMethod("GetModifyIssueHttpMethod", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, []);

        Assert.That(actualModifyIssueHttpMethod, Is.EqualTo(HttpMethod.Put));
    }

    [TestCase(TestName = "BuildIssuesApiUrl_Invoked_ReturnsCorrectUrl")]
    public void BuildIssuesApiUrlTest()
    {
        const string repositoryOwner = "owner";
        const string repositoryName = "name";
        const string expectedIssuesApiUrl = $"projects/{repositoryOwner}%2F{repositoryName}/issues";

        var testedService = new GitLabService(new Mock<IHttpClientFactory>().Object, string.Empty);

        var actualIssuesApiUrl = typeof(GitLabService)
            .GetMethod("BuildIssuesApiUrl", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedService, [repositoryOwner, repositoryName]);

        Assert.That(actualIssuesApiUrl, Is.EqualTo(expectedIssuesApiUrl));
    }
}