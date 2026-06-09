using MassTransit;
using PromptifyWebApi.Shared.Messaging;
using PromptifyWebApi.Web.Consumers;
using PromptifyWebApi.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Moq;
using NUnit.Framework;

namespace PromptifyWebApi.Application.UnitTests.Web.Consumers;

public class PromptStatusChangedConsumerTests
{
    [Test]
    public async Task Consume_ShouldSendPromptStatusChangedToSessionGroup()
    {
        var message = new PromptStatusChanged(
            PromptId: 1,
            SessionId: 42,
            OrderIndex: 0,
            Status: "Completed",
            Input: "hello",
            Output: "world",
            ErrorMessage: null);

        var clientProxy = new Mock<IClientProxy>();
        clientProxy
            .Setup(c => c.SendCoreAsync(
                "PromptStatusChanged",
                It.Is<object?[]>(args => args.Length == 1 && ReferenceEquals(args[0], message)),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var clients = new Mock<IHubClients>();
        clients
            .Setup(c => c.Group($"session-{message.SessionId}"))
            .Returns(clientProxy.Object);

        var hubContext = new Mock<IHubContext<PromptStatusHub>>();
        hubContext.Setup(h => h.Clients).Returns(clients.Object);

        var consumer = new PromptStatusChangedConsumer(hubContext.Object);

        var consumeContext = new Mock<ConsumeContext<PromptStatusChanged>>();
        consumeContext.Setup(c => c.Message).Returns(message);
        consumeContext.Setup(c => c.CancellationToken).Returns(CancellationToken.None);

        await consumer.Consume(consumeContext.Object);

        clientProxy.Verify(
            c => c.SendCoreAsync(
                "PromptStatusChanged",
                It.IsAny<object?[]>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
