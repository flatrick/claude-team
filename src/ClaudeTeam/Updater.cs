namespace ClaudeTeam;

public static class Updater
{
    // Handles each shipped file in install order, at the repository root and then at the user profile root.
    public static InstallRun Run(InstallTarget target, string? repoRoot, string home, bool dryRun)
    {
        List<string> lines = [];
        var failed = false;
        foreach (var file in ShippedFiles.All)
        {
            var relative = Path.Combine(".claude", file.PublishedPath.Replace('/', Path.DirectorySeparatorChar));
            if (target is InstallTarget.Repo or InstallTarget.Both)
            {
                if (repoRoot is null)
                {
                    lines.Add($"Cannot update {relative} in the repository: the current folder is not in a git repository.");
                    failed = true;
                }
                else
                    Handle(lines, Path.Combine(repoRoot, relative), file, dryRun);
            }

            if (target is InstallTarget.Global or InstallTarget.Both)
                Handle(lines, Path.Combine(home, relative), file, dryRun);
        }

        if (target == InstallTarget.Both)
            lines.Add(Installer.MixedPrecedenceNote);
        return new InstallRun(lines, failed ? 1 : 0);
    }

    private static void Handle(List<string> lines, string path, ShippedFile file, bool dryRun)
    {
        if (!File.Exists(path))
        {
            if (!dryRun)
                Write(path, file.Text);
            lines.Add(dryRun ? $"Would add {path}" : $"Added {path}");
            return;
        }

        var installed = File.ReadAllText(path).ReplaceLineEndings("\n");
        if (installed == file.Text.ReplaceLineEndings("\n"))
        {
            lines.Add($"Up to date {path}");
            return;
        }

        if (!dryRun)
        {
            Write(path, file.Text);
            lines.Add($"Replaced {path}");
            return;
        }

        var label = ShippedHashes.Contains(file.PublishedPath, installed) ? "older shipped version" : "edited";
        lines.Add($"Would replace {path} ({label})");
        lines.AddRange(LineDiff.Unified(installed, file.Text, path, Path.GetFileName(file.PublishedPath)));
    }

    private static void Write(string path, string text)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text.ReplaceLineEndings("\n"));
    }
}
