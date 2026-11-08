## 1. Confirm the measured items (see `design.md`, "Measured items")

- [x] 1.1 Confirm `git-cliff --bumped-version --bump minor` gives `v0.2.0` on a branch with a `feat` or `fix` commit after `v0.1.0`. Record the command and output in the ledger.
- [x] 1.2 After task 3.2, confirm the informational version of a build has `+<full commit>`. Record the command and output. If the commit is short, remove the shortening code from task 3.3.
- [x] 1.3 Confirm whether `dotnet tool update` with a lower version refuses a downgrade. Result: `dotnet tool update claude-team --tool-path .scratch/tool --add-source .scratch/pack-low --version 0.1.1-alpha.0.3` over `0.1.1-alpha.0.7` printed "The requested version 0.1.1-alpha.0.3 is lower than existing version 0.1.1-alpha.0.7." and exited with code 1. `dotnet tool update --help` on SDK 10.0.114 lists `--allow-downgrade`. Task 4.3 adds the option to `install`.
- [x] 1.4 After task 3.2, run `dotnet pack` and confirm `GetVersionFromNupkg` and `ParseInstalledVersion` in `scripts/install-tool.cs` read a pre-release version such as `0.1.1-alpha.0.3`.

## 2. Build version (MinVer)

- [x] 2.1 Add the MinVer package reference with `PrivateAssets="all"` and `<MinVerTagPrefix>v</MinVerTagPrefix>` to `src/ClaudeTeam/ClaudeTeam.csproj`. Remove `<Version>0.1.0</Version>`.
- [x] 2.2 Build in Release. Confirm the package version has the form `0.1.1-alpha.0.N` and not `0.0.0-alpha.0`. A shallow clone gives `0.0.0-alpha.0`.

## 3. `claude-team --version`

- [x] 3.1 Write failing tests in `tests/ClaudeTeam.Tests/OptionsTests.cs`: `--version` sets the flag. `-v` is an unknown option. `--version --unknown` fails. `--version --help` and `--help --version` set both flags. No arguments leave the flag unset.
- [x] 3.2 Add the `Version` flag and the `--version` case to `src/ClaudeTeam/Options.cs`. Make the tests pass.
- [x] 3.3 Write a failing test for a function that builds the line `claude-team <version>` from an informational version and shortens the commit to 7 characters. Include a value with no `+`. Then write the function.
- [x] 3.4 Write a failing test that `Help.Text` lists `--version`. Then add the line to `src/ClaudeTeam/Help.cs`.
- [x] 3.5 In `src/ClaudeTeam/CommandResolver.cs`, resolve `Help` first, then `Version`, then install, before the launch code. `src/ClaudeTeam/Program.cs` runs the result. The version output goes to standard output with exit code 0.
- [x] 3.6 Run `dotnet run --project src/ClaudeTeam -- --version`. Confirm one line, exit code 0, and that no session starts. Record the output.

## 4. `install-tool.cs status`

- [x] 4.1 Add the commit of the installed build to `RunStatus` in `scripts/install-tool.cs`. Run `claude-team --version`, and parse the text after `+`. Print `unknown` when the command fails or has no `+`. Print nothing when the tool is not installed.
- [x] 4.2 Run `dotnet run --file scripts/install-tool.cs -- status`. Confirm the output. No install is needed to test the not-installed case.
- [x] 4.3 In `RunInstall` in `scripts/install-tool.cs`, pass `--allow-downgrade` to `dotnet tool update`. Add a test, or a recorded run with `--tool-path .scratch/<dir>`, that installs a lower version over a higher one. Cover the scenario "Lower packed version" in the delta spec `tooling/install-tool/install`.
- [x] 4.4 **Needs approval.** A global install changes the machine. Ask the team lead, who asks the other leaders and the operator, before you run `dotnet run --file scripts/install-tool.cs -- install`. After the install, confirm that `status` shows the commit.

## 5. Release script

- [x] 5.1 Create `scripts/release.cs` as a single-file C# program with the header and the usage that `.claude/rules/cross-platform-tooling.md` requires. Add the modes and the argument checks. The usage names the `--` form.
- [x] 5.2 Implement `prepare`: start-state checks, latest tag, bump choice (minor while major is 0, automatic from 1), `--set-version`, the version from `git-cliff --bumped-version`, `CHANGELOG.md` from `git-cliff`, and the commit `chore(release): vX.Y.Z`.
- [x] 5.3 Implement `tag`: start-state checks, `git fetch origin`, the `HEAD` equals `origin/main` check, the subject check, the existing-tag check, `git tag -a`, and the printed push command. Never push.
- [x] 5.4 Test `prepare` and `tag` in a throwaway repository under `.scratch/`, not in this repository. The repository has a bare repository as `origin`. Cover each scenario in the specs `tooling/release/prepare` and `tooling/release/tag`. Record the commands and outputs in the ledger.
- [x] 5.5 Add `{ message = "^chore\\(release\\)", skip = true }` before the `chore` parser in `cliff.toml`. Confirm that a `chore(release)` commit does not show in `git-cliff --unreleased`.
- [x] 5.6 Check that `gir` accepts `chore(release): vX.Y.Z` as a commit subject.

## 6. Documentation and finish

- [x] 6.1 Describe the release workflow in `README.md` with the four steps from the design. Cite the script. Write no history.
- [x] 6.2 Run `openspec validate versioning --strict`, `openspec validate --all --strict`, and `openspec validate --archived`. All must pass with no warnings.
- [x] 6.3 Run `dotnet test ClaudeTeam.slnx`. Load the `dotnet-workflow` skill first.
- [x] 6.4 Run `/opsx:verify`, write the result to the ledger, and then `/opsx:archive` in the worktree before the branch merges.
