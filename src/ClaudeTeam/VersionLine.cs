using System.Reflection;

namespace ClaudeTeam;

public static class VersionLine
{
    private const int ShortCommitLength = 7;

    public static string Current() => Format(
        typeof(VersionLine).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);

    public static string Format(string? informationalVersion)
    {
        if (string.IsNullOrWhiteSpace(informationalVersion))
            return "claude-team unknown";

        var plus = informationalVersion.IndexOf('+');
        if (plus < 0)
            return $"claude-team {informationalVersion}";

        var commit = informationalVersion[(plus + 1)..];
        if (commit.Length > ShortCommitLength)
            commit = commit[..ShortCommitLength];
        return $"claude-team {informationalVersion[..plus]}+{commit}";
    }
}
