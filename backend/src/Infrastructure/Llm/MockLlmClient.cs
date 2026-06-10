using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Llm;
using Microsoft.Extensions.Options;

namespace PromptifyWebApi.Infrastructure.Llm;

public class MockLlmClient(IOptions<LlmOptions> options) : ILlmClient
{
    public async Task<string> CompleteAsync(
        IReadOnlyList<LlmMessage> messages,
        CancellationToken cancellationToken)
    {
        await Task.Delay(options.Value.MockDelayMs, cancellationToken);

        var lastUserMessage = messages
            .LastOrDefault(message => message.Role == LlmRole.User)
            ?.Content ?? string.Empty;

        return $"Mock response to: {lastUserMessage}";
    }
}
