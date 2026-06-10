using OpenAI.Chat;
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

    [Test]
    public void ExtractAssistantText_WhenContentEmpty_ShouldThrow()
    {
        var act = () => OpenAiLlmClient.ExtractAssistantText(
            Array.Empty<ChatMessageContentPart>());

        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("OpenAI returned an empty response.");
    }

    [Test]
    public void ExtractAssistantText_WhenTextIsWhitespace_ShouldThrow()
    {
        var act = () => OpenAiLlmClient.ExtractAssistantText(
            [ChatMessageContentPart.CreateTextPart("   ")]);

        act.ShouldThrow<InvalidOperationException>()
            .Message.ShouldBe("OpenAI returned an empty response.");
    }

    [Test]
    public void ExtractAssistantText_WhenTextPresent_ShouldReturnText()
    {
        var result = OpenAiLlmClient.ExtractAssistantText(
            [ChatMessageContentPart.CreateTextPart("hello from openai")]);

        result.ShouldBe("hello from openai");
    }
}
