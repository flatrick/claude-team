# tooling/single-file-scripts Specification

## Purpose

This is a shared spec for the tooling area.
It defines the form of every repository tool under `scripts/`, so its requirements apply to every spec under `tooling/`.

## Requirements

### Requirement: Single-file C# tools

Each tool under `scripts/` SHALL be a single-file C# program that runs with `dotnet run --file scripts/<name>.cs`, with no shell script, batch file, or shell shim around it.
Source: `scripts/install-tool.cs` (lines 1-3: `#!/usr/bin/env dotnet`, `#:property PublishAot=false`), `.claude/rules/cross-platform-tooling.md`.
Method: `ls scripts/` lists every tool.

#### Scenario: Run on native Windows

- **WHEN** a contributor with only the .NET SDK runs `dotnet run --file scripts/install-tool.cs -- status` on Windows
- **THEN** the tool runs without Git Bash or WSL
