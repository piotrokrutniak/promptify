namespace PromptifyWebApi.Application.Common.Models;

public class PromptDto
{
    public int Id { get; init; }

    public int OrderIndex { get; init; }

    public string Status { get; init; } = null!;

    public string Input { get; init; } = null!;

    public string? Output { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTimeOffset Created { get; init; }
}
