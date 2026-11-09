namespace ClaudeTeam;

public sealed record LaunchPreflight(bool HasAgent, IReadOnlyList<string> Warnings);

public static class LaunchOutput
{
    // The warnings for a missing agent, a missing peer-leader rule, and missing team skills, in this order.
    public static LaunchPreflight Preflight(string projectRoot, string? repoRoot, string home)
    {
        List<string> warnings = [];
        var hasAgent = LeaderAgent.IsInstalled(projectRoot, home);
        if (!hasAgent)
            warnings.Add("Warning: no team-leader agent in .claude/agents/ or ~/.claude/agents/. Starting claude without --agent team-leader. " +
                "Install a starter with: claude-team --install-leader-agent <repo|global|both>");
        if (!PeerRule.IsInstalled(repoRoot, home))
            warnings.Add($"Warning: the peer-leader rule {PeerRule.FileName} is not in .claude/rules/ or ~/.claude/rules/. " +
                "Install it with: claude-team --install-peer-rule <repo|global|both>");
        if (TeamSkills.MissingWarning(projectRoot, home) is { } skillWarning)
            warnings.Add(skillWarning);
        return new LaunchPreflight(hasAgent, warnings);
    }

    public static IReadOnlyList<string> DryRunLines(string session, LaunchPlan plan)
    {
        List<string> lines = [$"session: {session}", $"{LaunchPlan.TeamsVariable}=1"];
        lines.AddRange(plan.Commands.Select(command =>
            string.Join(' ', new[] { command.File }.Concat(command.Arguments).Select(a => a.Contains(' ') ? $"'{a}'" : a))));
        return lines;
    }

    // The warnings always come first. The dry-run lines, when given, come after them.
    public static void Write(TextWriter error, TextWriter output, IReadOnlyList<string> warnings, IReadOnlyList<string>? dryRunLines)
    {
        foreach (var warning in warnings)
            error.WriteLine(warning);
        if (dryRunLines is null)
            return;
        foreach (var line in dryRunLines)
            output.WriteLine(line);
    }
}
