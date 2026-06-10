namespace PromptifyWebApi.Infrastructure.Llm;

public class LlmOptions
{
    public const string SectionName = "Llm";

    public string Provider { get; set; } = "Mock";

    public int MockDelayMs { get; set; } = 2000;

    public string? OpenAiApiKey { get; set; }

    public string OpenAiModel { get; set; } = "gpt-4o-mini";

    public string? OpenAiBaseUrl { get; set; }

    public string OllamaBaseUrl { get; set; } = "http://localhost:11434";

    public string OllamaModel { get; set; } = "llama3.2";

    public int TimeoutSeconds { get; set; } = 120;
}
