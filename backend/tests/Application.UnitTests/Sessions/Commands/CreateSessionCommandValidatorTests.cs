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
        var result = _validator.Validate(new CreateSessionCommand("", null, null));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSessionCommand.Input));
    }

    [Test]
    public void ShouldRejectInputExceedingMaxLength()
    {
        var result = _validator.Validate(new CreateSessionCommand(new string('a', 8001), null, null));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSessionCommand.Input));
    }

    [Test]
    public void ShouldRejectTitleExceedingMaxLength()
    {
        var result = _validator.Validate(new CreateSessionCommand("Hello", new string('t', 201), null));

        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.PropertyName == nameof(CreateSessionCommand.Title));
    }

    [Test]
    public void ShouldAcceptMinimalValidCommand()
    {
        var result = _validator.Validate(new CreateSessionCommand("Hello", null, null));

        result.IsValid.ShouldBeTrue();
    }

    [Test]
    public void ShouldAcceptFullValidCommand()
    {
        var result = _validator.Validate(new CreateSessionCommand("Hello", "My session", "{\"key\":\"value\"}"));

        result.IsValid.ShouldBeTrue();
    }
}
