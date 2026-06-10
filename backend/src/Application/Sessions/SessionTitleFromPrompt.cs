namespace PromptifyWebApi.Application.Sessions;

public static class SessionTitleFromPrompt
{
    public const int MaxLength = 200;

    private const string FallbackTitle = "Untitled session";

    public static string Derive(string input)
    {
        foreach (var line in input.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.Length > 0)
            {
                return Truncate(trimmed);
            }
        }

        var collapsed = input.Trim();
        if (collapsed.Length > 0)
        {
            return Truncate(collapsed);
        }

        return FallbackTitle;
    }

    private static string Truncate(string value) =>
        value.Length <= MaxLength ? value : value[..MaxLength];
}
