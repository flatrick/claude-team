namespace ClaudeTeam;

public sealed record GitInfo(string Toplevel, string GitDir, string CommonDir, string? OriginUrl);

public static class SessionName
{
    public static string Resolve(GitInfo? git, string workingDirectory)
    {
        if (git is null)
            return Sanitize(FolderName(workingDirectory));

        var repository = (git.OriginUrl is { } url ? FromRemoteUrl(url) : null)
            ?? FolderName(Path.GetDirectoryName(Trim(git.CommonDir)) ?? git.CommonDir);
        var name = Trim(git.GitDir) == Trim(git.CommonDir)
            ? repository
            : $"{repository}+{FolderName(git.Toplevel)}";
        return Sanitize(name);
    }

    public static string? FromRemoteUrl(string url)
    {
        var trimmed = url.Trim().TrimEnd('/', '\\');
        var scheme = trimmed.IndexOf("://", StringComparison.Ordinal);
        if (scheme >= 0)
        {
            var path = trimmed.IndexOf('/', scheme + 3);
            if (path < 0)
                return null;
            trimmed = trimmed[path..];
        }
        var start = trimmed.LastIndexOfAny(['/', '\\', ':']) + 1;
        var last = trimmed[start..];
        if (last.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            last = last[..^4];
        return string.IsNullOrWhiteSpace(last) ? null : last;
    }

    public static string Sanitize(string name) => name.Replace('.', '_').Replace(':', '_');

    private static string FolderName(string path) => Path.GetFileName(Trim(path));

    private static string Trim(string path) => path.TrimEnd('/', '\\');
}
