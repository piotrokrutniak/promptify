using PromptifyWebApi.Domain.Enums;

namespace PromptifyWebApi.Application.Common.Models;

public class PromptDto
{
    public int Id { get; init; }

    public int OrderIndex { get; init; }

    public PromptStatus Status { get; init; }

    public string Input { get; init; } = null!;

    public string? Output { get; init; }

    public string? ErrorMessage { get; init; }

    public DateTimeOffset Created { get; init; }
}
