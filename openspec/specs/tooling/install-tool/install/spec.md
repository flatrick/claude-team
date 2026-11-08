# tooling/install-tool/install Specification

## Purpose

This spec defines the `install` command of `scripts/install-tool.cs`. The command packs `claude-team` from the checkout and installs it as a user-global .NET tool.

## Requirements

### Requirement: Install packs the checkout

The `install` command SHALL do these steps:

- warn if the checkout has uncommitted changes
- delete any `.nupkg` file in `<checkout>/.scratch/nuget-local`
- pack `src/ClaudeTeam/ClaudeTeam.csproj` in the Release configuration into that folder

If a step fails, the command SHALL stop with the exit code of that step.
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

The `install` command SHALL install the build of the current checkout, whatever version is installed.
If another version or no version is installed, the command SHALL run `dotnet tool update` with the option `--allow-downgrade`.
The update runs with `--global claude-team`, the pack folder as a source, and the packed version.
A failed step stops the command with its exit code.
Source: `scripts/install-tool.cs` (`RunInstall`, `GetInstalledVersion`).

#### Scenario: Lower packed version

- **WHEN** version `0.1.1-alpha.0.7` is installed and the checkout packs `0.1.1-alpha.0.3`
- **THEN** the script runs `dotnet tool update` with `--allow-downgrade`
- **AND** version `0.1.1-alpha.0.3` is installed
- **AND** the script prints no warning that the new version is lower

### Requirement: Install reinstalls the same version

If the same version is installed, the `install` command SHALL run `dotnet tool uninstall` and then `dotnet tool install`.
Each command runs with `--global claude-team`. The uninstall takes no source.
The install adds the pack folder as a source and the packed version.
A failed step stops the command with its exit code.
Source: `scripts/install-tool.cs` (`RunInstall`, `GetInstalledVersion`).

#### Scenario: Unchanged version

- **WHEN** version `0.1.0` is installed and the checkout packs `0.1.0`
- **THEN** the script uninstalls the tool and installs the new package

### Requirement: Install verifies the shim

After a successful install, the `install` command SHALL run the PATH check.
If the tool shim is missing from `~/.dotnet/tools`, the command SHALL exit with code 1.
Otherwise, the command SHALL print the installed version, commit, and branch, and exit with code 0.
Source: `scripts/install-tool.cs` (`RunInstall`, `GetShimPath`).

#### Scenario: Shim missing

- **WHEN** `dotnet tool` reports success but `~/.dotnet/tools/claude-team` does not exist on Linux
- **THEN** the script writes `Install reported success but shim not found at: <path>` to standard error and exits with code 1
