# Temporary artifacts location policy

## Rule

Write temporary artifacts under `.scratch/<kind>/<files...>` at the root of the checkout where you work.
Temporary artifacts are logs, test output, reports, scratch folders, and generated files.
Do not write them to the repository root or to a committed path.

`<kind>` is a short, stable folder name, for example `logs/`, `agent-ledgers/`, or `<task>/`.

## Examples

- Build log: `.scratch/logs/build-20261107-1432.log`
- Worker report: `.scratch/<task>/report.md`

## Notes

- `.scratch/` is ignored by git. It is a local-only scratch area.
- Confirm the ignore rule once per checkout with `git check-ignore -q .scratch/probe.log`.
- Prefer one logical run per log file.
- Keep a log until you need disk space.
- Decision files are an exception. They live in `.claude/decisions/`, not in `.scratch/`. See [decisions.md](decisions.md).
