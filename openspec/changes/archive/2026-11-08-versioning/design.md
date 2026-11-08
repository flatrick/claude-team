## Context

`src/ClaudeTeam/ClaudeTeam.csproj` sets `<Version>0.1.0</Version>`. The commit `bf37b92` carries the annotated tag `v0.1.0`.
`cliff.toml` was written by `gir` and groups commits by Conventional Commit type.
GitHub allows only "Rebase and merge", which writes new SHAs on merge.
`scripts/install-tool.cs` packs the checkout and installs it as a global tool. The operator decisions are in `proposal.md`.

## Goals and non-goals

Goals:

- Each build has a version and a commit that identify it.
- A release is a tag on `origin/main` and a changelog.
- One engine (`git-cliff`) decides the bump.

Non-goals: see "Out of scope" in `proposal.md`.

## Design

### Build version: MinVer

- Add `<PackageReference Include="MinVer" PrivateAssets="all" />` to `src/ClaudeTeam/ClaudeTeam.csproj`. Add `<MinVerTagPrefix>v</MinVerTagPrefix>`. Remove `<Version>`.
- MinVer keeps its default patch auto-increment and its default pre-release label `alpha.0`. After `v0.1.0`, a build three commits later is `0.1.1-alpha.0.3`.
- The next release can be `0.2.0`, so the label does not predict it. It only says "built after 0.1.0". The operator accepted this.
- The informational version is the build version, then `+`, then the commit. The .NET SDK has the target `AddSourceRevisionToInformationalVersion` in `Microsoft.NET.GenerateAssemblyInfo.targets`. It runs when `IncludeSourceRevisionInInformationalVersion` is true, which is the default. It reads `SourceRevisionId`, which the SDK file `Microsoft.Build.Tasks.Git.targets` sets from the git repository.
  A build shows the full commit. See "Measured items", item 2.

### `--version`

- `Options` gets a `Version` flag. `Parse` accepts `--version` only in the long form.
- `CommandResolver.Resolve` picks the command: `Help` first, then `Version`, then install, then launch. `Program.cs` runs the command that it returns, before any launch. So help wins when both are given, and a parse error exits with code 2 first.
- The line is `claude-team <version>`. The version is the `AssemblyInformationalVersionAttribute` value. The tool shortens the commit part after `+` to 7 characters, because the SDK writes the full commit.
  Reason: the operator wants the short commit, and the same length as `git rev-parse --short` in most repositories.
- `Help.cs` lists `--version` in the options section.

### `install-tool.cs status`

- `status` runs `claude-team --version` in `GetInstalledCommit`, and parses the text after `+`. It does not use `RunProcessCapture`, because that helper throws `Win32Exception` when the tool is not on `PATH`, and `status` must print `unknown` then.
- If the tool is not installed, `status` prints no commit. If the command fails or prints no `+`, `status` prints `unknown`.
- `GetVersionFromNupkg` and `ParseInstalledVersion` already pass any text through. A package version such as `0.1.1-alpha.0.3` has no spaces, so both functions work. The implementer confirms this with the build (see tasks).
- `install` passes `--allow-downgrade` to `dotnet tool update`, so it installs the build of the current checkout, whatever version is installed. The `install` spec has a delta spec for this. The `reinstall` and `uninstall` specs do not change.
  - Reason: each worktree has its own MinVer version. `dotnet tool update claude-team --tool-path .scratch/tool --add-source .scratch/pack-low --version 0.1.1-alpha.0.3` over `0.1.1-alpha.0.7` printed "The requested version 0.1.1-alpha.0.3 is lower than existing version 0.1.1-alpha.0.7." and exited with code 1. `dotnet tool update --help` on SDK 10.0.114 lists `--allow-downgrade`. The implementer measured this (ledger entry `impl-tool-update-refuses-downgrade`).
  - Accepted cost: an install from an old worktree replaces a newer build with no warning. `claude-team --version` and `status` show what is installed.
  - With MinVer, a new commit changes the version, so `install` mostly takes the `tool update` path.
  - A dirty tree on the same commit gives the same version as the commit. The same-version path (uninstall, then install) and the cache eviction are still needed. The `reinstall` workaround is also still needed, because Windows locks the running shim. These claims are NOT RE-DERIVED: they come from reading `scripts/install-tool.cs`, not from a run.

### Release script `scripts/release.cs`

A single-file C# program (`scripts/release.cs`, `RunPrepare`). It starts with `#!/usr/bin/env dotnet` and a usage header, and runs child processes with `ProcessStartInfo` and `ArgumentList`, as `.claude/rules/cross-platform-tooling.md` requires. It runs on native Windows. Arguments go after `--`.

`prepare` (on a branch):

1. Check the start state: clean tree, not detached, not `main`, `git-cliff` on `PATH`, and at least one commit after the latest `v*` tag.
2. Find the latest `v*` tag with `git describe --tags --abbrev=0 --match "v*"`.
3. Choose the bump argument. Major 0 gives `--bump minor`. Major 1 or higher gives `--bump` with no value (automatic).
   For `--set-version X.Y.Z`, check that the version is higher than the latest tag. git-cliff 2.14.2 rejects `--bump 1.0.0`, because `--bump` accepts only `major`, `minor`, `patch`, and `auto`. So the script uses `vX.Y.Z` as the tag and gives no bump argument.
4. Get the version: run `git-cliff <bump argument> --bumped-version`. For `--set-version`, skip this step. The version is `vX.Y.Z`.
5. Write the changelog: `git-cliff --tag vX.Y.Z -o CHANGELOG.md`.
6. Commit `CHANGELOG.md` with the subject `chore(release): vX.Y.Z`.

Why major 0 always gives a minor bump: the operator wants `0.x.0` releases whatever the commit types, and wants to choose `1.0.0` himself. The script passes `--bump minor` and git-cliff still decides. No second engine exists.

`tag` (in the main checkout, after `git pull --ff-only`):

1. Check the start state: on `main`, clean tree.
2. Run `git fetch origin`. Fetch changes only remote-tracking refs, never the working tree.
3. Check that `HEAD` equals `origin/main`.
4. Check that the subject of `HEAD` matches `chore(release): vX.Y.Z`, and that the tag does not exist.
5. Run `git tag -a vX.Y.Z -m "claude-team X.Y.Z"`.
6. Print `git push origin vX.Y.Z`. Do not push.

Why the tag is made after the merge: a rebase merge gives the release commit a new SHA. A tag made on the branch commit would point to a commit that is not on `main`.

### `cliff.toml`

Add a parser entry `{ message = "^chore\\(release\\)", skip = true }` before the `chore` entry, so the release commit does not appear in the next changelog.
This changes a file that `gir` generated. `gir init` can overwrite `cliff.toml`. The implementer checks this and reports it if it happens.

### Workflow

1. Work on a branch in a worktree. Use Conventional Commits, which `gir` enforces.
2. Before the pull request, the operator cleans up the commits (fixup or amend with autosquash, or a soft reset and new commits).
3. When the branch is ready, run `prepare` on it. Then open the pull request and use "Rebase and merge".
4. In the main checkout, run `git pull --ff-only`, then `tag`. The operator pushes the tag.

## Writes outside the repository

- `scripts/release.cs` writes only inside the checkout: `CHANGELOG.md`, the git commit, and the git tag. `git fetch origin` writes to `.git` of the checkout. It writes nothing outside.
- Build and pack with MinVer write NuGet package files to the global-packages folder (`~/.nuget/packages`). `dotnet restore` does this already today.
- `install-tool.cs` keeps its existing writes (`openspec/specs/tooling/install-tool/install/spec.md`).
- No new write goes to `~`, `/tmp`, or a system folder.

## Windows and non-Windows

- `release.cs` and `install-tool.cs` use `ProcessStartInfo` with `ArgumentList` and no shell. The same code runs on both.
- The commit part of the version is a hex string, so the output is the same on both.
- `git-cliff` must be on `PATH` on both. On Windows it is `git-cliff.exe`. `dotnet run --file` starts `git-cliff` by name, which resolves `.exe` on Windows.

## Risks and trade-offs

- MinVer needs full tag history. A shallow clone gives version `0.0.0-alpha.0`. Local releases only, so the risk is small. A future CI job must fetch tags.
- `v0.1.0` stays reachable only if the tag is not deleted. Tags on `origin/main` commits are kept.
- A second `prepare` run before the tag exists adds a second `chore(release)` commit. The operator removes the first one with `git reset` and runs `prepare` again.
- If the operator changes the commit history after `prepare`, the changelog can be stale. The operator runs `prepare` again after a reset.

## Measured items

The implementer measured these items. The ledger entries hold the commands and the output (`.scratch/agent-ledgers/versioning/`).

1. `git-cliff 2.14.2` with `--bump minor` gives `v0.2.0` after a `feat` commit that follows `v0.1.0`. Re-derive it with `git-cliff --bumped-version --bump minor` (ledger key `impl-cliff-bump-minor`).
2. The informational version of a build has `+<full commit>`. The built DLL has `0.1.1-alpha.0.1+b4e8c645231bf69da006c379585021b7898ea38f`, and `claude-team --version` prints `claude-team 0.1.1-alpha.0.1+b4e8c64` (ledger key `impl-informational-version-full-commit`).
3. The commit in the informational version has 40 characters, so the shortening code stays (same ledger key as item 2).
4. `dotnet tool update` with a lower version fails without `--allow-downgrade` (ledger key `impl-tool-update-refuses-downgrade`, described in "`install-tool.cs status`"). NOT RE-DERIVED: nobody has run `install` with `--allow-downgrade`. Task 4.3 does this with `--tool-path` in `.scratch/`.
5. The pre-release version in the `.nupkg` file name does not break `GetVersionFromNupkg`. The file is `claude-team.0.1.1-alpha.0.7.nupkg`, and the regex `^claude-team\.(.+)\.nupkg$` captures `0.1.1-alpha.0.7` (ledger key `impl-pack-version-form`).
