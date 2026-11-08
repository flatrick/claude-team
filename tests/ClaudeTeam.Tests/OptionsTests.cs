using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class OptionsTests
{
    [Fact]
    public void No_arguments_gives_defaults()
    {
        var options = Assert.IsType<Options>(Options.Parse([]).Options);
        Assert.Null(options.Session);
        Assert.False(options.DryRun);
        Assert.False(options.Help);
        Assert.Empty(options.Forwarded);
        Assert.Null(options.InstallPeerRule);
        Assert.False(options.Force);
    }

    [Fact]
    public void Parses_every_option_and_forwards_the_rest()
    {
        var options = Options.Parse(["--session", "x", "--dry-run", "--", "--model", "opus", "--dry-run"]).Options!;
        Assert.Equal("x", options.Session);
        Assert.True(options.DryRun);
        Assert.Equal(["--model", "opus", "--dry-run"], options.Forwarded);
    }

    [Theory]
    [InlineData("-h")]
    [InlineData("--help")]
    public void Parses_help(string flag) => Assert.True(Options.Parse([flag]).Options!.Help);

    [Theory]
    [InlineData("repo", InstallTarget.Repo)]
    [InlineData("global", InstallTarget.Global)]
    [InlineData("both", InstallTarget.Both)]
    [InlineData("BOTH", InstallTarget.Both)]
    public void Parses_install_peer_rule(string value, InstallTarget expected)
    {
        var options = Options.Parse(["--install-peer-rule", value, "--force"]).Options!;
        Assert.Equal(expected, options.InstallPeerRule);
        Assert.True(options.Force);
    }

    [Theory]
    [InlineData("repo", InstallTarget.Repo)]
    [InlineData("GLOBAL", InstallTarget.Global)]
    [InlineData("both", InstallTarget.Both)]
    public void Parses_install_leader_agent(string value, InstallTarget expected)
    {
        var options = Options.Parse(["--install-leader-agent", value, "--force"]).Options!;
        Assert.Equal(expected, options.InstallLeaderAgent);
        Assert.Null(options.InstallPeerRule);
        Assert.True(options.Force);
    }

    [Fact]
    public void Accepts_both_install_options_in_one_run()
    {
        var options = Options.Parse(["--install-peer-rule", "repo", "--install-leader-agent", "global"]).Options!;
        Assert.Equal(InstallTarget.Repo, options.InstallPeerRule);
        Assert.Equal(InstallTarget.Global, options.InstallLeaderAgent);
    }

    [Theory]
    [InlineData("--install-peer-rule", "0")]
    [InlineData("--install-peer-rule", "2")]
    [InlineData("--install-peer-rule", "7")]
    [InlineData("--install-peer-rule", "-1")]
    [InlineData("--install-peer-rule", "repo,global")]
    [InlineData("--install-peer-rule", "global,both")]
    [InlineData("--install-peer-rule", " repo ")]
    [InlineData("--install-leader-agent", "0")]
    [InlineData("--install-leader-agent", "2")]
    [InlineData("--install-leader-agent", "7")]
    [InlineData("--install-leader-agent", "-1")]
    [InlineData("--install-leader-agent", "repo,global")]
    [InlineData("--install-leader-agent", "global,both")]
    [InlineData("--install-leader-agent", " repo ")]
    public void Rejects_numeric_list_and_padded_install_targets(string option, string value)
    {
        var result = Options.Parse([option, value, "--force"]);
        Assert.Null(result.Options);
        Assert.Equal($"Supply repo, global, or both after {option}.", result.Error);
    }

    [Theory]
    [InlineData("--install-leader-agent")]
    [InlineData("--install-leader-agent", "elsewhere")]
    [InlineData("--session")]
    [InlineData("--session", "--")]
    [InlineData("--session", "a.b")]
    [InlineData("--session", "a:b")]
    [InlineData("--session", " ")]
    [InlineData("--install-peer-rule")]
    [InlineData("--install-peer-rule", "elsewhere")]
    [InlineData("--force")]
    [InlineData("--unknown")]
    public void Rejects_invalid_input(params string[] args)
    {
        var result = Options.Parse(args);
        Assert.Null(result.Options);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }
}
