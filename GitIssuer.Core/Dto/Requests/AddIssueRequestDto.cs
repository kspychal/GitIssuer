namespace GitIssuer.Core.Dto.Requests;

public record AddIssueRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}