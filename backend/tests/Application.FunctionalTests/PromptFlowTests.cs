using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using PromptifyWebApi.Application.FunctionalTests.Infrastructure;
using PromptifyWebApi.Application.Sessions.Commands.CreatePrompt;
using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Consumers;
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

        var response = await TestApp.SendAsync(new CreateSessionCommand("functional test", "title", null));

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

        var session = await TestApp.SendAsync(new CreateSessionCommand("first prompt", "title", null));
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

    private static async Task ConsumePromptAsync(int promptId, int sessionId)
    {
        using var scope = FunctionalTestSetup.ScopeFactory.CreateScope();
        var consumer = scope.ServiceProvider.GetRequiredService<ProcessPromptConsumer>();

        var consumeContext = new Mock<ConsumeContext<ProcessPromptCommand>>();
        consumeContext.Setup(c => c.Message).Returns(new ProcessPromptCommand(promptId, sessionId));
        consumeContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(consumeContext.Object);
    }
}
