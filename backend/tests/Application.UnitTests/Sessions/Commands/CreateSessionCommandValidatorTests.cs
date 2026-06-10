using PromptifyWebApi.Application.Sessions.Commands.CreateSession;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Sessions.Commands;

public class CreateSessionCommandValidatorTests
{
    private readonly CreateSessionCommandValidator _validator = new();

    [Test]
    public void ShouldRequireInput()
    {
        var result = _validator.Validate(new CreateSessionCommand("", null));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSessionCommand.Input));
    }

    [Test]
    public void ShouldRejectInputExceedingMaxLength()
    {
        var result = _validator.Validate(new CreateSessionCommand(new string('a', 8001), null));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSessionCommand.Input));
    }

    [Test]
    public void ShouldAcceptMinimalValidCommand()
    {
        var result = _validator.Validate(new CreateSessionCommand("Hello", null));

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptFullValidCommand()
    {
        var result = _validator.Validate(new CreateSessionCommand("Hello", "{\"key\":\"value\"}"));

        result.IsValid.ShouldBeTrue();
    }
}
