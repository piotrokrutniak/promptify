namespace PromptifyWebApi.Application.Common.Llm;

public record LlmMessage(LlmRole Role, string Content);
