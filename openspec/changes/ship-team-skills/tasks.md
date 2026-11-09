# Tasks

Load the `dotnet-workflow` skill before the first dotnet command. Write each failing test before the code that makes it pass (`.claude/rules/development.md`).

## 1. Shipped skill sources and embedding

- [ ] 1.1 Write a failing test that reads each team skill from the embedded resources (`skills/<skill name>/SKILL.md`) in the order `team-protocol`, `start-teammate`, `end-teammate`, and checks that the text has LF line endings only. Verify that it fails because the resources do not exist.
- [ ] 1.2 Add `src/ClaudeTeam/skills/<skill name>/SKILL.md` for each team skill (start from the current repository text), embed them in `ClaudeTeam.csproj`, and add the `TeamSkills` type (design decision 1). Verify that the test from 1.1 passes.

## 2. Shipped text content

- [ ] 2.1 Write failing content tests for the embedded agent and skills. They cover the spec `launcher/starter-files/content`: the local extension line as the first non-empty line after the front matter, the forbidden strings, the leader front matter (`effort: low`, an `initialPrompt` that names `team-protocol`, no `model:`, no `memory:`), the `+local` confirmations, the leader's use of `start-teammate` and `end-teammate` with no `tmux list-panes` in the agent, the neutral subagent and report text, the psmux forms with `not tested on Windows`, the PowerShell forms, and the statement that the checks decide a start or a shutdown. Replace the agent assertions in `LeaderAgentTests.cs` that the new tests cover. Verify that the new tests fail.
- [ ] 2.2 Rewrite `src/ClaudeTeam/team-leader.md` as design decisions 2, 4, and 7 say. Verify that the agent content tests pass.
- [ ] 2.3 Rewrite `src/ClaudeTeam/skills/team-protocol/SKILL.md` as design decisions 2, 6, and 7 say. Verify that its content tests pass.
- [ ] 2.4 Rewrite `src/ClaudeTeam/skills/start-teammate/SKILL.md` and `src/ClaudeTeam/skills/end-teammate/SKILL.md` as design decisions 2, 5, and 7 say. Verify that their content tests pass.
- [ ] 2.5 Check the shipped text against `.claude/rules/writing-style.md` and record the check in the ledger. Verify that every content test still passes.

## 3. Install of the skills with the agent

- [ ] 3.1 Write failing tests in `InstallerTests.cs` and `LeaderAgentTests.cs`: `--install-leader-agent repo` writes the agent and then the three skills; `both` writes the repository file before the user profile file for each file and prints the `Note:` line last; `repo` and `global` print no `Note:` line; a dry run prints the `Note:` line and writes nothing; an existing skill is kept without `--force` and replaced with it; outside git, each repository skill gives the outside-git line and the exit code is 1; no `-local` skill and no other agent is written. Verify that they fail.
- [ ] 3.2 Extend `LeaderAgent.Install` or `Installer.Run` to install the skills, and add the `Note:` line (design decision 3). Verify that the tests from 3.1 and every earlier install test pass.
- [ ] 3.3 Write a failing test in `HelpTests.cs` that the text for `--install-leader-agent` names the three skills. Then update `Help.cs`. Verify that the test passes.

## 4. Launch warnings

- [ ] 4.1 Write failing tests for a testable warning function (for example `TeamSkills.LaunchWarnings(projectRoot, home)`): one missing-skill warning that names each missing skill and `--install-leader-agent`; no warning when each skill is in one location; a shadowed-skill warning that names the user profile path when both copies exist and differ; no shadowed-skill warning when the copies differ only in line endings. Verify that they fail.
- [ ] 4.2 Implement the function and call it from `Program.cs` after the peer-rule check, before the dry run (design decision 3). Verify that the tests from 4.1 pass, and that `claude-team --dry-run` in a temporary folder with no skills prints the missing-skill warning before the dry-run output.

## 5. Repository copies and local skills

- [ ] 5.1 Generalize `PeerRuleCopyTests.cs` into one copy test with a case for the peer rule, the agent, and each team skill. Verify that the agent and skill cases fail.
- [ ] 5.2 In one commit, copy the shipped agent and skills to `.claude/agents/team-leader.md` and `.claude/skills/<skill name>/SKILL.md`, and create `.claude/skills/team-leader-local/SKILL.md`, `.claude/skills/team-protocol-local/SKILL.md`, and `.claude/skills/start-teammate-local/SKILL.md` with the moved text from design decision 7. Verify that the copy test passes, and that each row of the design table has its text in the named destination (`rg --hidden -n` for a distinctive phrase of each row).
- [ ] 5.3 Search for other files that quote the moved text or the old leader front matter (`rg --hidden --glob '!.git/**' -n 'model: opus|agent-spawn-log|dotnet-workflow loaded' .claude/ README.md AGENTS.md`). Update or confirm each hit, and record the command and its result in the ledger.

## 6. Documentation

- [ ] 6.1 Update `README.md`: the install section names the skills, the `Note:` line, and the launch warnings; a short section describes the `-local` extension point and says that a `-local` skill lives in the repository. Verify that each command in the new text matches the help output of `claude-team --help` from `dotnet run --project src/ClaudeTeam -- --help`.

## 7. Windows check (needs a human)

- [ ] 7.1 NEEDS A HUMAN ON WINDOWS. Run the psmux pane checks of `start-teammate`, the psmux check and the PowerShell wait loop of `end-teammate`, and the PowerShell variable check under psmux with PowerShell 7. Record the commands and the output in the ledger. For each form that works, remove the words `not tested on Windows` in the shipped source and the repository copy, in an OpenSpec change if the shipped text is behavior. Report each form that does not work to the team lead.

## 8. Final checks

- [ ] 8.1 Run `openspec validate ship-team-skills --strict`, `openspec validate --all --strict`, and `openspec validate --archived`. Verify that all three pass with no warnings.
- [ ] 8.2 Run `dotnet test ClaudeTeam.slnx` as the `dotnet-workflow` skill says. Verify that every test passes.

## Workflow follow-up

- Run `/opsx:verify` before the PR into main. It includes the code review.
- A global tool install to try the new build needs the approval of the operator and a check with the other leaders (`.claude/rules/claude-team-peer-leaders.md`).
- Archive the change with `/opsx:archive` in the worktree before the branch merges.
