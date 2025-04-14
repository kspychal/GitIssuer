using GitIssuer.Core.Dto.Responses.Interfaces;

namespace GitIssuer.Core.Tests.FakeObjects;

public record FakeResponseDto : IIssueResponse
{
    public string? IssueUrl { get; set; }
}