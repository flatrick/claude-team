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
