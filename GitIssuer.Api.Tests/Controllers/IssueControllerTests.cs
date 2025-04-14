using GitIssuer.Api.Controllers;
using GitIssuer.Api.Models;
using GitIssuer.Core.Dto.Requests;
using GitIssuer.Core.Factories.Interfaces;
using GitIssuer.Core.Services.Bases.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System.Reflection;

namespace GitIssuer.Api.Tests.Controllers;

public class IssueControllerTests
{
    [TestCase(TestName = "AddIssue_Invoked_GitServiceActionExecutedAndReturnsResponse")]
    public async Task AddIssueTest()
    {
        const string gitProvider = "GitHub";
        const string repositoryOwner = "test-user";
        const string repositoryName = "test-repo";
        const string issueTitle = "title";
        const string issueDescription = "description";
        const string expectedUrl = "https://fake.url";

        var issueDto = new AddIssueRequestDto
        {
            Title = issueTitle,
            Description = issueDescription
        };

        var gitServiceMock = new Mock<IGitService>();
        gitServiceMock
            .Setup(service => service.AddIssueAsync(repositoryOwner, repositoryName, issueTitle, issueDescription))
            .ReturnsAsync(expectedUrl);

        var gitServiceFactoryMock = new Mock<IGitServiceFactory>();
        gitServiceFactoryMock
            .Setup(factory => factory.GetService(It.IsAny<string>()))
            .Returns(gitServiceMock.Object);

        var testedController = new IssueController(gitServiceFactoryMock.Object, new Mock<ILogger<IssueController>>().Object);

        var result = await testedController.AddIssue(gitProvider, repositoryOwner, repositoryName, issueDto);

        Assert.Multiple(() =>
        {
            gitServiceFactoryMock
                .Verify(factory => factory.GetService(gitProvider), Times.Once);

            gitServiceMock
                .Verify(service => service.AddIssueAsync(repositoryOwner, repositoryName, issueTitle, issueDescription), Times.Once);

            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(201));

            var responseBody = objectResult.Value as SuccessApiResponseBody;
            Assert.That(responseBody, Is.Not.Null);
            Assert.That(responseBody!.Url, Is.EqualTo(expectedUrl));
        });
    }

    [TestCase(TestName = "ModifyIssue_Invoked_GitServiceActionExecutedAndReturnsResponse")]
    public async Task ModifyIssueTest()
    {
        const string gitProvider = "GitHub";
        const string repositoryOwner = "test-user";
        const string repositoryName = "test-repo";
        const int issueId = 1;
        const string issueTitle = "title";
        const string issueDescription = "description";
        const string expectedUrl = "https://fake.url";

        var issueDto = new ModifyIssueRequestDto
        {
            Title = issueTitle,
            Description = issueDescription
        };

        var gitServiceMock = new Mock<IGitService>();
        gitServiceMock
            .Setup(service => service.ModifyIssueAsync(repositoryOwner, repositoryName, issueId, issueTitle, issueDescription))
            .ReturnsAsync(expectedUrl);

        var gitServiceFactoryMock = new Mock<IGitServiceFactory>();
        gitServiceFactoryMock
            .Setup(factory => factory.GetService(It.IsAny<string>()))
            .Returns(gitServiceMock.Object);

        var testedController = new IssueController(gitServiceFactoryMock.Object, new Mock<ILogger<IssueController>>().Object);

        var result = await testedController.ModifyIssue(gitProvider, repositoryOwner, repositoryName, issueId, issueDto);

        Assert.Multiple(() =>
        {
            gitServiceFactoryMock
                .Verify(factory => factory.GetService(gitProvider), Times.Once);

            gitServiceMock
                .Verify(service => service.ModifyIssueAsync(repositoryOwner, repositoryName, issueId, issueTitle, issueDescription), Times.Once);

            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(200));

            var responseBody = objectResult.Value as SuccessApiResponseBody;
            Assert.That(responseBody, Is.Not.Null);
            Assert.That(responseBody!.Url, Is.EqualTo(expectedUrl));
        });
    }

    [TestCase(TestName = "CloseIssue_Invoked_GitServiceActionExecutedAndReturnsResponse")]
    public async Task CloseIssueTest()
    {
        const string gitProvider = "GitHub";
        const string repositoryOwner = "test-user";
        const string repositoryName = "test-repo";
        const int issueId = 1;
        const string expectedUrl = "https://fake.url";

        var gitServiceMock = new Mock<IGitService>();
        gitServiceMock
            .Setup(service => service.CloseIssueAsync(repositoryOwner, repositoryName, issueId))
            .ReturnsAsync(expectedUrl);

        var gitServiceFactoryMock = new Mock<IGitServiceFactory>();
        gitServiceFactoryMock
            .Setup(factory => factory.GetService(It.IsAny<string>()))
            .Returns(gitServiceMock.Object);

        var testedController = new IssueController(gitServiceFactoryMock.Object, new Mock<ILogger<IssueController>>().Object);

        var result = await testedController.CloseIssue(gitProvider, repositoryOwner, repositoryName, issueId);

        Assert.Multiple(() =>
        {
            gitServiceFactoryMock
                .Verify(factory => factory.GetService(gitProvider), Times.Once);

            gitServiceMock
                .Verify(service => service.CloseIssueAsync(repositoryOwner, repositoryName, issueId), Times.Once);

            var objectResult = result as ObjectResult;
            Assert.That(objectResult, Is.Not.Null);
            Assert.That(objectResult!.StatusCode, Is.EqualTo(200));

            var responseBody = objectResult.Value as SuccessApiResponseBody;
            Assert.That(responseBody, Is.Not.Null);
            Assert.That(responseBody!.Url, Is.EqualTo(expectedUrl));
        });
    }

    [TestCase(TestName = "ExecuteGitServiceActionAsync_Invoked_GitServiceActionInvoked")]
    public async Task ExecuteGitServiceActionAsyncTest()
    {
        const string providerName = "providerName";
        const string expectedIssueUrl = "https://some.example/123";
        IActionResult expectedResponse = new OkObjectResult(new { IssueUrl = expectedIssueUrl });
        Func<IGitService, Task<string>> gitServiceAction = _ => Task.FromResult(expectedIssueUrl);
        Func<string, IActionResult> createSuccessResponseAction = (url) => new OkObjectResult(new { IssueUrl = url });

        var loggerMock = new Mock<ILogger<IssueController>>();

        var testedControllerMock = new Mock<IssueController>(new Mock<IGitServiceFactory>().Object, loggerMock.Object)
        {
            CallBase = true
        };

        var gitServiceMock = new Mock<IGitService>();

        testedControllerMock
            .Protected()
            .Setup<(IGitService?, IActionResult?)>("TryGetGitService", providerName)
            .Returns((gitServiceMock.Object, null));

        var actualResponse = await (Task<IActionResult>)(typeof(IssueController)
            .GetMethod("ExecuteGitServiceActionAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedControllerMock.Object, [providerName, gitServiceAction, createSuccessResponseAction])!)
            as OkObjectResult;

        Assert.Multiple(() =>
        {
            Assert.That(actualResponse!.StatusCode, Is.EqualTo(200));

            var responseValue = actualResponse.Value as dynamic;
            Assert.That(responseValue?.IssueUrl, Is.EqualTo(expectedIssueUrl));

            loggerMock.Verify(logger => logger.Log(It.Is<LogLevel>(l => l == LogLevel.Information), It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"Successfully handled {expectedIssueUrl} request.")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        });
    }
}