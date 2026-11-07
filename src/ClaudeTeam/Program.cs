using ClaudeTeam;

var parsed = Options.Parse(args);
if (parsed.Options is not { } options)
{
    Console.Error.WriteLine(parsed.Error);
    Console.Error.WriteLine("Run 'claude-team --help' for usage.");
    return 2;
}

var isWindows = OperatingSystem.IsWindows();
var backend = LaunchPlan.BackendFor(isWindows);
var workingDirectory = Environment.CurrentDirectory;
var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var git = Git.Read();
var session = options.Session ?? SessionName.Resolve(git, workingDirectory);

if (options.Help)
{
    Console.WriteLine(Help.Text(backend, session));
    return 0;
}

if (options.InstallPeerRule is { } target)
{
    var failed = false;
    foreach (var result in PeerRule.Install(target, git?.Toplevel, home, options.Force))
    {
        Console.WriteLine(result.Outcome switch
        {
            InstallOutcome.Written => $"Wrote {result.Path}",
            InstallOutcome.KeptExisting => $"Kept the existing {result.Path}. Use --force to replace it.",
            _ => "Cannot install to the repository: the current folder is not in a git repository.",
        });
        failed |= result.Outcome == InstallOutcome.NoRepository;
    }
    return failed ? 1 : 0;
}

var projectRoot = git?.Toplevel ?? workingDirectory;
var hasAgent = File.Exists(Path.Combine(projectRoot, ".claude", "agents", "team-leader.md"))
    || File.Exists(Path.Combine(home, ".claude", "agents", "team-leader.md"));
if (!hasAgent)
    Console.Error.WriteLine("Warning: no team-leader agent in .claude/agents/ or ~/.claude/agents/. Starting claude without --agent team-leader.");
if (!PeerRule.IsInstalled(git?.Toplevel, home))
    Console.Error.WriteLine($"Warning: the peer-leader rule {PeerRule.FileName} is not in .claude/rules/ or ~/.claude/rules/. " +
        "Install it with: claude-team --install-peer-rule <repo|global|both>");

var insidePane = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TMUX"));
var plan = LaunchPlan.Build(new LaunchInputs(session, workingDirectory, isWindows, insidePane, hasAgent, options.Forwarded));

if (options.DryRun)
{
    Console.WriteLine($"session: {session}");
    Console.WriteLine($"{LaunchPlan.TeamsVariable}=1");
    foreach (var command in plan.Commands)
        Console.WriteLine(string.Join(' ', new[] { command.File }.Concat(command.Arguments).Select(a => a.Contains(' ') ? $"'{a}'" : a)));
    return 0;
}

switch (Multiplexer.Check(backend, session))
{
    case SessionState.BackendMissing:
        Console.Error.WriteLine($"Cannot run '{backend}'. Install it and put it on PATH.");
        return Processes.NotFound;
    case SessionState.Exists:
        Console.Error.WriteLine(Multiplexer.ExistsMessage(backend, session));
        return 1;
}

// A running tmux server does not take the variable from the client, so new-session -e also passes it.
var environment = new Dictionary<string, string> { [LaunchPlan.TeamsVariable] = "1" };
foreach (var command in plan.Commands)
{
    var exitCode = Processes.Run(command, environment);
    if (exitCode != 0)
        return exitCode;
}
return 0;
