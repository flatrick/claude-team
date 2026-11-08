# tooling/release/build-version Specification

## Purpose
This spec defines how every build of `claude-team` gets its version from git tags. It lets a maintainer tell builds apart by version and by commit.

## Requirements

### Requirement: Version comes from git tags

The build SHALL take the version of `claude-team` from the git tags with the prefix `v`, and the project file SHALL NOT set a fixed version.
On a commit that carries a tag `vX.Y.Z`, the version SHALL be `X.Y.Z`.
Source: `src/ClaudeTeam/ClaudeTeam.csproj` (the MinVer `PackageReference` and `MinVerTagPrefix`; no `Version` element).

#### Scenario: Build on a tagged commit

- **WHEN** the build runs on the commit that carries the tag `v0.2.0`
- **THEN** the package version is `0.2.0`

### Requirement: Version between releases

The build SHALL give a build after the latest tag the next patch version, the pre-release label `alpha.0`, and the number of commits since that tag, for example `0.1.1-alpha.0.3` three commits after `v0.1.0`.
The label SHALL mean "built after the latest release". It does not predict the next release version.
Source: `src/ClaudeTeam/ClaudeTeam.csproj`. This is the default behavior of MinVer.

#### Scenario: Build after a release

- **WHEN** the build runs three commits after the tag `v0.1.0`
- **THEN** the package version is `0.1.1-alpha.0.3`

### Requirement: Informational version holds the commit

The assembly informational version SHALL hold the build version, then `+`, then the commit of the build.
Source: `src/ClaudeTeam/ClaudeTeam.csproj`. The .NET SDK adds the commit through `IncludeSourceRevisionInInformationalVersion`.

#### Scenario: Two worktrees with the same height

- **WHEN** two worktree builds have the same number of commits since the latest tag
- **THEN** the two informational versions have the same version part and different commits
