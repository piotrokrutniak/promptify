using PromptifyWebApi.Application.Common.Exceptions;
using PromptifyWebApi.Application.FunctionalTests.Infrastructure;
using PromptifyWebApi.Application.Sessions.Commands.CreatePrompt;
using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Consumers;
using PromptifyWebApi.Shared.Messaging;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.FunctionalTests.Sessions.Commands;

public class CreatePromptTests : TestBase
{
    [Test]
    public async Task ShouldRequireInput()
    {
        await TestApp.RunAsDefaultUserAsync();

        var session = await TestApp.SendAsync(new CreateSessionCommand("first"));

        await Should.ThrowAsync<ValidationException>(
            () => TestApp.SendAsync(new CreatePromptCommand(session.SessionId, "")));
    }

    [Test]
    public async Task ShouldRejectWhenSessionBusy()
    {
        await TestApp.RunAsDefaultUserAsync();

        var session = await TestApp.SendAsync(new CreateSessionCommand("first"));

        await Should.ThrowAsync<ConflictException>(
            () => TestApp.SendAsync(new CreatePromptCommand(session.SessionId, "second")));
    }

    [Test]
    public async Task ShouldAddFollowUpPromptWhenIdle()
    {
        await TestApp.RunAsDefaultUserAsync();

        var session = await TestApp.SendAsync(new CreateSessionCommand("first"));

        await ConsumePromptAsync(session.Prompt.Id, session.SessionId);

        var followUp = await TestApp.SendAsync(new CreatePromptCommand(session.SessionId, "second"));

        followUp.OrderIndex.ShouldBe(1);
        followUp.Status.ShouldBe(PromptStatus.Pending);
        followUp.Input.ShouldBe("second");

        var prompt = await TestApp.FindAsync<Prompt>(followUp.Id);
        prompt.ShouldNotBeNull();
        prompt!.SessionId.ShouldBe(session.SessionId);
        prompt.Status.ShouldBe(PromptStatus.Pending);
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
