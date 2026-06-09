namespace PromptifyWebApi.Application.Common.Models;

public class SessionDto
{
    public int Id { get; init; }

    public string? Title { get; init; }

    public DateTimeOffset Created { get; init; }

    public IList<PromptDto> Prompts { get; init; } = [];
}
