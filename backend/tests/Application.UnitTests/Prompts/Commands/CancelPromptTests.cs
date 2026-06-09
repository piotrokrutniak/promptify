using MassTransit;
using PromptifyWebApi.Application.Common.Exceptions;
using PromptifyWebApi.Application.Common.Interfaces;
using PromptifyWebApi.Application.Prompts.Commands.CancelPrompt;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using PromptifyWebApi.Infrastructure.Data;
using PromptifyWebApi.Shared.Messaging;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Prompts.Commands;

public class CancelPromptTests
{
    [Test]
    public async Task Handle_WhenPromptIsPending_ShouldCancelAndPublishStatusChanged()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var userId = Guid.NewGuid().ToString();
        var session = new Session { UserId = userId };
        var prompt = new Prompt
        {
            OrderIndex = 0,
            Input = "cancel me",
            Status = PromptStatus.Pending
        };
        session.Prompts.Add(prompt);
        context.Sessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);

        PromptStatusChanged? published = null;
        var publishEndpoint = new Mock<IPublishEndpoint>();
        publishEndpoint
            .Setup(p => p.Publish(It.IsAny<PromptStatusChanged>(), It.IsAny<CancellationToken>()))
            .Callback<PromptStatusChanged, CancellationToken>((message, _) => published = message)
            .Returns(Task.CompletedTask);

        var handler = new CancelPromptCommandHandler(context, user.Object, publishEndpoint.Object);

        await handler.Handle(new CancelPromptCommand(prompt.Id), CancellationToken.None);

        var updated = await context.Prompts.AsNoTracking().SingleAsync();
        updated.Status.ShouldBe(PromptStatus.Cancelled);

        published.ShouldNotBeNull();
        published!.PromptId.ShouldBe(prompt.Id);
        published.Status.ShouldBe(nameof(PromptStatus.Cancelled));
    }

    [Test]
    public async Task Handle_WhenPromptIsProcessing_ShouldThrowConflictException()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var context = new ApplicationDbContext(options);

        var userId = Guid.NewGuid().ToString();
        var session = new Session { UserId = userId };
        var prompt = new Prompt
        {
            OrderIndex = 0,
            Input = "busy",
            Status = PromptStatus.Processing
        };
        session.Prompts.Add(prompt);
        context.Sessions.Add(session);
        await context.SaveChangesAsync(CancellationToken.None);

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(userId);

        var handler = new CancelPromptCommandHandler(
            context,
            user.Object,
            Mock.Of<IPublishEndpoint>());

        var act = () => handler.Handle(new CancelPromptCommand(prompt.Id), CancellationToken.None);

        await act.ShouldThrowAsync<ConflictException>();
    }
}
