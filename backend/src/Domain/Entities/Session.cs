namespace PromptifyWebApi.Domain.Entities;

public class Session : BaseAuditableEntity
{
    public string UserId { get; set; } = null!;

    public string? Title { get; set; }

    public IList<Prompt> Prompts { get; private set; } = new List<Prompt>();
}
