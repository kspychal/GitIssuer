namespace GitIssuer.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a Git-related operation returns a response with a non-success status code.
/// The inner message contains the content of the response, providing additional details about the error.
/// </summary>
/// <param name="message">The message that describes the error.</param>
/// <param name="innerMessage">The response content.</param>
public class GitException(string message, string innerMessage) : Exception(message)
{
    public string InnerMessage { get; } = innerMessage;
}