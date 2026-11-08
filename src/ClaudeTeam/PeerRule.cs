namespace ClaudeTeam;

public enum InstallOutcome { Written, KeptExisting, NoRepository }

public sealed record InstallResult(string Path, InstallOutcome Outcome);

public static class PeerRule
{
    public const string FileName = "claude-team-peer-leaders.md";
    public const string Marker = "claude-team-leader";

    public static string Text { get; } = Installer.ReadEmbedded(FileName);

    public static string RepoPath(string repoRoot) => Path.Combine(repoRoot, ".claude", "rules", FileName);

    public static string GlobalPath(string home) => Path.Combine(home, ".claude", "rules", FileName);

    public static bool IsInstalled(string? repoRoot, string home)
        => (repoRoot is not null && File.Exists(RepoPath(repoRoot))) || File.Exists(GlobalPath(home));

    public static IReadOnlyList<InstallResult> Install(InstallTarget target, string? repoRoot, string home, bool force, bool dryRun = false)
        => Installer.Install(target, repoRoot, home, subfolder: "rules", fileName: FileName, text: Text, force: force, dryRun: dryRun);
}
