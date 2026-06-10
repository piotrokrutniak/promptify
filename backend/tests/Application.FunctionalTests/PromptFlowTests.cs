using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Common.Llm;
using PromptifyWebApi.Application.FunctionalTests.Infrastructure;
using PromptifyWebApi.Application.Sessions.Commands.CreatePrompt;
using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Consumers;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Infrastructure.Llm;
using PromptifyWebApi.Infrastructure.Prompts;
using PromptifyWebApi.Shared.Messaging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.FunctionalTests;

public class PromptFlowTests : TestBase
{
    [Test]
    public async Task CreateSession_ShouldProcessPromptToCompleted()
    {
        await TestApp.RunAsDefaultUserAsync();

        var response = await TestApp.SendAsync(new CreateSessionCommand("functional test", null));

        await ConsumePromptAsync(response.Prompt.Id, response.SessionId);

        var completed = await TestApp.FindAsync<Prompt>(response.Prompt.Id);
        completed.ShouldNotBeNull();
        completed!.Status.ShouldBe(PromptStatus.Completed);
        completed.Output.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task FollowUpPrompt_ShouldProcessToCompletedAfterFirstPromptFinishes()
    {
        await TestApp.RunAsDefaultUserAsync();

        var session = await TestApp.SendAsync(new CreateSessionCommand("first prompt", null));
        await ConsumePromptAsync(session.Prompt.Id, session.SessionId);

        var followUp = await TestApp.SendAsync(new CreatePromptCommand(session.SessionId, "second prompt", null));
        await ConsumePromptAsync(followUp.Id, session.SessionId);

        var first = await TestApp.FindAsync<Prompt>(session.Prompt.Id);
        first.ShouldNotBeNull();
        first!.Status.ShouldBe(PromptStatus.Completed);

        var second = await TestApp.FindAsync<Prompt>(followUp.Id);
        second.ShouldNotBeNull();
        second!.Status.ShouldBe(PromptStatus.Completed);
        second.OrderIndex.ShouldBe(1);
        second.Output.ShouldNotBeNullOrWhiteSpace();
    }

    [Test]
    public async Task FollowUpPrompt_ShouldPassPriorConversationToLlm()
    {
        await TestApp.RunAsDefaultUserAsync();

        IReadOnlyList<LlmMessage>? capturedOnSecond = null;
        var callCount = 0;
        var llmClient = new Mock<ILlmClient>();
        llmClient
            .Setup(client => client.CompleteAsync(
                It.IsAny<IReadOnlyList<LlmMessage>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<LlmMessage> messages, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 2)
                {
                    capturedOnSecond = messages;
                }

                var lastUserMessage = messages
                    .Last(message => message.Role == LlmRole.User)
                    .Content;

                return $"Mock response to: {lastUserMessage}";
            });

        var session = await TestApp.SendAsync(new CreateSessionCommand("first prompt", null));
        await ConsumePromptAsync(session.Prompt.Id, session.SessionId, llmClient.Object);

        var followUp = await TestApp.SendAsync(
            new CreatePromptCommand(session.SessionId, "second prompt", null));
        await ConsumePromptAsync(followUp.Id, session.SessionId, llmClient.Object);

        capturedOnSecond.ShouldNotBeNull();
        capturedOnSecond!.Count.ShouldBe(3);
        capturedOnSecond[0].ShouldBe(new LlmMessage(LlmRole.User, "first prompt"));
        capturedOnSecond[1].ShouldBe(new LlmMessage(LlmRole.Assistant, "Mock response to: first prompt"));
        capturedOnSecond[2].ShouldBe(new LlmMessage(LlmRole.User, "second prompt"));
    }

    private static async Task ConsumePromptAsync(int promptId, int sessionId)
    {
        await ConsumePromptAsync(promptId, sessionId, llmClient: null);
    }

    private static async Task ConsumePromptAsync(
        int promptId,
        int sessionId,
        ILlmClient? llmClient)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var consumer = llmClient is null
            ? scope.ServiceProvider.GetRequiredService<ProcessPromptConsumer>()
            : new ProcessPromptConsumer(
                scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(),
                scope.ServiceProvider.GetRequiredService<PromptClaimService>(),
                scope.ServiceProvider.GetRequiredService<ConversationHistoryBuilder>(),
                llmClient,
                scope.ServiceProvider.GetRequiredService<IPublishEndpoint>(),
                scope.ServiceProvider.GetRequiredService<TimeProvider>(),
                scope.ServiceProvider.GetRequiredService<ILogger<ProcessPromptConsumer>>());

        var consumeContext = new Mock<ConsumeContext<ProcessPromptCommand>>();
        consumeContext.Setup(c => c.Message).Returns(new ProcessPromptCommand(promptId, sessionId));
        consumeContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(consumeContext.Object);
    }
}
