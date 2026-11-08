## MODIFIED Requirements

### Requirement: Status

The `status` command SHALL print these items:

- the installed global version of `claude-team`, or that it is not installed
- the commit of the installed build, taken from the output of `claude-team --version`, or `unknown` when the tool cannot report it
- the current branch of the checkout, or `(detached HEAD)`
- a note, if the checkout has uncommitted changes
- a reminder that a running leader keeps its old build until it restarts
- the PATH check
- the usage

The `status` command SHALL write nothing and exit with code 0.
Source: `scripts/install-tool.cs` (`RunStatus`, `GetInstalledVersion`, `GetInstalledCommit`, `GetCurrentBranch`, `IsDirty`).

#### Scenario: Not installed

- **WHEN** `dotnet tool list --global` does not list `claude-team`
- **THEN** the script prints `Global tool: not installed.`
- **AND** the script prints no commit

#### Scenario: Installed build reports its commit

- **WHEN** `claude-team --version` prints `claude-team 0.1.1-alpha.0.3+bf37b92`
- **THEN** `status` shows the commit `bf37b92` as the commit of the installed build

#### Scenario: Installed build cannot report a commit

- **WHEN** the tool is installed but `claude-team --version` fails or prints no commit
- **THEN** `status` shows the commit as `unknown`
- **AND** the exit code is 0
