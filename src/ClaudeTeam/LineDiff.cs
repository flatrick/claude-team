namespace ClaudeTeam;

public static class LineDiff
{
    private const int Context = 3;
    private const string NoNewlineMarker = "\\ No newline at end of file";

    // A last line without a final newline carries this suffix while the texts are compared,
    // so that a difference in the final newline is a difference of that line.
    private const char NoNewlineSuffix = '\0';

    private enum Kind { Same, Removed, Added }

    private readonly record struct Edit(Kind Kind, string Text);

    // Unified diff of two texts, made from a longest-common-subsequence table over lines.
    // An empty list means that the texts are the same after line-ending normalization.
    public static IReadOnlyList<string> Unified(string installed, string shipped, string installedPath, string fileName)
    {
        var edits = Compare(SplitLines(installed), SplitLines(shipped));
        if (edits.All(edit => edit.Kind == Kind.Same))
            return [];

        List<string> output = [$"--- {installedPath}", $"+++ {fileName} (shipped)"];
        var changed = Enumerable.Range(0, edits.Count).Where(i => edits[i].Kind != Kind.Same).ToList();
        var index = 0;
        while (index < changed.Count)
        {
            var start = Math.Max(0, changed[index] - Context);
            var last = changed[index];
            while (index + 1 < changed.Count && changed[index + 1] - last <= 2 * Context)
                last = changed[++index];
            var end = Math.Min(edits.Count - 1, last + Context);
            index++;
            AddHunk(output, edits, start, end);
        }

        return output;
    }

    private static void AddHunk(List<string> output, List<Edit> edits, int start, int end)
    {
        int oldStart = 1, newStart = 1;
        for (var i = 0; i < start; i++)
        {
            if (edits[i].Kind != Kind.Added) oldStart++;
            if (edits[i].Kind != Kind.Removed) newStart++;
        }

        List<string> lines = [];
        int oldCount = 0, newCount = 0;
        for (var i = start; i <= end; i++)
        {
            var edit = edits[i];
            var prefix = edit.Kind switch { Kind.Same => " ", Kind.Removed => "-", _ => "+" };
            if (edit.Text.EndsWith(NoNewlineSuffix))
            {
                lines.Add(prefix + edit.Text[..^1]);
                lines.Add(NoNewlineMarker);
            }
            else
                lines.Add(prefix + edit.Text);
            if (edit.Kind != Kind.Added) oldCount++;
            if (edit.Kind != Kind.Removed) newCount++;
        }

        output.Add($"@@ -{oldStart},{oldCount} +{newStart},{newCount} @@");
        output.AddRange(lines);
    }

    private static string[] SplitLines(string text)
    {
        var normalized = text.ReplaceLineEndings("\n");
        if (normalized.Length == 0)
            return [];
        var lines = normalized.Split('\n');
        if (normalized.EndsWith('\n'))
            return lines[..^1];
        lines[^1] += NoNewlineSuffix;
        return lines;
    }

    private static List<Edit> Compare(string[] a, string[] b)
    {
        var table = new int[a.Length + 1, b.Length + 1];
        for (var i = a.Length - 1; i >= 0; i--)
            for (var j = b.Length - 1; j >= 0; j--)
                table[i, j] = a[i] == b[j] ? table[i + 1, j + 1] + 1 : Math.Max(table[i + 1, j], table[i, j + 1]);

        List<Edit> edits = [];
        int x = 0, y = 0;
        while (x < a.Length && y < b.Length)
        {
            if (a[x] == b[y])
            {
                edits.Add(new Edit(Kind.Same, a[x]));
                x++;
                y++;
            }
            else if (table[x + 1, y] >= table[x, y + 1])
                edits.Add(new Edit(Kind.Removed, a[x++]));
            else
                edits.Add(new Edit(Kind.Added, b[y++]));
        }

        while (x < a.Length) edits.Add(new Edit(Kind.Removed, a[x++]));
        while (y < b.Length) edits.Add(new Edit(Kind.Added, b[y++]));
        return edits;
    }
}
