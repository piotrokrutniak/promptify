using PromptifyWebApi.Application.Common.Llm;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Infrastructure.Llm;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Infrastructure.Llm;

public class ConversationHistoryBuilderTests
{
    [Test]
    public async Task BuildAsync_WithNoPriorPrompts_ShouldReturnCurrentUserMessageOnly()
    {
        var (builder, _) = await CreateBuilderAsync();

        var messages = await builder.BuildAsync(1, 0, "hello", CancellationToken.None);

        messages.Count.ShouldBe(1);
        messages[0].ShouldBe(new LlmMessage(LlmRole.User, "hello"));
    }

    [Test]
    public async Task BuildAsync_WithOnePriorPrompt_ShouldIncludeUserAssistantPair()
    {
        var (builder, context) = await CreateBuilderAsync(
            new Prompt
            {
                SessionId = 1,
                OrderIndex = 0,
                Input = "first",
                Output = "first reply",
                Status = PromptStatus.Completed,
            });

        var messages = await builder.BuildAsync(1, 1, "second", CancellationToken.None);

        messages.Count.ShouldBe(3);
        messages[0].ShouldBe(new LlmMessage(LlmRole.User, "first"));
        messages[1].ShouldBe(new LlmMessage(LlmRole.Assistant, "first reply"));
        messages[2].ShouldBe(new LlmMessage(LlmRole.User, "second"));

        await context.DisposeAsync();
    }

    [Test]
    public async Task BuildAsync_ShouldIgnoreIncompleteOrLaterPrompts()
    {
        var (builder, context) = await CreateBuilderAsync(
            new Prompt
            {
                SessionId = 1,
                OrderIndex = 0,
                Input = "completed",
                Output = "done",
                Status = PromptStatus.Completed,
            },
            new Prompt
            {
                SessionId = 1,
                OrderIndex = 1,
                Input = "pending",
                Status = PromptStatus.Pending,
            },
            new Prompt
            {
                SessionId = 2,
                OrderIndex = 0,
                Input = "other session",
                Output = "other",
                Status = PromptStatus.Completed,
            });

        var messages = await builder.BuildAsync(1, 2, "current", CancellationToken.None);

        messages.Count.ShouldBe(3);
        messages[0].Content.ShouldBe("completed");
        messages[2].Content.ShouldBe("current");

        await context.DisposeAsync();
    }

    private static async Task<(ConversationHistoryBuilder Builder, ApplicationDbContext Context)> CreateBuilderAsync(
        params Prompt[] prompts)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Prompts.AddRange(prompts);
        await context.SaveChangesAsync();

        return (new ConversationHistoryBuilder(context), context);
    }
}
