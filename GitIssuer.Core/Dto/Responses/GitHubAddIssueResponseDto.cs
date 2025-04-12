using System.Text.Json.Serialization;

namespace GitIssuer.Core.Dto.Responses;

public record GitHubAddIssueResponseDto
{
    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }
}