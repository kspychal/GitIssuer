namespace GitIssuer.Core.Exceptions;

public class GitException(string message, string innerMessage) : Exception(message)
{
    public string InnerMessage { get; } = innerMessage;
}