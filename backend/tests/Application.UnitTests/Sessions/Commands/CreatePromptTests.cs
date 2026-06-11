using MassTransit;
using PromptifyWebApi.Application.Common.Exceptions;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Sessions.Commands.CreatePrompt;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Sessions.Commands;

public class CreatePromptTests
{
    [Test]
    public async Task Handle_ShouldPersistFollowUpPromptAndPublishProcessPromptCommand()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var userId = Guid.NewGuid().ToString();
        var session = new Session { UserId = userId, Title = "Test" };
        session.Prompts.Add(new Prompt
        {
            OrderIndex = 0,
            Input = "first",
            Status = PromptStatus.Completed,
            Output = "done"
        });
        context.Sessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);

        ProcessPromptCommand? published = null;
        var publishEndpoint = new Mock<IPublishEndpoint>();
        publishEndpoint
            .Setup(p => p.Publish(It.IsAny<ProcessPromptCommand>(), It.IsAny<CancellationToken>()))
            .Callback<ProcessPromptCommand, CancellationToken>((command, _) => published = command)
            .Returns(Task.CompletedTask);

        var handler = new CreatePromptCommandHandler(context, user.Object, publishEndpoint.Object);

        var result = await handler.Handle(
            new CreatePromptCommand(session.Id, "second", null),
            CancellationToken.None);

        result.Id.ShouldBeGreaterThan(0);
        result.OrderIndex.ShouldBe(1);
        result.Status.ShouldBe(PromptStatus.Pending);
        result.Input.ShouldBe("second");

        var prompts = await context.Prompts.AsNoTracking().ToListAsync();
        prompts.Count.ShouldBe(2);
        prompts[1].OrderIndex.ShouldBe(1);
        prompts[1].Status.ShouldBe(PromptStatus.Pending);

        published.ShouldNotBeNull();
        published!.PromptId.ShouldBe(result.Id);
        published.SessionId.ShouldBe(session.Id);
    }

    [Test]
    public async Task Handle_WhenUserDoesNotOwnSession_ShouldThrowForbiddenAccessException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var session = new Session { UserId = Guid.NewGuid().ToString(), Title = "Test" };
        context.Sessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());

        var handler = new CreatePromptCommandHandler(
            context,
            user.Object,
            Mock.Of<IPublishEndpoint>());

        var act = () => handler.Handle(
            new CreatePromptCommand(session.Id, "second", null),
            CancellationToken.None);

        await act.ShouldThrowAsync<ForbiddenAccessException>();
    }

    [Test]
    public async Task Handle_WhenSessionNotFound_ShouldThrowEntityNotFoundException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(Guid.NewGuid().ToString());

        var handler = new CreatePromptCommandHandler(
            context,
            user.Object,
            Mock.Of<IPublishEndpoint>());

        var act = () => handler.Handle(
            new CreatePromptCommand(999, "second", null),
            CancellationToken.None);

        await act.ShouldThrowAsync<EntityNotFoundException>();
    }

    [Test]
    public async Task Handle_WhenSessionHasActivePrompt_ShouldThrowConflictException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var userId = Guid.NewGuid().ToString();
        var session = new Session { UserId = userId, Title = "Test" };
        session.Prompts.Add(new Prompt
        {
            OrderIndex = 0,
            Input = "first",
            Status = PromptStatus.Processing
        });
        context.Sessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);

        var publishEndpoint = new Mock<IPublishEndpoint>();
        var handler = new CreatePromptCommandHandler(context, user.Object, publishEndpoint.Object);

        var act = () => handler.Handle(
            new CreatePromptCommand(session.Id, "second", null),
            CancellationToken.None);

        await act.ShouldThrowAsync<ConflictException>();
        publishEndpoint.Verify(
            p => p.Publish(It.IsAny<ProcessPromptCommand>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
