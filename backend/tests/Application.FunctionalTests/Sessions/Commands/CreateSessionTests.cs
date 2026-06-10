using PromptifyWebApi.Application.Common.Exceptions;
using PromptifyWebApi.Application.FunctionalTests.Infrastructure;
using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.FunctionalTests.Sessions.Commands;

public class CreateSessionTests : TestBase
{
    [Test]
    public async Task ShouldRequireInput()
    {
        await TestApp.RunAsDefaultUserAsync();

        await Should.ThrowAsync<ValidationException>(
            () => TestApp.SendAsync(new CreateSessionCommand("", null, null)));
    }

    [Test]
    public async Task ShouldCreateSessionWithPendingPrompt()
    {
        await TestApp.RunAsDefaultUserAsync();

        var response = await TestApp.SendAsync(new CreateSessionCommand("Hello", "My session", null));

        response.SessionId.ShouldBeGreaterThan(0);
        response.Title.ShouldBe("My session");
        response.Prompt.Status.ShouldBe(nameof(PromptStatus.Pending));
        response.Prompt.Input.ShouldBe("Hello");
        response.Prompt.OrderIndex.ShouldBe(0);

        var session = await TestApp.FindAsync<Session>(response.SessionId);
        session.ShouldNotBeNull();
        session!.Title.ShouldBe("My session");

        var prompt = await TestApp.FindAsync<Prompt>(response.Prompt.Id);
        prompt.ShouldNotBeNull();
        prompt!.SessionId.ShouldBe(response.SessionId);
        prompt.Status.ShouldBe(PromptStatus.Pending);
    }
}
