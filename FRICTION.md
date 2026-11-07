# Friction log

## F-001 — Copied rules name files that this repository does not have

- Date: 2026-11-07
- Observation: The untracked `.claude/rules/` files came from another repository.
  `testing-discipline.md`, `development.md`, and `dotnet/build-parallelism.md` require `scripts/run-tests.cs`, `test.runsettings`, `DotNetMcpServer.slnx`, Reqnroll E2E tests, and Stryker runs.
- Evidence: `ls scripts/run-tests.cs test.runsettings DotNetMcpServer.slnx` in the main checkout returned "No such file or directory" for all three.
- Effect: The work on `feat/claude-team-tool` could not follow these rules as written.
  It used plain `dotnet build` and `dotnet test` with logs in `.scratch/`, per the user's global CLAUDE.md.
- Cause: inferred. The files were copied from the dotnet-mcp repository without adaptation.
- Status: open. The user decides later which copied files to keep.
