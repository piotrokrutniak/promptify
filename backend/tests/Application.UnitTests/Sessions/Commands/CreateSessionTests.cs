using MassTransit;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Sessions.Commands;

public class CreateSessionTests
{
    [Test]
    public async Task Handle_ShouldPersistPromptWithIdAndPublishProcessPromptCommand()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var userId = Guid.NewGuid().ToString();
        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);

        ProcessPromptCommand? published = null;
        var publishEndpoint = new Mock<IPublishEndpoint>();
        publishEndpoint
            .Setup(p => p.Publish(It.IsAny<ProcessPromptCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ProcessPromptCommand, CancellationToken>((command, _) => published = command)
            .Returns(Task.CompletedTask);

        var handler = new CreateSessionCommandHandler(context, user.Object, publishEndpoint.Object);

        var result = await handler.Handle(
            new CreateSessionCommand("Hello", null),
            CancellationToken.None);

        result.SessionId.ShouldBeGreaterThan(0);
        result.Title.ShouldBe("Hello");
        result.Prompt.Id.ShouldBeGreaterThan(0);
        result.Prompt.Status.ShouldBe(PromptStatus.Pending);
        result.Prompt.Input.ShouldBe("Hello");

        var prompts = await context.Prompts.AsNoTracking().ToListAsync();
        prompts.Count.ShouldBe(1);
        prompts[0].Id.ShouldBe(result.Prompt.Id);
        prompts[0].SessionId.ShouldBe(result.SessionId);
        prompts[0].OrderIndex.ShouldBe(0);
        prompts[0].Status.ShouldBe(PromptStatus.Pending);

        published.ShouldNotBeNull();
        published!.PromptId.ShouldBe(result.Prompt.Id);
        published.SessionId.ShouldBe(result.SessionId);
    }
}
