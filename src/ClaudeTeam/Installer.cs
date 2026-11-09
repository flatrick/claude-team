using System.Reflection;

namespace ClaudeTeam;

public sealed record InstallRun(IReadOnlyList<string> Lines, int ExitCode);

public static class Installer
{
    public const string MemoryReminder =
        "Reminder: add .claude/agent-memory-local/ to the .gitignore of each repository where you run the team leader, so that the leader memory is not committed.";

    public const string MixedPrecedenceNote =
        "Note: for a name in both places, Claude Code uses the skill in the user profile and the agent in the repository. Keep both copies the same.";

    public static string ReadEmbedded(string resourceName)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The embedded resource {resourceName} is missing.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }

    public static IReadOnlyList<InstallResult> Install(
        InstallTarget target, string? repoRoot, string home, string subfolder, string fileName, string text, bool force,
        bool dryRun = false)
    {
        List<InstallResult> results = [];
        if (target is InstallTarget.Repo or InstallTarget.Both)
            results.Add(repoRoot is null
                ? new InstallResult(Path.Combine(".claude", subfolder, fileName), InstallOutcome.NoRepository)
                : Write(Path.Combine(repoRoot, ".claude", subfolder, fileName), text: text, force: force, dryRun: dryRun));
        if (target is InstallTarget.Global or InstallTarget.Both)
            results.Add(Write(Path.Combine(home, ".claude", subfolder, fileName), text: text, force: force, dryRun: dryRun));
        return results;
    }

    public static InstallRun Run(
        InstallTarget? peerRule, InstallTarget? leaderAgent, string? repoRoot, string home, bool force, bool dryRun)
    {
        List<InstallResult> results = [];
        if (peerRule is { } ruleTarget)
            results.AddRange(PeerRule.Install(target: ruleTarget, repoRoot: repoRoot, home: home, force: force, dryRun: dryRun));
        if (leaderAgent is { } agentTarget)
        {
            results.AddRange(LeaderAgent.Install(target: agentTarget, repoRoot: repoRoot, home: home, force: force, dryRun: dryRun));
            results.AddRange(TeamSkills.Install(target: agentTarget, repoRoot: repoRoot, home: home, force: force, dryRun: dryRun));
        }
        var lines = results.Select(result => result.Outcome switch
        {
            InstallOutcome.Written => dryRun ? $"Would write {result.Path}" : $"Wrote {result.Path}",
            InstallOutcome.KeptExisting => dryRun
                ? $"Would keep the existing {result.Path}. Use --force to replace it."
                : $"Kept the existing {result.Path}. Use --force to replace it.",
            _ => $"Cannot install {result.Path} to the repository: the current folder is not in a git repository.",
        }).ToList();
        if (leaderAgent is not null)
            lines.Add(MemoryReminder);
        if (leaderAgent == InstallTarget.Both)
            lines.Add(MixedPrecedenceNote);
        return new InstallRun(lines, results.Any(result => result.Outcome == InstallOutcome.NoRepository) ? 1 : 0);
    }

    private static InstallResult Write(string path, string text, bool force, bool dryRun)
    {
        if (File.Exists(path) && !force)
            return new InstallResult(path, InstallOutcome.KeptExisting);
        if (dryRun)
            return new InstallResult(path, InstallOutcome.Written);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
        return new InstallResult(path, InstallOutcome.Written);
    }
}
