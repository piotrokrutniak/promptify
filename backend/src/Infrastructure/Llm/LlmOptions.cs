namespace PromptifyWebApi.Infrastructure.Llm;

public class LlmOptions
{
    public const string SectionName = "Llm";

    public string Provider { get; set; } = "Mock";

    public int MockDelayMs { get; set; } = 2000;
}
