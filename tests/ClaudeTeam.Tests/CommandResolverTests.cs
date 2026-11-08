using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class CommandResolverTests
{
    private static CommandKind Resolve(params string[] args) => CommandResolver.Resolve(Options.Parse(args).Options!);

    [Fact]
    public void No_arguments_launch() => Assert.Equal(CommandKind.Launch, Resolve());

    [Fact]
    public void Version_alone_gives_version() => Assert.Equal(CommandKind.Version, Resolve("--version"));

    [Theory]
    [InlineData("--version", "--help")]
    [InlineData("--help", "--version")]
    [InlineData("--version", "-h")]
    public void Help_wins_over_version(params string[] args) => Assert.Equal(CommandKind.Help, Resolve(args));

    [Theory]
    [InlineData("--version", "--install-peer-rule", "repo")]
    [InlineData("--install-peer-rule", "repo", "--version")]
    [InlineData("--version", "--install-leader-agent", "global")]
    public void Version_wins_over_an_install_option(params string[] args) => Assert.Equal(CommandKind.Version, Resolve(args));

    [Fact]
    public void An_install_option_gives_install() => Assert.Equal(CommandKind.Install, Resolve("--install-peer-rule", "repo"));
}
