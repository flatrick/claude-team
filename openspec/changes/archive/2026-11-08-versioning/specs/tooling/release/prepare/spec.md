## Purpose

This spec defines the `prepare` mode of `scripts/release.cs`. The mode computes the next version, writes the changelog, and commits both on a branch, so that the release commit reaches main through a pull request.

## ADDED Requirements

### Requirement: Release script modes

The script SHALL accept exactly one mode, `prepare` or `tag`, and run with `dotnet run --file scripts/release.cs -- <mode>`.
For no mode, an unknown mode, or an extra argument, the script SHALL print the usage to standard error and exit with code 1.
Source: `scripts/release.cs` (the `args` switch, `Usage`). The form follows `openspec/specs/tooling/single-file-scripts/spec.md`.

#### Scenario: Unknown mode

- **WHEN** the script runs with `-- publish`
- **THEN** standard error shows the usage
- **AND** the exit code is 1

### Requirement: Prepare computes the next version with git-cliff

The `prepare` mode SHALL ask `git-cliff` for the next version, and SHALL NOT compute a version itself.
While the major version of the latest `v*` tag is 0, the mode SHALL ask for a minor bump, whatever the commit types are.
From major version 1, the mode SHALL ask for the automatic bump of git-cliff, so a breaking change gives a new major version.
Source: `scripts/release.cs` (`RunPrepare`, the bump choice). Config: `cliff.toml`.

#### Scenario: Fix commits only, major version 0

- **WHEN** the latest tag is `v0.2.0` and the commits since the tag are all `fix` commits
- **THEN** the next version is `v0.3.0`

#### Scenario: Breaking change, major version 1

- **WHEN** the latest tag is `v1.0.0` and a commit since the tag has a breaking change
- **THEN** the next version is `v2.0.0`

### Requirement: Explicit version

The `prepare` mode SHALL accept `--set-version <X.Y.Z>` and SHALL use that version in place of the computed one.
The operator uses this option to choose `1.0.0`.
The mode SHALL reject a value that is not `X.Y.Z`, or that is not higher than the latest tag.
Source: `scripts/release.cs` (`RunPrepare`, `ParseVersion`, the `--set-version` branch).

#### Scenario: Choose 1.0.0

- **WHEN** the latest tag is `v0.9.0` and the script runs with `-- prepare --set-version 1.0.0`
- **THEN** the release version is `v1.0.0`

#### Scenario: Version not higher

- **WHEN** the latest tag is `v0.9.0` and the script runs with `-- prepare --set-version 0.8.0`
- **THEN** the script writes an error to standard error, changes no file, and exits with code 1

### Requirement: Prepare writes the changelog and commits

The `prepare` mode SHALL write `CHANGELOG.md` at the checkout root with `git-cliff` and the existing `cliff.toml`, with the release version as the tag of the newest section.
The mode SHALL then commit `CHANGELOG.md` with the subject `chore(release): vX.Y.Z`.
The mode SHALL NOT create a tag and SHALL NOT push.
Source: `scripts/release.cs` (`RunPrepare`, the `git-cliff --tag` and `git commit` steps).

#### Scenario: Successful prepare

- **WHEN** the script runs `-- prepare` on a clean branch, and the next version is `0.2.0`
- **THEN** `CHANGELOG.md` has a section whose heading starts with `## v0.2.0`
- **AND** the newest commit has the subject `chore(release): v0.2.0`
- **AND** no tag is created

### Requirement: Prepare checks its start state

The `prepare` mode SHALL exit with code 1 and change no file when any of these is true:

- the working tree has uncommitted changes
- the checkout is on a detached HEAD
- the checkout is on the `main` branch
- no commit exists after the latest `v*` tag
- `git-cliff` is not on `PATH`

Each case SHALL print a message that names the cause to standard error.
Source: `scripts/release.cs` (`RunPrepare`, the start-state checks).

#### Scenario: Prepare on main

- **WHEN** the script runs `-- prepare` and the current branch is `main`
- **THEN** standard error names the cause
- **AND** the exit code is 1

#### Scenario: Nothing to release

- **WHEN** the script runs `-- prepare` and `HEAD` has the latest tag
- **THEN** standard error says that no commits follow the latest tag
- **AND** the exit code is 1
