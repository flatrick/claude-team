## Why

The project has one fixed version, `0.1.0`, in `src/ClaudeTeam/ClaudeTeam.csproj`.
Two worktree builds with different code carry the same version number.
A maintainer cannot tell which build is installed, and nothing records a release.
The operator wants a repeatable local release process with a clean changelog.

## Decisions

Each decision comes from the operator. The reason follows each decision.

1. **Commits.** `gir` enforces Conventional Commits. Before each pull request, the operator cleans up the branch commits (fixup or amend with autosquash, or a soft reset and new commits).
   Reason: a clean linear history gives a clean changelog.
2. **Merge.** Keep GitHub "Rebase and merge". It is the only enabled method. It writes new SHAs on merge, so a tag on a branch commit is lost. Tags go only on commits that are already on `origin/main`.
3. **Bump engine.** `git-cliff` with the existing `cliff.toml` is the only place that decides the next version. Do not add GitVersion.
   Reason: with one engine, the bump rules cannot disagree.
4. **Build version.** MinVer stamps every build from git tags with the prefix `v`. Remove the fixed `<Version>` from the csproj. Keep the default patch auto-increment of MinVer between releases, for example `0.1.1-alpha.0.N` after `v0.1.0`. The label means "built after 0.1.0".
5. **Local releases only.** A release is a git tag and `CHANGELOG.md`. Add no CI, no GitHub Release, and no NuGet.org publish for now.
   Reason: the operator first wants to improve the tool and choose a better name.
6. **Release script.** `scripts/release.cs` is a single-file C# program, as the cross-platform rule requires. Mode `prepare` runs on a branch. Mode `tag` runs in the main checkout. The operator pushes the tag.
7. **First tag.** `v0.1.0` already exists on commit `bf37b92`. This change goes into release `v0.2.0`.
8. **Bump policy.** While the major version is 0, every release is a minor bump (`0.x.0`), whatever the commit types. The operator chooses `1.0.0` explicitly. From `1.0.0`, normal SemVer applies: a breaking change gives a new major version.
9. **`claude-team --version`.** It prints one line with the version and the short commit, for example `claude-team 0.1.1-alpha.0.3+bf37b92`. The value comes from the assembly informational version.
   Reason: the commit tells apart two worktree builds with the same MinVer height.
10. **`install-tool.cs status`.** It also shows the commit of the installed build. It gets the commit from `claude-team --version`.
11. **`install-tool.cs install` and downgrade.** `install` always passes `--allow-downgrade` to `dotnet tool update`, so it installs the build of the current checkout, whatever version is installed.
    Reason: with MinVer, each worktree has its own version. Without the option, an install from a branch with fewer commits after `v0.1.0` fails over a build from a branch with more commits.
    Accepted cost: an install from an old worktree replaces a newer build with no warning. `claude-team --version` and `status` show what is installed.

The operator did not reject these defaults for `--version`:

- Only the long form `--version`. No short alias.
- Like `--help`, parsing must succeed first. The tool prints to standard output, exits with code 0, and starts no install or launch.
- If `--help` and `--version` are both given, help wins.
- The help text lists `--version`.

## What changes

- Remove `<Version>` from `src/ClaudeTeam/ClaudeTeam.csproj`. Add a MinVer package reference with tag prefix `v`.
- Add the option `--version` to `claude-team`, and list it in the help text.
- Change `scripts/install-tool.cs status` to show the commit of the installed build.
- Change `scripts/install-tool.cs install` to pass `--allow-downgrade` to `dotnet tool update`.
- Add `scripts/release.cs` with the modes `prepare` and `tag`.
- Add `CHANGELOG.md`, which `prepare` writes.
- Change `cliff.toml` so that the changelog skips `chore(release)` commits.

## Capabilities

### New capabilities

- `tooling/release/build-version`: how every build gets its version from git tags.
- `tooling/release/prepare`: the `prepare` mode of `scripts/release.cs`, and the bump policy.
- `tooling/release/tag`: the `tag` mode of `scripts/release.cs`.

### Modified capabilities

- `launcher/command-line`: add the `--version` option. Change the accepted options list.
- `tooling/install-tool/commands`: `status` shows the commit of the installed build.
- `tooling/install-tool/install`: `install` always installs the build of the current checkout, also when its version is lower than the installed version.

## Impact

- Spec paths touched: `openspec/specs/launcher/command-line/spec.md`, `openspec/specs/tooling/install-tool/commands/spec.md`, `openspec/specs/tooling/install-tool/install/spec.md`.
- Spec paths added: `openspec/specs/tooling/release/build-version/spec.md`, `openspec/specs/tooling/release/prepare/spec.md`, `openspec/specs/tooling/release/tag/spec.md`.
- Code: `src/ClaudeTeam/ClaudeTeam.csproj`, `src/ClaudeTeam/Options.cs`, `src/ClaudeTeam/Program.cs`, `src/ClaudeTeam/CommandResolver.cs`, `src/ClaudeTeam/Help.cs`, `scripts/install-tool.cs`, `scripts/release.cs`, `cliff.toml`.
- Tests: `tests/ClaudeTeam.Tests/OptionsTests.cs`.
- The `reinstall` spec does not change. See the design for the reason.
- New dependency: the MinVer NuGet package, as a build-time reference only.
- New tool for the contributor who releases: `git-cliff` must be on `PATH`.

## Out of scope

- CI, GitHub Releases, and NuGet.org publish.
- A new tool name.
- GitVersion.
- A short alias for `--version`.
- A change to the `reinstall` or `uninstall` commands.
- A change to the commit rules of `gir`.
- Automation of the commit clean-up before a pull request.
