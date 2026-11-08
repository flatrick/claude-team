# Changelog
## v0.2.0 - 2026-11-08

### Build

- Stamp the version from git tags with MinVer

### Documentation

- **openspec:** Propose versioning change
- **readme:** Describe the release workflow and --version
- **openspec:** Archive versioning

### Features

- **cli:** Add the --version option
- **tooling:** Show the installed commit in install-tool status
- **tooling:** Add the release script
## v0.1.0 - 2026-11-08

### Bug Fixes

- **launcher:** Accept only repo, global, or both as install target

### Documentation

- **claude:** Cite the claude-team launcher, not a script
- Add friction log with F-001
- Add friction log entries F-002, F-003, and F-004
- Add friction log entry F-005
- Reword the F-001 source in the friction log
- **rules:** Adopt decision-file rule for operator decisions
- **openspec:** Add baseline specs for current claude-team behavior
- **rules:** Route every /opsx decision through decisions.md
- **instructions:** Replace session-coordination with skills
- **readme:** Add README and remove .claude/session-launcher.md
- **friction:** Remove entries F1-F12 from a different repository
- **rules:** Add ASD-STE100 writing-style rule
- **readme:** Rewrite AGENTS.md and README.md in STE100
- **skills:** Rewrite skills and agent briefs in STE100
- **code:** Rewrite code comments in STE100
- **specs:** Rewrite OpenSpec specs in STE100
- **rules:** Rewrite rule files in STE100
- **skills:** Load dotnet build rules through dotnet-workflow
- **rules:** Load the observations rule in every session
- **rules:** Give each development rule one home
- **rules:** Move the gate-run rules into dotnet-workflow
- **skills:** Delete dotnet-testing
- **openspec:** Make each config citation cover its claim
- **agents:** Point the leader brief to the peer-leaders rule
- **agents:** Load dotnet-workflow before the first message
- **friction:** Make FRICTION.md the one friction log

### Features

- Add claude-team dotnet tool
- **launcher:** Add optional starter team-leader install

### Miscellaneous

- **git:** Initial setup of git repo
- **git:** Ignore scratch, artifacts, worktrees, and build output
- **claude:** Adapt .claude rules, skills, and agents to claude-team
- **scripts:** Add install-tool script and reference it
- **scripts:** Make install-tool.cs executable
- **git:** Ignore local agent memory
- **openspec:** Add OpenSpec scaffold, config, and workflow rules

### Testing

- Check the .claude peer rule copy against the source
