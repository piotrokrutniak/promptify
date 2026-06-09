using PromptifyWebApi.Application.Common.Interfaces;
using Microsoft.Extensions.Options;

namespace PromptifyWebApi.Infrastructure.Llm;

public class MockLlmClient(IOptions<LlmOptions> options) : ILlmClient
{
    public async Task<string> CompleteAsync(string input, CancellationToken cancellationToken)
    {
        await Task.Delay(options.Value.MockDelayMs, cancellationToken);
        return $"Mock response to: {input}";
    }
}
