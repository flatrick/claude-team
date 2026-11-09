using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class InstallerTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");
    private string Repo => Path.Combine(_root.FullName, "repo");
    private string Home => Path.Combine(_root.FullName, "home");

    public void Dispose() => _root.Delete(recursive: true);

    [Fact]
    public void Combined_run_writes_rule_and_agent()
    {
        var run = Installer.Run(peerRule: InstallTarget.Repo, leaderAgent: InstallTarget.Global, repoRoot: Repo, home: Home, force: false, dryRun: false);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal(
            [
                $"Wrote {PeerRule.RepoPath(Repo)}",
                $"Wrote {LeaderAgent.GlobalPath(Home)}",
                .. TeamSkills.Names.Select(name => $"Wrote {TeamSkills.GlobalPath(Home, name)}"),
                Installer.MemoryReminder,
            ],
            run.Lines);
        Assert.True(File.Exists(PeerRule.RepoPath(Repo)));
        Assert.True(File.Exists(LeaderAgent.GlobalPath(Home)));
    }

    [Fact]
    public void Outside_git_reports_the_path_and_exits_1()
    {
        var run = Installer.Run(peerRule: null, leaderAgent: InstallTarget.Repo, repoRoot: null, home: Home, force: false, dryRun: false);
        Assert.Equal(1, run.ExitCode);
        var line = run.Lines[0];
        Assert.Contains(Path.Combine(".claude", "agents", LeaderAgent.FileName), line);
        Assert.Contains("not in a git repository", line);
    }

    [Fact]
    public void Dry_run_writes_nothing_and_reports_what_would_happen()
    {
        var existing = PeerRule.GlobalPath(Home);
        Directory.CreateDirectory(Path.GetDirectoryName(existing)!);
        File.WriteAllText(existing, "edited");

        var run = Installer.Run(peerRule: InstallTarget.Both, leaderAgent: InstallTarget.Both, repoRoot: Repo, home: Home, force: false, dryRun: true);

        Assert.Equal(0, run.ExitCode);
        Assert.Contains($"Would write {PeerRule.RepoPath(Repo)}", run.Lines);
        Assert.Contains($"Would write {LeaderAgent.GlobalPath(Home)}", run.Lines);
        Assert.Contains(run.Lines, line => line.StartsWith($"Would keep the existing {existing}"));
        Assert.False(Directory.Exists(Repo));
        Assert.False(File.Exists(LeaderAgent.GlobalPath(Home)));
        Assert.Equal("edited", File.ReadAllText(existing));
    }

    [Fact]
    public void Dry_run_outside_git_still_exits_1()
    {
        var run = Installer.Run(peerRule: InstallTarget.Repo, leaderAgent: null, repoRoot: null, home: Home, force: false, dryRun: true);
        Assert.Equal(1, run.ExitCode);
    }

    private const string Reminder =
        "Reminder: add .claude/agent-memory-local/ to the .gitignore of each repository where you run the team leader, so that the leader memory is not committed.";

    private const string Note =
        "Note: for a name in both places, Claude Code uses the skill in the user profile and the agent in the repository. Keep both copies the same.";

    private static string[] SkillPaths(Func<string, string> path) => TeamSkills.Names.Select(path).ToArray();

    [Fact]
    public void Agent_install_to_repo_writes_the_agent_and_then_the_three_skills()
    {
        var run = Installer.Run(peerRule: null, leaderAgent: InstallTarget.Repo, repoRoot: Repo, home: Home, force: false, dryRun: false);
        Assert.Equal(0, run.ExitCode);
        string[] expected =
        [
            $"Wrote {LeaderAgent.RepoPath(Repo)}",
            .. SkillPaths(name => $"Wrote {TeamSkills.RepoPath(Repo, name)}"),
            Reminder,
        ];
        Assert.Equal(expected, run.Lines);
        Assert.All(TeamSkills.Names, name => Assert.Equal(TeamSkills.Text(name), File.ReadAllText(TeamSkills.RepoPath(Repo, name))));
    }

    [Fact]
    public void Both_target_writes_the_repository_file_before_the_user_profile_file_and_ends_with_the_note()
    {
        var run = Installer.Run(peerRule: null, leaderAgent: InstallTarget.Both, repoRoot: Repo, home: Home, force: false, dryRun: false);
        Assert.Equal(0, run.ExitCode);
        List<string> expected = [$"Wrote {LeaderAgent.RepoPath(Repo)}", $"Wrote {LeaderAgent.GlobalPath(Home)}"];
        foreach (var name in TeamSkills.Names)
        {
            expected.Add($"Wrote {TeamSkills.RepoPath(Repo, name)}");
            expected.Add($"Wrote {TeamSkills.GlobalPath(Home, name)}");
        }
        expected.Add(Reminder);
        expected.Add(Note);
        Assert.Equal(expected, run.Lines);
    }

    [Theory]
    [InlineData(InstallTarget.Repo)]
    [InlineData(InstallTarget.Global)]
    public void Single_targets_print_no_note(InstallTarget target)
    {
        var run = Installer.Run(peerRule: null, leaderAgent: target, repoRoot: Repo, home: Home, force: false, dryRun: false);
        Assert.DoesNotContain(run.Lines, line => line.StartsWith("Note:"));
        Assert.Equal(Reminder, run.Lines[^1]);
    }

    [Theory]
    [InlineData(InstallTarget.Repo)]
    [InlineData(InstallTarget.Global)]
    [InlineData(InstallTarget.Both)]
    public void Dry_run_of_the_agent_install_prints_the_reminder_and_writes_nothing(InstallTarget target)
    {
        var run = Installer.Run(peerRule: null, leaderAgent: target, repoRoot: Repo, home: Home, force: false, dryRun: true);
        Assert.Contains(Reminder, run.Lines);
        Assert.Equal(target == InstallTarget.Both, run.Lines[^1] == Note);
        Assert.False(Directory.Exists(Repo));
        Assert.False(Directory.Exists(Home));
    }

    [Fact]
    public void Peer_rule_alone_prints_no_reminder_and_no_note()
    {
        var run = Installer.Run(peerRule: InstallTarget.Both, leaderAgent: null, repoRoot: Repo, home: Home, force: false, dryRun: false);
        Assert.DoesNotContain(run.Lines, line => line.StartsWith("Reminder:") || line.StartsWith("Note:"));
    }

    [Fact]
    public void Install_writes_no_gitignore_no_local_skill_and_no_other_agent()
    {
        Directory.CreateDirectory(Repo);
        Installer.Run(peerRule: null, leaderAgent: InstallTarget.Both, repoRoot: Repo, home: Home, force: false, dryRun: false);
        foreach (var root in new[] { Repo, Home })
        {
            var files = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(file => Path.GetRelativePath(root, file).Replace('\\', '/')).Order().ToArray();
            string[] expected =
            [
                ".claude/agents/team-leader.md",
                .. TeamSkills.Names.Select(name => $".claude/skills/{name}/SKILL.md"),
            ];
            Assert.Equal(expected.Order().ToArray(), files);
        }
    }

    [Fact]
    public void An_existing_skill_is_kept_without_force_and_replaced_with_it()
    {
        var path = TeamSkills.RepoPath(Repo, "start-teammate");
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "edited");

        var kept = Installer.Run(peerRule: null, leaderAgent: InstallTarget.Repo, repoRoot: Repo, home: Home, force: false, dryRun: false);
        Assert.Contains($"Kept the existing {path}. Use --force to replace it.", kept.Lines);
        Assert.Equal("edited", File.ReadAllText(path));

        var forced = Installer.Run(peerRule: null, leaderAgent: InstallTarget.Repo, repoRoot: Repo, home: Home, force: true, dryRun: false);
        Assert.Contains($"Wrote {path}", forced.Lines);
        Assert.Equal(TeamSkills.Text("start-teammate"), File.ReadAllText(path));
    }

    [Fact]
    public void Outside_git_each_repository_skill_gives_the_outside_git_line()
    {
        var run = Installer.Run(peerRule: null, leaderAgent: InstallTarget.Repo, repoRoot: null, home: Home, force: false, dryRun: false);
        Assert.Equal(1, run.ExitCode);
        foreach (var name in TeamSkills.Names)
            Assert.Contains(run.Lines, line =>
                line.Contains(Path.Combine(".claude", "skills", name, "SKILL.md")) && line.Contains("not in a git repository"));
    }
}
