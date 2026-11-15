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

var kind = CommandResolver.Resolve(options);

switch (kind)
{
    case CommandKind.Help:
        Console.WriteLine(Help.Text(backend, session));
        return 0;
    case CommandKind.Version:
        Console.WriteLine(VersionLine.Current());
        return 0;
    case CommandKind.Install:
    {
        var run = Installer.Run(
            peerRule: options.InstallPeerRule,
            leaderAgent: options.InstallLeaderAgent,
            repoRoot: git?.Toplevel,
            home: home,
            force: options.Force,
            dryRun: options.DryRun);

        foreach (var line in run.Lines)
            Console.WriteLine(line);

        return run.ExitCode;
    }
    case CommandKind.Update:
    {
        var run = Updater.Run(
            target: options.Update!.Value,
            repoRoot: git?.Toplevel,
            home: home,
            dryRun: options.DryRun);

        foreach (var line in run.Lines)
            Console.WriteLine(line);

        return run.ExitCode;
    }
    case CommandKind.Launch:
        break;
    default:
        throw new ArgumentOutOfRangeException();
}

var projectRoot = git?.Toplevel ?? workingDirectory;
var preflight = LaunchOutput.Preflight(projectRoot, git?.Toplevel, home);
var insidePane = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TMUX"));
var inputs = new LaunchInputs(
    Session: session,
    WorkingDirectory: workingDirectory,
    IsWindows: isWindows,
    InsidePane: insidePane,
    HasTeamLeaderAgent: preflight.HasAgent,
    Forwarded: options.Forwarded);
if (LaunchPlan.Validate(inputs) is { } inputError)
{
    Console.Error.WriteLine(inputError);
    return 2;
}

var plan = LaunchPlan.Build(inputs);

LaunchOutput.Write(
    Console.Error, Console.Out, preflight.Warnings, options.DryRun ? LaunchOutput.DryRunLines(session, plan) : null);
if (options.DryRun)
    return 0;

switch (Multiplexer.Check(backend, session))
{
    case SessionState.BackendMissing:
        Console.Error.WriteLine($"Cannot run '{backend}'. Install it and put it on PATH.");
        return Processes.NotFound;
    case SessionState.Exists:
        Console.Error.WriteLine(Multiplexer.ExistsMessage(backend, session));
        return 1;
    case SessionState.Absent:
        break;
    default:
        throw new ArgumentOutOfRangeException();
}

if (!isWindows && Multiplexer.CheckVersion() is { } versionError)
{
    Console.Error.WriteLine(versionError);
    return 1;
}

// A running tmux server does not take the variable from the client, so new-session -e passes it too.
var environment = new Dictionary<string, string> { [LaunchPlan.TeamsVariable] = "1" };
foreach (var command in plan.Commands)
{
    var exitCode = Processes.Run(command, environment);
    if (exitCode != 0)
        return exitCode;
}

return 0;
