using ClaudeTeam;

namespace ClaudeTeam.Tests;

public sealed class ShippedCopyTests
{
    public static TheoryData<string> PublishedPaths
    {
        get
        {
            var data = new TheoryData<string>();
            foreach (var file in ShippedFiles.All)
                data.Add(file.PublishedPath);
            return data;
        }
    }

    [Theory]
    [MemberData(nameof(PublishedPaths))]
    public void Repository_copy_matches_the_embedded_source(string publishedPath)
    {
        var shipped = ShippedFiles.All.Single(file => file.PublishedPath == publishedPath);
        var path = Path.Combine(FindRepositoryRoot(), ".claude", publishedPath.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(path), $"The repository copy {path} is missing.");
        Assert.Equal(shipped.Text.ReplaceLineEndings("\n"), File.ReadAllText(path).ReplaceLineEndings("\n"));
    }

    private static string FindRepositoryRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "ClaudeTeam.slnx")))
                return dir.FullName;
        throw new InvalidOperationException("ClaudeTeam.slnx not found above the test output directory.");
    }
}
