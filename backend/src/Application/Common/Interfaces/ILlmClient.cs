using PromptifyWebApi.Application.Common.Llm;

namespace PromptifyWebApi.Application.Common.Interfaces;

public interface ILlmClient
{
    Task<string> CompleteAsync(
        IReadOnlyList<LlmMessage> messages,
        CancellationToken cancellationToken);
}
