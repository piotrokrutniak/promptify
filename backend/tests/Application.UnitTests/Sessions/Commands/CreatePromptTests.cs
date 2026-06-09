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
