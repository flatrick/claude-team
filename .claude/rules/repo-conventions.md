# Repo conventions

## C# parameter ordering

Never reorder C# method parameters such that a required parameter follows an optional
one. The compiler rejects this. When adding a parameter, place it before any existing
optional parameters.

Order the remaining parameters by what belongs together rather than by when they were added.
Call sites into `src/` APIs pass three-or-more arguments by name — see
[named-arguments.md](named-arguments.md) — so a declaration is free to group related parameters
without churning readability at the call site.

## Numbered list integrity

After editing any numbered list (in any document),
re-read the file and verify that numbering is contiguous with no gaps or duplicates.

## Scripted text edits must preserve line endings and escape backslashes

Prefer the editing tools for prose. When a bulk edit genuinely needs a script, guard against
these silent failure modes:

- **Backslash escapes.** A non-raw string literal turns a Windows path's backslash-`r` into a
  carriage return, corrupting the text mid-line with nothing visible in most viewers. Use raw
  string literals for anything containing a backslash, and grep the written file for the intended
  text rather than trusting the tool's report.
- **Line endings.** `.gitattributes` pins `* text=auto eol=lf`. A script that writes with platform
  defaults rewrites the whole file to CRLF; git normalizes on staging, so `git diff --cached` looks
  clean while the working tree is not. Read and write with an explicit line-feed newline argument.
- **A replace whose anchor is absent does nothing, and says nothing.** `str.replace` and its kin
  return the unchanged string when the pattern is not found, so the script exits 0 and the edit is
  reported as applied though nothing changed. **Assert every anchor before replacing** —
  `assert old in text, "anchor missing"` — so a failed match fails loudly. This compounds when an
  earlier scripted edit already removed the block a later one is anchored to.
- **A slice between two matches can swallow what lies between them.** Deleting "from this heading to
  the next `#### Scenario:`" crosses any `### Requirement:` sitting in the gap. Bound a cut by the
  *next heading of any level you care about*, and re-check the outline afterward with
  `rg -n '^#{2,4} '` rather than trusting the diff stat.

Verify a suspected CRLF flip by counting bytes in binary mode, not by eye or with a `grep` for a
carriage-return pattern — that check has produced false negatives on a pure-LF file.

## A measured fact belongs in one place; every copy is a future lie

When a number or a decision is recorded in several documents, updating one of them silently
falsifies the rest — and the stale copies read exactly like the fresh one, because a plausible
figure looks like a checked figure. This is the single most common defect in this repository's
documentation, and it is never caught by the person making the change.

Prefer citing the source over restating it. Where restating is genuinely clearer, expect the copy
to go stale and give it a mechanical check.

No script checks this. Review it by hand.

### Never write a bare count

A count ("58 occurrences", "22 tools", "the seven handlers") is a measurement of something that
changes, so it is wrong from the moment the thing changes, and nothing about the stale number looks
stale.

**State the method, not the number.** "Every test class that uses `Process`
(`rg -l 'Process' tests/`)" stays true; a bare count of the same set goes stale.

A number is admissible **only** in this form, and both halves are mandatory:

> As of writing this, the count is 22 — re-derive it with `rg -l 'Process' tests/`.

The stamp without the re-derivation command is still a trap: it tells a reader the number is old
without giving them any way to get the current one.

**A measurement from a run you performed is a different thing and should be reported** — a test
total, a search's hit count, a mutation's red count. It is evidence about what you did, not a claim
about current state. Give it its command and its date, so a reader can tell the two apart.

**Prefer a mechanical check over either.** Where a set's size is load-bearing, a test that derives
the set and fails when it changes beats every number in prose — see
[testing-discipline.md](testing-discipline.md).

**When you change a decision, sweep for the words that described the old one.** The mechanical
check covers numbers, not prose. A term renamed in one document while another still uses
the old term is the same defect, and only a reader catches it. A change is finished when the old text is gone.

### A count is also wrong when its unit is wrong

A number can be fresh, correctly derived, and still wrong, because it counts something other than
what the sentence around it claims — a tool's total is a fact about that tool's unit of work, which
is often not the unit the question is about.

**Derive a denominator from the artifact that defines it, not from whatever summary count is
nearest to hand** — the solution file (`ClaudeTeam.slnx`), the registry, the enumeration — over a count a tool
volunteered for its own purposes. State what the denominator counts, in the same sentence as the
number.

## Searching with `rg`: pass `--hidden` to reach dot-directories

Use `rg --hidden --glob '!.git/**'` for repository searches.
Without `--hidden`, `rg` omits paths with a leading dot, including `.claude/` and `.github/`.

**Do not write `rg -rn`.** `-r` takes a value, so `-rn` parses as `-r n` — "replace every match with
the letter n" — and `rg` prints the *replacement* in place of what you searched for, exit code 0, no
warning. Write `-n -r` apart, or just `-n`.

## Enforcement

Behavioral: caught in code review or a fresh-eyes pass.
