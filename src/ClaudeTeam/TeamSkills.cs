namespace ClaudeTeam;

public static class TeamSkills
{
    public const string FileName = "SKILL.md";

    // Install order.
    public static IReadOnlyList<string> Names { get; } = ["team-protocol", "start-teammate", "end-teammate"];

    public static string ResourceName(string name) => $"skills/{name}/{FileName}";

    public static string Text(string name) => Installer.ReadEmbedded(ResourceName(name));

    public static string RepoPath(string repoRoot, string name) => Path.Combine(repoRoot, ".claude", "skills", name, FileName);

    public static string GlobalPath(string home, string name) => Path.Combine(home, ".claude", "skills", name, FileName);

    public static IReadOnlyList<InstallResult> Install(InstallTarget target, string? repoRoot, string home, bool force, bool dryRun = false)
        => Names.SelectMany(name => Installer.Install(
            target, repoRoot, home, subfolder: Path.Combine("skills", name), fileName: FileName, text: Text(name), force: force, dryRun: dryRun))
            .ToList();

    // One warning that names each team skill that is in neither location, or null when none is missing.
    public static string? MissingWarning(string projectRoot, string home)
    {
        var missing = Names.Where(name => !File.Exists(RepoPath(projectRoot, name)) && !File.Exists(GlobalPath(home, name))).ToList();
        return missing.Count == 0
            ? null
            : $"Warning: the team skills {string.Join(", ", missing)} are not in .claude/skills/ or ~/.claude/skills/. " +
              "Install them with: claude-team --install-leader-agent <repo|global|both>";
    }
}
