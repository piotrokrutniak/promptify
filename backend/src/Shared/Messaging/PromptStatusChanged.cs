namespace PromptifyWebApi.Shared.Messaging;

public record PromptStatusChanged(
    int PromptId,
    int SessionId,
    int OrderIndex,
    string Status,
    string Input,
    string? Output,
    string? ErrorMessage);
