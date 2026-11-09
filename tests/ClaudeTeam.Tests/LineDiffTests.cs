using ClaudeTeam;

namespace ClaudeTeam.Tests;

public class LineDiffTests
{
    private static string Lines(int from, int to) => string.Join('\n', Enumerable.Range(from, to - from + 1).Select(n => $"line{n}")) + "\n";

    [Fact]
    public void Identical_texts_give_no_hunk()
        => Assert.Empty(LineDiff.Unified(Lines(1, 5), Lines(1, 5), "/x/SKILL.md", "SKILL.md"));

    [Fact]
    public void One_changed_line_gives_one_hunk_with_three_lines_of_context()
    {
        var installed = Lines(1, 10);
        var shipped = installed.Replace("line5\n", "changed\n");
        var diff = LineDiff.Unified(installed, shipped, "/x/SKILL.md", "SKILL.md");
        Assert.Equal(
            [
                "--- /x/SKILL.md",
                "+++ SKILL.md (shipped)",
                "@@ -2,7 +2,7 @@",
                " line2", " line3", " line4",
                "-line5",
                "+changed",
                " line6", " line7", " line8",
            ],
            diff);
    }

    [Fact]
    public void Context_stops_at_the_start_and_end_of_the_file()
    {
        var diff = LineDiff.Unified("a\nb\n", "a\nc\n", "/p", "f");
        Assert.Equal(["--- /p", "+++ f (shipped)", "@@ -1,2 +1,2 @@", " a", "-b", "+c"], diff);
    }

    [Fact]
    public void Added_and_removed_lines_are_shown()
    {
        var diff = LineDiff.Unified("a\nb\nc\n", "a\nc\nd\n", "/p", "f");
        Assert.Contains("-b", diff);
        Assert.Contains("+d", diff);
        Assert.DoesNotContain("-a", diff);
    }

    [Fact]
    public void Far_apart_changes_give_two_hunks()
    {
        var installed = Lines(1, 30);
        var shipped = installed.Replace("line3\n", "x\n").Replace("line28\n", "y\n");
        var diff = LineDiff.Unified(installed, shipped, "/p", "f");
        Assert.Equal(2, diff.Count(line => line.StartsWith("@@ ")));
    }

    [Fact]
    public void Line_endings_do_not_count_as_a_difference()
        => Assert.Empty(LineDiff.Unified("a\r\nb\r\n", "a\nb\n", "/p", "f"));

    [Fact]
    public void A_missing_final_newline_is_a_visible_difference()
    {
        var diff = LineDiff.Unified("a\nb", "a\nb\n", "/p", "f");
        Assert.Equal(
            [
                "--- /p", "+++ f (shipped)", "@@ -1,2 +1,2 @@",
                " a",
                "-b",
                "\\ No newline at end of file",
                "+b",
            ],
            diff);
    }

    [Fact]
    public void An_extra_final_newline_is_a_visible_difference()
    {
        var diff = LineDiff.Unified("a\nb\n", "a\nb", "/p", "f");
        Assert.Equal(
            [
                "--- /p", "+++ f (shipped)", "@@ -1,2 +1,2 @@",
                " a",
                "-b",
                "+b",
                "\\ No newline at end of file",
            ],
            diff);
    }

    [Fact]
    public void Same_text_without_a_final_newline_gives_no_hunk()
        => Assert.Empty(LineDiff.Unified("a\nb", "a\nb", "/p", "f"));
}
