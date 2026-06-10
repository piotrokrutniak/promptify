using PromptifyWebApi.Application.Sessions;
using NUnit.Framework;
using Shouldly;

namespace PromptifyWebApi.Application.UnitTests.Sessions;

public class SessionTitleFromPromptTests
{
    [Test]
    public void Derive_ShouldUseFirstLine()
    {
        SessionTitleFromPrompt.Derive("Hello world\nmore").ShouldBe("Hello world");
    }

    [Test]
    public void Derive_ShouldSkipLeadingBlankLines()
    {
        SessionTitleFromPrompt.Derive("  \nFirst line\nsecond").ShouldBe("First line");
    }

    [Test]
    public void Derive_ShouldTrimFirstLine()
    {
        SessionTitleFromPrompt.Derive("  padded title  ").ShouldBe("padded title");
    }

    [Test]
    public void Derive_ShouldTruncateLongFirstLine()
    {
        var longLine = new string('a', SessionTitleFromPrompt.MaxLength + 50);

        SessionTitleFromPrompt.Derive(longLine).ShouldBe(new string('a', SessionTitleFromPrompt.MaxLength));
    }

    [Test]
    public void Derive_WhenOnlyWhitespaceLines_ShouldFallbackToTrimmedInput()
    {
        SessionTitleFromPrompt.Derive("  \n\t  \n  inner  ").ShouldBe("inner");
    }

    [Test]
    public void Derive_WhenInputIsWhitespaceOnly_ShouldReturnUntitledSession()
    {
        SessionTitleFromPrompt.Derive("   \n\t  ").ShouldBe("Untitled session");
    }
}
