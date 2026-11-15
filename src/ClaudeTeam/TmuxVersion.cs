using System.Text.RegularExpressions;

namespace ClaudeTeam;

// tmux added new-session -e in version 3.2 (tmux CHANGES, "CHANGES FROM 3.1c TO 3.2").
public static partial class TmuxVersion
{
    public const int MinimumMajor = 3;
    public const int MinimumMinor = 2;

    public static bool TryParse(string text, out int major, out int minor)
    {
        major = minor = 0;
        var match = VersionPattern().Match(text.Trim());
        return match.Success
               && int.TryParse(match.Groups[1].Value, out major)
               && int.TryParse(match.Groups[2].Value, out minor);
    }

    // Text that does not parse counts as supported, so a development build does not fail.
    public static bool IsSupported(string text)
        => !TryParse(text, out var major, out var minor)
           || major > MinimumMajor
           || (major == MinimumMajor && minor >= MinimumMinor);

    public static string TooOldMessage(string text)
        => $"{text.Trim()} is too old. claude-team needs tmux {MinimumMajor}.{MinimumMinor} or newer (new-session -e). Nothing was started.";

    [GeneratedRegex(@"^tmux (\d+)\.(\d+)[a-z]*$")]
    private static partial Regex VersionPattern();
}
