using PromptifyWebApi.Application.Sessions.Commands.CreatePrompt;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Sessions.Commands;

public class CreatePromptCommandValidatorTests
{
    private readonly CreatePromptCommandValidator _validator = new();

    [Test]
    public void ShouldRequirePositiveSessionId()
    {
        var result = _validator.Validate(new CreatePromptCommand(0, "Hello"));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreatePromptCommand.SessionId));
    }

    [Test]
    public void ShouldRequireInput()
    {
        var result = _validator.Validate(new CreatePromptCommand(1, ""));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreatePromptCommand.Input));
    }

    [Test]
    public void ShouldRejectInputExceedingMaxLength()
    {
        var result = _validator.Validate(new CreatePromptCommand(1, new string('a', 8001)));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreatePromptCommand.Input));
    }

    [Test]
    public void ShouldAcceptValidCommand()
    {
        var result = _validator.Validate(new CreatePromptCommand(1, "Hello"));

        result.IsValid.ShouldBeTrue();
    }
}
