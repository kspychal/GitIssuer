namespace GitIssuer.Core.Dto.Requests;

public record ModifyIssueRequestDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}