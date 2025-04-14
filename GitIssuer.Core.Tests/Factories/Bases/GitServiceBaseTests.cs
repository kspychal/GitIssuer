using GitIssuer.Core.Services.Bases;
using GitIssuer.Core.Tests.FakeObjects;
using Moq;
using Moq.Protected;
using System.Xml.Linq;

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
            .Setup<string>("GetIssuesUrl", repositoryOwner, repositoryName)
            .Returns(issuesUrl);

        testedServiceBaseMock
            .Protected()
            .Setup<Task<string>>("SendIssueRequestAsync", issuesUrl, addIssueRequestBody, HttpMethod.Post)
            .ReturnsAsync(expectedResponse);

        var actualResponse = await testedServiceBaseMock.Object.AddIssueAsync(repositoryOwner, repositoryName, issueName, issueDescription);

        Assert.That(actualResponse, Is.EqualTo(expectedResponse));
    }
}