namespace ClaudeTeam;

public static class LeaderAgent
{
    public const string FileName = "team-leader.md";

    public static string Text { get; } = Installer.ReadEmbedded(FileName);

    public static string RepoPath(string repoRoot) => Path.Combine(repoRoot, ".claude", "agents", FileName);

    public static string GlobalPath(string home) => Path.Combine(home, ".claude", "agents", FileName);

    public static bool IsInstalled(string? repoRoot, string home)
        => (repoRoot is not null && File.Exists(RepoPath(repoRoot))) || File.Exists(GlobalPath(home));

    public static IReadOnlyList<InstallResult> Install(InstallTarget target, string? repoRoot, string home, bool force, bool dryRun = false)
        => Installer.Install(target, repoRoot, home, subfolder: "agents", fileName: FileName, text: Text, force: force, dryRun: dryRun);
}
