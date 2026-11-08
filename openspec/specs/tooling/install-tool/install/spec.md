# tooling/install-tool/install Specification

## Purpose

Defines the `install` command of `scripts/install-tool.cs`, which packs `claude-team` from the checkout and installs it as a user-global .NET tool.

## Requirements

### Requirement: Install packs the checkout

The `install` command SHALL warn when the checkout has uncommitted changes, delete any `.nupkg` file in `<checkout>/.scratch/nuget-local`, and pack `src/ClaudeTeam/ClaudeTeam.csproj` in the Release configuration into that folder.
A failed step SHALL stop the command with that step's exit code.
Source: `scripts/install-tool.cs` (`RunInstall`, `GetVersionFromNupkg`).

#### Scenario: Pack fails

- **WHEN** `dotnet pack` exits with code 1
- **THEN** the script writes `pack failed (exit 1).` to standard error and exits with code 1

### Requirement: Install evicts the cached package

The `install` command SHALL remove the packed version's folder from the NuGet global-packages cache before it installs.
Source: `scripts/install-tool.cs` (`RunInstall`, `ParseNugetLocalsOutput`).

#### Scenario: Cached version present

- **WHEN** the global-packages cache holds `claude-team/0.1.0` and the checkout packs `0.1.0`
- **THEN** the script deletes that folder and prints `Evicted stale cache entry: <path>`

### Requirement: Install replaces the global tool

When another version or none is installed, the `install` command SHALL run `dotnet tool update`.
When the same version is installed, it SHALL run `dotnet tool uninstall`, which takes no source, and then `dotnet tool install`.
Each of these runs with `--global claude-team`. The update and the install add the pack folder as a source and the packed version. A failed step stops the command with its exit code.
Source: `scripts/install-tool.cs` (`RunInstall`, `GetInstalledVersion`).

#### Scenario: Unchanged version

- **WHEN** version `0.1.0` is installed and the checkout packs `0.1.0`
- **THEN** the script uninstalls the tool and installs the new package

### Requirement: Install verifies the shim

After a successful install, the `install` command SHALL run the PATH check and SHALL exit with code 1 when the tool shim is missing from `~/.dotnet/tools`.
Otherwise it SHALL print the installed version, commit, and branch, and exit with code 0.
Source: `scripts/install-tool.cs` (`RunInstall`, `GetShimPath`).

#### Scenario: Shim missing

- **WHEN** `dotnet tool` reports success but `~/.dotnet/tools/claude-team` does not exist on Linux
- **THEN** the script writes `Install reported success but shim not found at: <path>` to standard error and exits with code 1
