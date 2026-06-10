using MassTransit;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Infrastructure.Prompts;
using PromptifyWebApi.Shared.Messaging;
using PromptifyWebApi.Infrastructure.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Worker.Consumers;

public class ProcessPromptConsumerTests
{
    [Test]
    public async Task Consume_WhenPromptIsCancelled_ShouldReturnWithoutPublishing()
    {
        var (consumer, publishEndpoint, context) = await CreateConsumerAsync(
            new Prompt
            {
                SessionId = 1,
                OrderIndex = 0,
                Input = "hello",
                Status = PromptStatus.Cancelled
            });

        await consumer.Consume(CreateContext(context.Prompts.First().Id, 1));

        publishEndpoint.Verify(
            p => p.Publish(It.IsAny<PromptStatusChanged>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task Consume_WhenPromptIsAlreadyCompleted_ShouldReturnWithoutPublishing()
    {
        var (consumer, publishEndpoint, context) = await CreateConsumerAsync(
            new Prompt
            {
                SessionId = 1,
                OrderIndex = 0,
                Input = "hello",
                Status = PromptStatus.Completed,
                Output = "done"
            });

        await consumer.Consume(CreateContext(context.Prompts.First().Id, 1));

        publishEndpoint.Verify(
            p => p.Publish(It.IsAny<PromptStatusChanged>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static async Task<(ProcessPromptConsumer Consumer, Mock<IPublishEndpoint> PublishEndpoint, ApplicationDbContext Context)> CreateConsumerAsync(
        Prompt prompt)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new ApplicationDbContext(options);
        context.Prompts.Add(prompt);
        await context.SaveChangesAsync(CancellationToken.None);

        var publishEndpoint = new Mock<IPublishEndpoint>();
        var consumer = new ProcessPromptConsumer(
            context,
            new PromptClaimService(context, TimeProvider.System, Mock.Of<ILogger<PromptClaimService>>()),
            Mock.Of<ILlmClient>(),
            publishEndpoint.Object,
            TimeProvider.System,
            Mock.Of<ILogger<ProcessPromptConsumer>>());

        return (consumer, publishEndpoint, context);
    }

    private static ConsumeContext<ProcessPromptCommand> CreateContext(int promptId, int sessionId)
    {
        var consumeContext = new Mock<ConsumeContext<ProcessPromptCommand>>();
        consumeContext.Setup(c => c.Message).Returns(new ProcessPromptCommand(promptId, sessionId));
        consumeContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);
        return consumeContext.Object;
    }
}
