using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class UpdaterTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");
    private string Repo => Path.Combine(_root.FullName, "repo");
    private string Home => Path.Combine(_root.FullName, "home");

    public void Dispose() => _root.Delete(recursive: true);

    private static string PathUnder(string root, ShippedFile file)
        => Path.Combine(root, ".claude", file.PublishedPath.Replace('/', Path.DirectorySeparatorChar));

    private static ShippedFile Shipped(string publishedPath) => ShippedFiles.All.Single(file => file.PublishedPath == publishedPath);

    private void InstallAll(string root)
    {
        foreach (var file in ShippedFiles.All)
        {
            var path = PathUnder(root, file);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, file.Text);
        }
    }

    private static string[] AllFiles(string root)
        => Directory.Exists(root)
            ? Directory.GetFiles(root, "*", SearchOption.AllDirectories).Order().ToArray()
            : [];

    [Fact]
    public void Mixed_states_add_replace_and_keep()
    {
        InstallAll(Repo);
        var protocol = PathUnder(Repo, Shipped("skills/team-protocol/SKILL.md"));
        var start = PathUnder(Repo, Shipped("skills/start-teammate/SKILL.md"));
        var end = PathUnder(Repo, Shipped("skills/end-teammate/SKILL.md"));
        File.Delete(protocol);
        File.WriteAllText(start, "old text\n");
        var stamp = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        File.SetLastWriteTimeUtc(end, stamp);

        var run = Updater.Run(InstallTarget.Repo, Repo, Home, dryRun: false);

        Assert.Equal(0, run.ExitCode);
        Assert.Contains($"Added {protocol}", run.Lines);
        Assert.Contains($"Replaced {start}", run.Lines);
        Assert.Contains($"Up to date {end}", run.Lines);
        Assert.Equal(Shipped("skills/team-protocol/SKILL.md").Text, File.ReadAllText(protocol));
        Assert.Equal(Shipped("skills/start-teammate/SKILL.md").Text, File.ReadAllText(start));
        Assert.Equal(stamp, File.GetLastWriteTimeUtc(end));
        Assert.Equal(ShippedFiles.All.Count, run.Lines.Count);
    }

    [Fact]
    public void A_file_that_differs_only_in_line_endings_counts_as_the_same()
    {
        InstallAll(Home);
        var path = PathUnder(Home, Shipped("agents/team-leader.md"));
        File.WriteAllText(path, Shipped("agents/team-leader.md").Text.ReplaceLineEndings("\r\n"));
        var before = File.ReadAllBytes(path);

        var run = Updater.Run(InstallTarget.Global, Repo, Home, dryRun: false);

        Assert.Contains($"Up to date {path}", run.Lines);
        Assert.Equal(before, File.ReadAllBytes(path));
    }

    [Fact]
    public void Written_files_have_lf_line_endings_only()
    {
        Updater.Run(InstallTarget.Repo, Repo, Home, dryRun: false);
        foreach (var file in AllFiles(Repo))
            Assert.DoesNotContain((byte)'\r', File.ReadAllBytes(file));
    }

    [Fact]
    public void Dry_run_writes_nothing_and_labels_each_change()
    {
        InstallAll(Repo);
        var protocol = PathUnder(Repo, Shipped("skills/team-protocol/SKILL.md"));
        var leader = PathUnder(Repo, Shipped("agents/team-leader.md"));
        var rule = PathUnder(Repo, Shipped("rules/claude-team-peer-leaders.md"));
        File.Delete(protocol);
        File.WriteAllText(leader, Shipped("agents/team-leader.md").Text.Replace("effort: low", "effort: high"));
        File.WriteAllText(rule, "an old rule\n");
        var before = AllFiles(Repo).ToDictionary(file => file, File.ReadAllText);

        var run = Updater.Run(InstallTarget.Repo, Repo, Home, dryRun: true);

        Assert.Equal(0, run.ExitCode);
        Assert.Contains($"Would add {protocol}", run.Lines);
        Assert.Contains($"Would replace {leader} (edited)", run.Lines);
        Assert.Contains($"Would replace {rule} (edited)", run.Lines);
        Assert.Contains($"Up to date {PathUnder(Repo, Shipped("skills/end-teammate/SKILL.md"))}", run.Lines);
        var at = run.Lines.ToList().IndexOf($"Would replace {leader} (edited)");
        Assert.Equal($"--- {leader}", run.Lines[at + 1]);
        Assert.Equal("+++ team-leader.md (shipped)", run.Lines[at + 2]);
        Assert.Contains("-effort: high", run.Lines);
        Assert.Contains("+effort: low", run.Lines);
        Assert.Equal(before.Keys.Order().ToArray(), AllFiles(Repo));
        Assert.All(before, pair => Assert.Equal(pair.Value, File.ReadAllText(pair.Key)));
    }

    [Fact]
    public void An_earlier_shipped_text_is_labeled_older_shipped_version()
    {
        InstallAll(Repo);
        var leader = PathUnder(Repo, Shipped("agents/team-leader.md"));
        File.WriteAllText(leader, EarlierLeaderText);

        var run = Updater.Run(InstallTarget.Repo, Repo, Home, dryRun: true);

        Assert.Contains($"Would replace {leader} (older shipped version)", run.Lines);
    }

    [Fact]
    public void A_difference_only_in_the_final_newline_shows_a_diff_and_is_replaced()
    {
        InstallAll(Repo);
        var file = Shipped("skills/end-teammate/SKILL.md");
        var path = PathUnder(Repo, file);
        File.WriteAllText(path, file.Text.TrimEnd('\n'));

        var dry = Updater.Run(InstallTarget.Repo, Repo, Home, dryRun: true);
        var at = dry.Lines.ToList().FindIndex(line => line.StartsWith($"Would replace {path} ("));
        Assert.True(at >= 0);
        Assert.Equal($"--- {path}", dry.Lines[at + 1]);
        Assert.Contains("\\ No newline at end of file", dry.Lines);
        Assert.Contains(dry.Lines.Skip(at), line => line.StartsWith('-') && !line.StartsWith("---"));
        Assert.Contains(dry.Lines.Skip(at), line => line.StartsWith('+') && !line.StartsWith("+++"));

        var run = Updater.Run(InstallTarget.Repo, Repo, Home, dryRun: false);
        Assert.Contains($"Replaced {path}", run.Lines);
        Assert.Equal(file.Text, File.ReadAllText(path));
    }

    [Fact]
    public void Both_handles_the_repository_file_before_the_user_profile_file_and_ends_with_the_note()
    {
        var run = Updater.Run(InstallTarget.Both, Repo, Home, dryRun: false);

        List<string> expected = [];
        foreach (var file in ShippedFiles.All)
        {
            expected.Add($"Added {PathUnder(Repo, file)}");
            expected.Add($"Added {PathUnder(Home, file)}");
        }
        expected.Add(Installer.MixedPrecedenceNote);
        Assert.Equal(expected, run.Lines);
        Assert.Equal(0, run.ExitCode);
    }

    [Fact]
    public void Single_targets_print_no_note_and_no_reminder()
    {
        var run = Updater.Run(InstallTarget.Global, Repo, Home, dryRun: true);
        Assert.DoesNotContain(run.Lines, line => line.StartsWith("Note:") || line.StartsWith("Reminder:"));
    }

    [Fact]
    public void Outside_git_reports_each_repository_file_updates_the_profile_and_exits_1()
    {
        var run = Updater.Run(InstallTarget.Both, null, Home, dryRun: false);

        Assert.Equal(1, run.ExitCode);
        foreach (var file in ShippedFiles.All)
        {
            Assert.Contains(run.Lines, line =>
                line.Contains(Path.Combine(".claude", file.PublishedPath.Replace('/', Path.DirectorySeparatorChar)))
                && line.Contains("not in a git repository"));
            Assert.Equal(file.Text, File.ReadAllText(PathUnder(Home, file)));
        }
    }

    [Fact]
    public void An_update_writes_no_file_other_than_the_shipped_files()
    {
        Updater.Run(InstallTarget.Both, Repo, Home, dryRun: false);
        var expected = ShippedFiles.All.Select(file => file.PublishedPath).Order().ToArray();
        foreach (var root in new[] { Repo, Home })
            Assert.Equal(
                expected,
                AllFiles(root).Select(file => Path.GetRelativePath(Path.Combine(root, ".claude"), file).Replace('\\', '/')).Order().ToArray());
    }

    // The text that releases v0.1.0 and v0.2.0 shipped. Re-derive: git show v0.2.0:src/ClaudeTeam/team-leader.md
    private static string EarlierLeaderText => """
            ---
            name: team-leader
            description: Leads a native Claude agent team. Coordinates and delegates; never implements.
            ---

            You coordinate and delegate. You do not implement, edit, build, or test.

            If you have no goal, ask the operator for one before you do anything else.

            ## Start a teammate

            1. Check that `CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` is `1`: run `printenv CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` on macOS or Linux, or read `$env:CLAUDE_CODE_EXPERIMENTAL_AGENT_TEAMS` in PowerShell. If it is not `1`, stop and tell the operator. Do not start a subagent instead.
            2. Call the Agent tool with `name` and `subagent_type`. Pass no `isolation`. Do not use `subagent_type: "fork"`.
            3. Start one teammate at a time. Wait for its first message, or check that ListAgents lists it, before you start the next.
            4. Use a definition from `.claude/agents/` or `~/.claude/agents/` when one fits the work. Otherwise use `general-purpose`.
            5. Bounded read-only lookups, for example `Explore`, may run as plain subagents.

            ## Write each assignment

            Name these items:

            - the absolute worktree path
            - the task
            - the done criteria
            - the allowed scope
            - whether the teammate may start subagents, and how many

            Give each parallel editor its own git worktree. Two editors never share one.

            Tell every teammate:

            - Your first action is ToolSearch with the query `select:SendMessage`.
            - Report a blocker or a question to the leader at once with SendMessage.
            - To answer a shutdown request, call SendMessage with `message` as an object, not a string: `{"type": "shutdown_response", "request_id": "<id from the request>", "approve": true}`.

            ## End a teammate

            Shut a teammate down when its work is done. Call SendMessage with `message` as an object, not a string: `{"type": "shutdown_request", "reason": "<short reason>"}`.

            ## Decisions and shared state

            Decisions belong to the operator. Ask short, concrete questions and wait for the answer.
            Never commit, push, or change shared or remote state without the approval of the operator.

            If the rule `claude-team-peer-leaders.md` is installed, follow it.

            """;
}
