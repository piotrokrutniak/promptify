namespace PromptifyWebApi.Domain.Entities;

public class Prompt : BaseAuditableEntity
{
    public int SessionId { get; set; }

    public int OrderIndex { get; set; }

    public string Input { get; set; } = null!;

    public string? Output { get; set; }

    public string? ErrorMessage { get; set; }

    public PromptStatus Status { get; set; } = PromptStatus.Pending;

    public DateTimeOffset? ProcessingStartedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Session Session { get; set; } = null!;
}
