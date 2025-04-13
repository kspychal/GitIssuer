namespace GitIssuer.Api.Models;

/// <summary>
/// Represents the structure of an error response body.
/// </summary>
/// <param name="Error">A brief description of the error.</param>
/// <param name="Details">Optional additional details about the error.</param>
public record ErrorApiResponseBody(string Error, string? Details);