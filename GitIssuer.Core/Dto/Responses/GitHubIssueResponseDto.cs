using GitIssuer.Core.Dto.Responses.Interfaces;
using System.Text.Json.Serialization;

namespace GitIssuer.Core.Dto.Responses;

public record GitHubIssueResponseDto : IIssueResponse
{
    [JsonPropertyName("html_url")]
    public string? IssueUrl { get; set; }
}