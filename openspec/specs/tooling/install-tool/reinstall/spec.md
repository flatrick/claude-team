# tooling/install-tool/reinstall Specification

## Purpose

Defines the `reinstall` command of `scripts/install-tool.cs`, which stops running `claude-team` instances and then runs `install`.

## Requirements

### Requirement: Reinstall stops running instances first

The `reinstall` command SHALL stop each running process whose executable is the tool shim and then run `install`.
It SHALL identify processes by the main module path on Windows, by the `/proc/<pid>/exe` link on Linux, and by `ps -Ao pid=,comm=` on other platforms, and SHALL wait up to 10 seconds for each stopped process.
Source: `scripts/install-tool.cs` (`RunReinstall`, `StopRunningInstances`, `FindShimProcesses`, `StopTimeoutMilliseconds`).

#### Scenario: No running instance

- **WHEN** no process runs the shim
- **THEN** the script prints `No running claude-team instances.` and runs `install`

### Requirement: Stop failure per platform

When a shim process cannot be stopped or the process list cannot be read, the script SHALL exit with code 1 on Windows, because Windows locks the running shim.
On other platforms, it SHALL print a warning and continue with `install`.
Source: `scripts/install-tool.cs` (`ReportStopFailure`).

#### Scenario: Unstoppable instance on Windows

- **WHEN** a shim process does not exit within 10 seconds on Windows
- **THEN** the script exits with code 1 without installing

#### Scenario: Unstoppable instance on Linux

- **WHEN** a shim process does not exit within 10 seconds on Linux
- **THEN** the script prints a warning that the instance keeps its old build
- **AND** it continues with `install`
