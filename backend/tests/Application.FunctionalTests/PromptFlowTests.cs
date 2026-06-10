using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using PromptifyWebApi.Application.FunctionalTests.Infrastructure;
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

        using (var scope = FunctionalTestSetup.ScopeFactory.CreateScope())
        {
            var consumer = scope.ServiceProvider.GetRequiredService<ProcessPromptConsumer>();

            var consumeContext = new Mock<ConsumeContext<ProcessPromptCommand>>();
            consumeContext.Setup(c => c.Message)
                .Returns(new ProcessPromptCommand(response.Prompt.Id, response.SessionId));
            consumeContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

            await consumer.Consume(consumeContext.Object);
        }

        var completed = await TestApp.FindAsync<Prompt>(response.Prompt.Id);
        completed.ShouldNotBeNull();
        completed!.Status.ShouldBe(PromptStatus.Completed);
        completed.Output.ShouldNotBeNullOrWhiteSpace();
    }
}
