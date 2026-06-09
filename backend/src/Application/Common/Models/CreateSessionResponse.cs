namespace PromptifyWebApi.Application.Common.Models;

public class CreateSessionResponse
{
    public int SessionId { get; init; }

    public string? Title { get; init; }

    public PromptDto Prompt { get; init; } = null!;
}
