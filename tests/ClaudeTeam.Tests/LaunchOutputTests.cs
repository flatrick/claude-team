using System.Text;
using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class LaunchOutputTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("claude-team-tests-");
    private string Repo => Path.Combine(_root.FullName, "repo");
    private string Home => Path.Combine(_root.FullName, "home");

    public void Dispose() => _root.Delete(recursive: true);

    // Writes each line to a shared log with a tag, so that the order across two writers is visible.
    private sealed class TaggedWriter(List<string> log, string tag) : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;

        public override void WriteLine(string? value) => log.Add($"{tag}: {value}");
    }

    private static LaunchPlan PlanFor(bool hasAgent)
        => LaunchPlan.Build(new LaunchInputs("s", "/w", IsWindows: false, InsidePane: false, hasAgent, []));

    [Fact]
    public void Preflight_gives_the_agent_rule_and_skill_warnings_in_that_order()
    {
        var preflight = LaunchOutput.Preflight(Repo, Repo, Home);

        Assert.False(preflight.HasAgent);
        Assert.Equal(3, preflight.Warnings.Count);
        Assert.Contains("team-leader agent", preflight.Warnings[0]);
        Assert.Contains(PeerRule.FileName, preflight.Warnings[1]);
        Assert.Contains("team skills", preflight.Warnings[2]);
    }

    [Fact]
    public void Preflight_gives_no_warning_when_everything_is_installed()
    {
        Installer.Run(peerRule: InstallTarget.Repo, leaderAgent: InstallTarget.Repo, repoRoot: Repo, home: Home, force: false, dryRun: false);

        var preflight = LaunchOutput.Preflight(Repo, Repo, Home);

        Assert.True(preflight.HasAgent);
        Assert.Empty(preflight.Warnings);
    }

    [Fact]
    public void Dry_run_writes_every_warning_before_the_dry_run_output()
    {
        var preflight = LaunchOutput.Preflight(Repo, Repo, Home);
        List<string> log = [];

        LaunchOutput.Write(
            new TaggedWriter(log, "err"), new TaggedWriter(log, "out"), preflight.Warnings,
            LaunchOutput.DryRunLines("s", PlanFor(preflight.HasAgent)));

        Assert.Equal(["err", "err", "err"], log.Take(3).Select(line => line[..3]));
        Assert.All(log.Skip(3), line => Assert.StartsWith("out: ", line));
        Assert.Equal("out: session: s", log[3]);
        Assert.Equal($"out: {LaunchPlan.TeamsVariable}=1", log[4]);
    }

    [Fact]
    public void A_launch_without_dry_run_writes_the_warnings_and_no_output()
    {
        List<string> log = [];

        LaunchOutput.Write(new TaggedWriter(log, "err"), new TaggedWriter(log, "out"), ["Warning: x"], dryRunLines: null);

        Assert.Equal(["err: Warning: x"], log);
    }
}
