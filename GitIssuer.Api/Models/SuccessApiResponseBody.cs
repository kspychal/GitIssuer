namespace GitIssuer.Api.Models;

/// <summary>
/// Represents the structure of a successful response body.
/// </summary>
/// <param name="Url">The URL to the issue affected.</param>
public record SuccessApiResponseBody(string Url);