using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class SessionNameTests
{
    [Theory]
    [InlineData("git@github.com:flatrick/claude-team.git", "claude-team")]
    [InlineData("https://github.com/flatrick/dotnet-knowledge.git", "dotnet-knowledge")]
    [InlineData("https://github.com/flatrick/dotnet-knowledge", "dotnet-knowledge")]
    [InlineData("https://github.com/flatrick/dotnet-knowledge/", "dotnet-knowledge")]
    [InlineData("ssh://git@host:2222/owner/repo.git", "repo")]
    [InlineData("git@host:repo.git", "repo")]
    [InlineData("/srv/git/local-repo.git", "local-repo")]
    [InlineData(@"C:\repos\win-repo", "win-repo")]
    [InlineData("  https://github.com/o/spaced.git  \n", "spaced")]
    public void FromRemoteUrl_returns_the_last_path_segment_without_dot_git(string url, string expected)
        => Assert.Equal(expected, SessionName.FromRemoteUrl(url));

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(".git")]
    [InlineData("https://host/")]
    public void FromRemoteUrl_returns_null_when_no_name_remains(string url)
        => Assert.Null(SessionName.FromRemoteUrl(url));

    [Theory]
    [InlineData("my.repo", "my_repo")]
    [InlineData("a:b", "a_b")]
    [InlineData("plain", "plain")]
    public void Sanitize_replaces_tmux_target_separators(string name, string expected)
        => Assert.Equal(expected, SessionName.Sanitize(name));

    [Fact]
    public void Resolve_uses_the_origin_name_in_the_main_checkout()
    {
        var git = new GitInfo("/src/local-folder", "/src/local-folder/.git", "/src/local-folder/.git",
            "git@github.com:flatrick/claude-team.git");
        Assert.Equal("claude-team", SessionName.Resolve(git, "/src/local-folder/sub"));
    }

    [Fact]
    public void Resolve_appends_the_worktree_folder_in_a_linked_worktree()
    {
        var git = new GitInfo("/src/repo/.worktrees/my-feature", "/src/repo/.git/worktrees/my-feature",
            "/src/repo/.git", "https://github.com/flatrick/claude-team.git");
        Assert.Equal("claude-team+my-feature", SessionName.Resolve(git, "/src/repo/.worktrees/my-feature"));
    }

    [Fact]
    public void Resolve_uses_the_main_checkout_folder_when_origin_is_absent()
    {
        var git = new GitInfo("/src/repo/.worktrees/wt", "/src/repo/.git/worktrees/wt", "/src/repo/.git", null);
        Assert.Equal("repo+wt", SessionName.Resolve(git, "/src/repo/.worktrees/wt"));
    }

    [Fact]
    public void Resolve_uses_the_current_folder_outside_git()
        => Assert.Equal("some_folder", SessionName.Resolve(null, "/home/u/some.folder"));

    [Fact]
    public void Resolve_sanitizes_the_worktree_part_too()
    {
        var git = new GitInfo("/r/.worktrees/v1.2", "/r/.git/worktrees/v1.2", "/r/.git", null);
        Assert.Equal("r+v1_2", SessionName.Resolve(git, "/r/.worktrees/v1.2"));
    }
}
