using PromptifyWebApi.Infrastructure.Llm;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Infrastructure.Llm;

public class OpenAiLlmClientTests
{
    [Test]
    public void Constructor_WhenApiKeyMissing_ShouldThrow()
    {
        var act = () => new OpenAiLlmClient(Options.Create(new LlmOptions
        {
            Provider = "OpenAI",
            OpenAiApiKey = null,
        }));

        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldContain("OpenAiApiKey");
    }
}
