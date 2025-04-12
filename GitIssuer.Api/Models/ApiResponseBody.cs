using System.Text.Json.Serialization;

namespace GitIssuer.Api.Models;

public record ApiResponseBody
{
    public bool Success { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Url { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Error { get; set; }
}