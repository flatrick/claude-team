using System.Reflection;

namespace ClaudeTeam;

public enum InstallOutcome { Written, KeptExisting, NoRepository }

public sealed record InstallResult(string Path, InstallOutcome Outcome);

public static class PeerRule
{
    public const string FileName = "claude-team-peer-leaders.md";
    public const string Marker = "claude-team-leader";

    public static string Text { get; } = ReadEmbedded();

    public static string RepoPath(string repoRoot) => Path.Combine(repoRoot, ".claude", "rules", FileName);

    public static string GlobalPath(string home) => Path.Combine(home, ".claude", "rules", FileName);

    public static bool IsInstalled(string? repoRoot, string home)
        => (repoRoot is not null && File.Exists(RepoPath(repoRoot))) || File.Exists(GlobalPath(home));

    public static IReadOnlyList<InstallResult> Install(InstallTarget target, string? repoRoot, string home, bool force)
    {
        List<InstallResult> results = [];
        if (target is InstallTarget.Repo or InstallTarget.Both)
            results.Add(repoRoot is null
                ? new InstallResult(Path.Combine(".claude", "rules", FileName), InstallOutcome.NoRepository)
                : Write(RepoPath(repoRoot), force));
        if (target is InstallTarget.Global or InstallTarget.Both)
            results.Add(Write(GlobalPath(home), force));
        return results;
    }

    private static InstallResult Write(string path, bool force)
    {
        if (File.Exists(path) && !force)
            return new InstallResult(path, InstallOutcome.KeptExisting);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, Text);
        return new InstallResult(path, InstallOutcome.Written);
    }

    private static string ReadEmbedded()
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(FileName)
            ?? throw new InvalidOperationException($"The embedded resource {FileName} is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }
}
