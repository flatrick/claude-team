namespace ClaudeTeam;

public enum InstallTarget { Repo, Global, Both }

public sealed record Options(
    string? Session,
    bool DryRun,
    bool Help,
    string[] Forwarded,
    InstallTarget? InstallPeerRule,
    InstallTarget? InstallLeaderAgent,
    bool Force)
{
    public static ParseResult Parse(string[] args)
    {
        string? session = null;
        InstallTarget? install = null, installAgent = null;
        bool dryRun = false, help = false, force = false;
        string[] forwarded = [];

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--":
                    forwarded = args[(i + 1)..];
                    i = args.Length;
                    break;
                case "--session":
                    if (i + 1 >= args.Length || args[i + 1] == "--")
                        return ParseResult.Fail("Supply a name after --session.");
                    session = args[++i];
                    if (string.IsNullOrWhiteSpace(session)
                        || session.Any(character => char.IsControl(character) || character is '.' or ':'))
                        return ParseResult.Fail("Use a nonempty session name without control characters, periods, or colons.");
                    break;
                case "--install-peer-rule":
                    if (i + 1 >= args.Length || !Enum.TryParse<InstallTarget>(args[i + 1], ignoreCase: true, out var target))
                        return ParseResult.Fail("Supply repo, global, or both after --install-peer-rule.");
                    install = target;
                    i++;
                    break;
                case "--install-leader-agent":
                    if (i + 1 >= args.Length || !Enum.TryParse<InstallTarget>(args[i + 1], ignoreCase: true, out var agentTarget))
                        return ParseResult.Fail("Supply repo, global, or both after --install-leader-agent.");
                    installAgent = agentTarget;
                    i++;
                    break;
                case "--force":
                    force = true;
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                case "-h" or "--help":
                    help = true;
                    break;
                default:
                    return ParseResult.Fail($"Unknown option '{args[i]}'. Put Claude arguments after --.");
            }
        }

        if (force && install is null && installAgent is null)
            return ParseResult.Fail("Use --force only with --install-peer-rule or --install-leader-agent.");

        return new ParseResult(new Options(session, dryRun, help, forwarded, install, installAgent, force), null);
    }
}

public sealed record ParseResult(Options? Options, string? Error)
{
    public static ParseResult Fail(string error) => new(null, error);
}
