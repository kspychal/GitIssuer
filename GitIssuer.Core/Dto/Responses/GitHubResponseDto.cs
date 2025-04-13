using System.Text.Json.Serialization;

namespace GitIssuer.Core.Dto.Responses;

public record GitHubResponseDto
{
    [JsonPropertyName("html_url")]
    public string? HtmlUrl { get; set; }
}