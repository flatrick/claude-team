## MODIFIED Requirements

### Requirement: Status

The `status` command SHALL print the installed global version, or that it is not installed.
It SHALL print the current branch, or `(detached HEAD)`, and a note if the checkout has uncommitted changes.
It SHALL print a reminder that a running leader keeps its old build until it restarts.
It SHALL print the PATH check and the usage.
It SHALL write nothing to standard error and exit with code 0.
Source: `scripts/install-tool.cs` (`RunStatus`, `GetInstalledVersion`, `GetCurrentBranch`, `IsDirty`).

#### Scenario: Not installed

- **WHEN** `dotnet tool list --global` does not list `claude-team`
- **THEN** the script prints `Global tool: not installed.`
- **AND** the script prints no commit

## ADDED Requirements

### Requirement: Status reports the installed commit

If the tool is installed, the `status` command SHALL print the commit of the installed build.
It SHALL take the commit from the output of `claude-team --version`.
It SHALL print `unknown` when the tool cannot report a commit.
Source: `scripts/install-tool.cs` (`RunStatus`, `GetInstalledCommit`).

#### Scenario: Installed build reports its commit

- **WHEN** `claude-team --version` prints `claude-team 0.1.1-alpha.0.3+bf37b92`
- **THEN** `status` shows the commit `bf37b92` as the commit of the installed build

#### Scenario: Installed build cannot report a commit

- **WHEN** the tool is installed but `claude-team --version` fails or prints no commit
- **THEN** `status` shows the commit as `unknown`
- **AND** the exit code is 0
