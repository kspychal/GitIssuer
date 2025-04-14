using GitIssuer.Core.Dto.Responses.Interfaces;
using System.Text.Json.Serialization;

namespace GitIssuer.Core.Dto.Responses;

public record GitLabIssueResponseDto : IIssueResponse
{
    [JsonPropertyName("web_url")]
    public string? IssueUrl { get; set; }
}