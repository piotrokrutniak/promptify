using PromptifyWebApi.Application.FunctionalTests.Infrastructure;
using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using PromptifyWebApi.Domain.Entities;
using PromptifyWebApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
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

        var deadline = DateTime.UtcNow.AddSeconds(60);
        PromptStatus? status = null;

        while (DateTime.UtcNow < deadline)
        {
            var prompt = await TestApp.FindAsync<Prompt>(response.Prompt.Id);
            status = prompt?.Status;

            if (status is PromptStatus.Completed or PromptStatus.Failed)
            {
                break;
            }

            await Task.Delay(500);
        }

        status.ShouldBe(PromptStatus.Completed);

        var completed = await TestApp.FindAsync<Prompt>(response.Prompt.Id);
        completed!.Output.ShouldNotBeNullOrWhiteSpace();
    }
}
