using GitIssuer.Core.Services.Bases;
using GitIssuer.Core.Tests.FakeObjects;
using Moq;
using Moq.Protected;

namespace GitIssuer.Core.Tests.Factories.Bases;

public class GitServiceBaseTests
{
    [TestCase(TestName = "AddIssueAsync_Invoked_ReturnsExpectedResponse")]
    public async Task AddIssueAsyncTest()
    {
        const string repositoryOwner = "repositoryOwner";
        const string repositoryName = "repositoryName";
        const string issueName = "issueName";
        const string issueDescription = "issueDescription";
        var addIssueRequestBody = new
        {
            FakeTitle = "fakeTitle"
        };
        const string issuesUrl = "issuesUrl";

        const string expectedResponse = "Mocked Response";

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object)
        {
            CallBase = true
        }; 

        testedServiceBaseMock
            .Protected()
            .Setup<object>("CreateAddIssueRequestBody", issueName, issueDescription)
            .Returns(addIssueRequestBody);

        testedServiceBaseMock
            .Protected()
            .Setup<string>("GetBaseIssuesUrl", repositoryOwner, repositoryName)
            .Returns(issuesUrl);

        testedServiceBaseMock
            .Protected()
            .Setup<Task<string>>("SendIssueRequestAsync", issuesUrl, addIssueRequestBody, HttpMethod.Post)
            .ReturnsAsync(expectedResponse);

        var actualResponse = await testedServiceBaseMock.Object.AddIssueAsync(repositoryOwner, repositoryName, issueName, issueDescription);

        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }

    [TestCase(TestName = "ModifyIssueAsync_Invoked_ReturnsExpectedResponse")]
    public async Task ModifyIssueAsyncTest()
    {
        const string repositoryOwner = "repositoryOwner";
        const string repositoryName = "repositoryName";
        const int issueId = 1;
        const string issueName = "issueName";
        const string issueDescription = "issueDescription";
        var modifyIssueRequestBody = new
        {
            FakeTitle = "fakeTitle"
        };
        const string baseIssuesUrl = "issuesUrl";
        var httpMethod = HttpMethod.Head;

        var expectedIssueUrl = $"{baseIssuesUrl}/{issueId}";
        const string expectedResponse = "Mocked Response";

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object)
        {
            CallBase = true
        };

        testedServiceBaseMock
            .Protected()
            .Setup<object>("CreateModifyIssueRequestBody", issueName, issueDescription)
            .Returns(modifyIssueRequestBody); 

        testedServiceBaseMock
            .Protected()
            .Setup<string>("GetBaseIssuesUrl", repositoryOwner, repositoryName)
            .Returns(baseIssuesUrl);

        testedServiceBaseMock
            .Protected()
            .Setup<HttpMethod>("GetModifyIssueHttpMethod")
            .Returns(httpMethod);

        testedServiceBaseMock
            .Protected()
            .Setup<Task<string>>("SendIssueRequestAsync", expectedIssueUrl, modifyIssueRequestBody, httpMethod)
            .ReturnsAsync(expectedResponse);

        var actualResponse = await testedServiceBaseMock.Object.ModifyIssueAsync(repositoryOwner, repositoryName, issueId, issueName, issueDescription);

        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }

    [TestCase(TestName = "CloseIssueAsync_Invoked_ReturnsExpectedResponse")]
    public async Task CloseIssueAsyncTest()
    {
        const string repositoryOwner = "repositoryOwner";
        const string repositoryName = "repositoryName";
        const int issueId = 1;
        var closeIssueRequestBody = new
        {
            Close = "yes"
        };
        const string baseIssuesUrl = "issuesUrl";
        var httpMethod = HttpMethod.Head;

        var expectedIssueUrl = $"{baseIssuesUrl}/{issueId}";
        const string expectedResponse = "Mocked Response";

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object)
        {
            CallBase = true
        };

        testedServiceBaseMock
            .Protected()
            .Setup<object>("CreateCloseIssueRequestBody")
            .Returns(closeIssueRequestBody);

        testedServiceBaseMock
            .Protected()
            .Setup<string>("GetBaseIssuesUrl", repositoryOwner, repositoryName)
            .Returns(baseIssuesUrl);

        testedServiceBaseMock
            .Protected()
            .Setup<HttpMethod>("GetModifyIssueHttpMethod")
            .Returns(httpMethod);

        testedServiceBaseMock
            .Protected()
            .Setup<Task<string>>("SendIssueRequestAsync", expectedIssueUrl, closeIssueRequestBody, httpMethod)
            .ReturnsAsync(expectedResponse);

        var actualResponse = await testedServiceBaseMock.Object.CloseIssueAsync(repositoryOwner, repositoryName, issueId);

        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }
}