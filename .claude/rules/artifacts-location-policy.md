# Temporary artifacts location policy

## Rule

Write temporary artifacts under `.scratch/<kind>/<files...>` at the root of the checkout where you work.
Temporary artifacts are logs, test output, reports, scratch folders, and generated files.
Do not write them to the repository root or to a committed path.

`<kind>` is a short, stable folder name, for example `logs/`, `agent-ledgers/`, or `<task>/`.

## Committed content never references `.scratch/`

Added: 2026-11-25.

Treat all content under `.scratch/` as throwaway content.
Content that you commit to git MUST NOT reference a file under `.scratch/`.
This applies to tracked files and to commit messages.

If committed content needs a fact or a conclusion from `.scratch/`, use one of these two forms:

- **Durable.** Copy the necessary text into a durable home: the committed artifact itself, a different committed file, an issue, or a PR. Then cite that home.
- **Re-derivable.** Give the command or the steps that re-create the fact or the conclusion from durable inputs. Examples are an `rg` invocation, a script in the repository, and a named test.

Do not write a `.scratch/` path as a citation in either form.
[observations-must-be-re-derivable.md](observations-must-be-re-derivable.md) says how to write a re-derivation.
If neither form is possible, use the `NOT RE-DERIVED` label from that rule.

- Text that only names the place where a role or a tool writes temporary files is not a reference.
- A short-lived handoff that you do not commit is allowed, for example an assignment that names a report path under `.scratch/`.

## Why

Git does not track `.scratch/`, and anyone can delete its content.
A reader of the committed text then cannot get the content that the text cites.

## Examples

- Build log: `.scratch/logs/build-20261107-1432.log`
- Worker report: `.scratch/<task>/report.md`

## Notes

- `.scratch/` is ignored by git. It is a local-only scratch area.
- Confirm the ignore rule once per checkout with `git check-ignore -q .scratch/probe.log`.
- Prefer one logical run per log file.
- Keep a log until you need disk space.
- Decision files are an exception to the location rule. They live in `.claude/decisions/`, not in `.scratch/`. See [decisions.md](decisions.md).
