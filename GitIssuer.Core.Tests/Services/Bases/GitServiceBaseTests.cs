using GitIssuer.Core.Exceptions;
using GitIssuer.Core.Services.Bases;
using GitIssuer.Core.Tests.FakeObjects;
using Moq;
using Moq.Protected;
using System.Net;
using System.Reflection;
using IHttpClientFactory = System.Net.Http.IHttpClientFactory;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GitIssuer.Core.Tests.Factories.Bases;

public class GitServiceBaseTests
{
    [TestCase(TestName = "Constructor_Invoked_PropertiesInitialized")]
    public void ConstructorTest()
    {
        const string expectedPersonalAccessToken = "SomePat";
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(httpClientFactoryMock.Object, expectedPersonalAccessToken)
        {
            CallBase = true
        };

        var actualPersonalAccessToken = typeof(GitServiceBase<FakeResponseDto>)
            .GetProperty("PersonalAccessToken", BindingFlags.NonPublic | BindingFlags.Instance)?
            .GetValue(testedServiceBaseMock.Object);

        Assert.That(actualPersonalAccessToken, Is.EqualTo(expectedPersonalAccessToken));
    }

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

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object, string.Empty)
        {
            CallBase = true
        }; 

        testedServiceBaseMock
            .Protected()
            .Setup<object>("CreateAddIssueRequestBody", issueName, issueDescription)
            .Returns(addIssueRequestBody);

        testedServiceBaseMock
            .Protected()
            .Setup<string>("BuildIssuesApiUrl", repositoryOwner, repositoryName)
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

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object, string.Empty)
        {
            CallBase = true
        };

        testedServiceBaseMock
            .Protected()
            .Setup<object>("CreateModifyIssueRequestBody", issueName, issueDescription)
            .Returns(modifyIssueRequestBody); 

        testedServiceBaseMock
            .Protected()
            .Setup<string>("BuildIssuesApiUrl", repositoryOwner, repositoryName)
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

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object, string.Empty)
        {
            CallBase = true
        };

        testedServiceBaseMock
            .Protected()
            .Setup<object>("CreateCloseIssueRequestBody")
            .Returns(closeIssueRequestBody);

        testedServiceBaseMock
            .Protected()
            .Setup<string>("BuildIssuesApiUrl", repositoryOwner, repositoryName)
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

    private static readonly object[] SendIssueRequestAsyncSource =
    [
        new TestCaseData(HttpMethod.Post).SetName("SendIssueRequestAsync_PostMethod_ReturnsSuccessResponse"),
        new TestCaseData(HttpMethod.Patch).SetName("SendIssueRequestAsync_PatchMethod_ReturnsSuccessResponse"),
        new TestCaseData(HttpMethod.Put).SetName("SendIssueRequestAsync_PutMethod_ReturnsSuccessResponse")
    ];

    [TestCaseSource(nameof(SendIssueRequestAsyncSource))]
    public async Task SendIssueRequestAsyncTest(HttpMethod httpMethod)
    {
        const string personalAccessToken = "pat";
        const string endpoint = "endpoint";

        var requestBody = new
        {
            SomeProperty = "SomeValue"
        };
        var requestContent = new StringContent("\"SomeProperty\" = \"SomeValue\"");

        var responseDto = new FakeResponseDto
        {
            IssueUrl = "UrlFromGit"
        };

        const string expectedResponse = "\"SomeUrl\" = \"UrlFromGit\"";

        var httpClientMock = new Mock<HttpClient>(new Mock<HttpMessageHandler>().Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(httpClientMock.Object);

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(httpClientFactoryMock.Object, personalAccessToken)
        {
            CallBase = true
        };

        testedServiceBaseMock
            .Protected()
            .Setup<string>("ApiBaseUrl")
            .Returns("https://SomeApi.Url/");

        testedServiceBaseMock
            .Protected()
            .Setup<StringContent>("CreateJsonStringContent", requestBody)
            .Returns(requestContent);

        var expectedResponseFromGitProvider = new HttpResponseMessage(HttpStatusCode.OK);

        var methodName = httpMethod.Method;
        var methodNamePascalCase = char.ToUpper(methodName[0]) + methodName.Substring(1).ToLower();

        testedServiceBaseMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>($"{methodNamePascalCase}Async", httpClientMock.Object, endpoint, requestContent)
            .ReturnsAsync(expectedResponseFromGitProvider);

        testedServiceBaseMock
            .Protected()
            .Setup<Task<FakeResponseDto>>("DeserializeResponseAsync", expectedResponseFromGitProvider)  
            .ReturnsAsync(responseDto);

        testedServiceBaseMock
            .Protected()
            .Setup<string>("GetIssueUrlFromResponse", responseDto)
            .Returns(expectedResponse);

        var actualResponse = await (Task<string>)typeof(GitServiceBase<FakeResponseDto>)
            .GetMethod("SendIssueRequestAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedServiceBaseMock.Object, [endpoint, requestBody, httpMethod])!;

        Assert.Multiple(() =>
        {
            Assert.That(actualResponse, Is.EqualTo(expectedResponse));

            testedServiceBaseMock
                .Protected()
                .Verify("AddCustomRequestHeaders", Times.Once(), httpClientMock.Object);

            Assert.That(httpClientMock.Object.DefaultRequestHeaders.Authorization!.Scheme, Is.EqualTo("Bearer"));
            Assert.That(httpClientMock.Object.DefaultRequestHeaders.Authorization.Parameter, Is.EqualTo("pat"));
        });
    }

    private static readonly object[] SendIssueRequestAsyncExceptionSource =
    [
        new TestCaseData(HttpMethod.Post).SetName("SendIssueRequestAsync_PostMethod_ThrowsGitException"),
        new TestCaseData(HttpMethod.Patch).SetName("SendIssueRequestAsync_PatchMethod_ThrowsGitException"),
        new TestCaseData(HttpMethod.Put).SetName("SendIssueRequestAsync_PutMethod_ThrowsGitException")
    ];

    [TestCaseSource(nameof(SendIssueRequestAsyncExceptionSource))]
    public void SendIssueRequestAsyncGitExceptionTest(HttpMethod httpMethod)
    {
        const string endpoint = "endpoint";

        var requestBody = new
        {
            SomeProperty = "SomeValue"
        };
        var requestContent = new StringContent("\"SomeProperty\" = \"SomeValue\"");

        var httpClientMock = new Mock<HttpClient>(new Mock<HttpMessageHandler>().Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(httpClientMock.Object);

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(httpClientFactoryMock.Object, string.Empty)
        {
            CallBase = true
        };

        testedServiceBaseMock
            .Protected()
            .Setup<string>("ApiBaseUrl")
            .Returns("https://SomeApi.Url/");

        testedServiceBaseMock
            .Protected()
            .Setup<StringContent>("CreateJsonStringContent", requestBody)
            .Returns(requestContent);

        const string providerName = "GitX";
        testedServiceBaseMock
            .Protected()
            .Setup<string>("ProviderName")
            .Returns(providerName);

        const string expectedInnerMessage = "Something went wrong.";
        var expectedResponseFromGitProvider = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent(expectedInnerMessage)
        };

        var methodName = httpMethod.Method;
        var methodNamePascalCase = char.ToUpper(methodName[0]) + methodName.Substring(1).ToLower();

        testedServiceBaseMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>($"{methodNamePascalCase}Async", httpClientMock.Object, endpoint, requestContent)
            .ReturnsAsync(expectedResponseFromGitProvider);

        async Task ActionUnderTest() 
            => await (Task<string>)typeof(GitServiceBase<FakeResponseDto>)
                .GetMethod("SendIssueRequestAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(testedServiceBaseMock.Object, [endpoint, requestBody, httpMethod])!;

        
        Assert.Multiple(() =>
        {
            var actualException = Assert.ThrowsAsync<GitException>(() => _ = ActionUnderTest());
            Assert.That(actualException!.Message, Is.EqualTo($"{providerName} API responded with a non-success status code."));
            Assert.That(actualException.InnerMessage, Is.EqualTo(expectedInnerMessage));
        });
    }

    [TestCase(TestName = "SendIssueRequestAsync_GetMethod_ThrowsArgumentException")]
    public void SendIssueRequestAsyncArgumentExceptionTest()
    {
        var httpClientMock = new Mock<HttpClient>(new Mock<HttpMessageHandler>().Object);

        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(httpClientMock.Object);

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(httpClientFactoryMock.Object, string.Empty)
        {
            CallBase = true
        };

        testedServiceBaseMock
            .Protected()
            .Setup<string>("ApiBaseUrl")
            .Returns("https://SomeApi.Url/");

        const string endpoint = "endpoint";

        var requestBody = new
        {
            SomeProperty = "SomeValue"
        };

        async Task ActionUnderTest()
            => await (Task<string>)typeof(GitServiceBase<FakeResponseDto>)
                .GetMethod("SendIssueRequestAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
                .Invoke(testedServiceBaseMock.Object, [endpoint, requestBody, HttpMethod.Get])!;

        Assert.Multiple(() =>
        {
            var actualException = Assert.ThrowsAsync<ArgumentException>(() => _ = ActionUnderTest());
            Assert.That(actualException!.Message, Is.EqualTo($"Invalid HttpMethod. Only Post, Patch, and Put are supported."));
        });
    }

    [TestCase(TestName = "CreateJsonStringContent_WithBasicObject_ReturnsCorrectJson")]
    public void CreateJsonStringContentTest()
    {
        var requestBody = new
        {
            StringProperty = "Hello",
            IntProperty = 456,
            BoolProperty = true,
            DecimalProperty = 1.23m,
            NullProperty = (string)null
        };
        var expectedJson = JsonSerializer.Serialize(requestBody);

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object, string.Empty)
        {
            CallBase = true
        };

        var content = (StringContent)typeof(GitServiceBase<FakeResponseDto>)
            .GetMethod("CreateJsonStringContent", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedServiceBaseMock.Object, [requestBody])!;

        var actualJson = content.ReadAsStringAsync().Result;

        Assert.Multiple(() =>
        {
            Assert.That(content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
            Assert.That(content.Headers.ContentType?.CharSet, Is.EqualTo("utf-8"));

            Assert.That(actualJson, Is.EqualTo(expectedJson));
        });
    }

    [TestCase(TestName = "DeserializeResponseAsync_ValidResponse_ReturnsDeserializedResponse")]
    public async Task DeserializeResponseAsyncTest()
    {
        const string jsonResponse = "{\"IssueUrl\": \"UrlFromGit\"}";
        var response = new HttpResponseMessage
        {
            Content = new StringContent(jsonResponse)
        };
        var expectedDeserializedResponse = new FakeResponseDto
        {
            IssueUrl = "UrlFromGit"
        };

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object, string.Empty)
        {
            CallBase = true
        };

        var deserializedResponse = await (Task<FakeResponseDto>)typeof(GitServiceBase<FakeResponseDto>)
            .GetMethod("DeserializeResponseAsync", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedServiceBaseMock.Object, [response])!;

        Assert.That(deserializedResponse, Is.EqualTo(expectedDeserializedResponse));
    }

    [TestCase(TestName = "GetIssueUrlFromResponse_ValidResponse_ReturnsCorrectUrl")]
    public void GetIssueUrlFromResponseTest()
    {
        const string expectedUrl = "UrlFromGit";
        var response = new FakeResponseDto
        {
            IssueUrl = expectedUrl
        };

        var testedServiceBaseMock = new Mock<GitServiceBase<FakeResponseDto>>(new Mock<IHttpClientFactory>().Object, string.Empty)
        {
            CallBase = true
        };

        var actualUrl = (string)typeof(GitServiceBase<FakeResponseDto>)
            .GetMethod("GetIssueUrlFromResponse", BindingFlags.NonPublic | BindingFlags.Instance)?
            .Invoke(testedServiceBaseMock.Object, [response])!;

        Assert.That(actualUrl, Is.EqualTo(expectedUrl));
    }
}