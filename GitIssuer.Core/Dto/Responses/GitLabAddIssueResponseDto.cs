using System.Text.Json.Serialization;

namespace GitIssuer.Core.Dto.Responses;

public record GitLabAddIssueResponseDto
{
    [JsonPropertyName("web_url")]
    public string? WebUrl { get; set; }
}