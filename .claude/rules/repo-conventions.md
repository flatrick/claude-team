# Repo conventions

## C# parameter ordering

Never reorder C# method parameters such that a required parameter follows an optional
one. The compiler rejects this. When you add a parameter, place it before any existing
optional parameters.

Order the other parameters by what belongs together, not by when you added them.
Call sites into `src/` APIs pass three or more arguments by name (see
[named-arguments.md](named-arguments.md)). A declaration can therefore group related parameters
and the call site stays readable.

## Numbered list integrity

After you edit any numbered list (in any document),
re-read the file and verify that the numbering is contiguous, with no gaps or duplicates.

## Scripted text edits must preserve line endings and escape backslashes

Prefer the editing tools for prose. If a bulk edit really needs a script, guard against
these silent failure modes:

- **Backslash escapes.** A non-raw string literal turns the backslash-`r` of a Windows path into a
  carriage return. This corrupts the text in the middle of a line, and most viewers show nothing. Use raw
  string literals for anything that contains a backslash. Grep the written file for the intended
  text. Do not trust the report of the tool.
- **Line endings.** `.gitattributes` pins `* text=auto eol=lf`. A script that writes with the platform
  defaults rewrites the whole file to CRLF. Git normalizes on staging, so `git diff --cached` looks
  clean while the working tree is not. Read and write with an explicit line-feed newline argument.
- **A replace whose anchor is absent does nothing, and says nothing.** `str.replace` and similar
  functions return the unchanged string when they do not find the pattern. The script then exits 0 and reports
  the edit as applied, but nothing changed. **Assert every anchor before you replace** —
  `assert old in text, "anchor missing"` — so that a failed match fails loudly. The risk is higher when an
  earlier scripted edit already removed the block that a later edit uses as anchor.
- **A slice between two matches can swallow what lies between them.** A deletion "from this heading to
  the next `#### Scenario:`" crosses any `### Requirement:` in the gap. Bound a cut by the
  *next heading of any level you care about*. Re-check the outline afterward with
  `rg -n '^#{2,4} '`. Do not trust the diff stat.

To verify a suspected CRLF flip, count bytes in binary mode. Do not check by eye or with a `grep` for a
carriage-return pattern. That check has produced false negatives on a pure-LF file.

## A measured fact belongs in one place; every copy is a future lie

A fact that you record in several documents goes stale in all of them but one. The stale copies read
exactly like the fresh copy, because a plausible figure looks like a checked figure.
[AGENTS.md](../../AGENTS.md) ("Where instructions live") requires one home for each instruction.
The same applies to a measured fact. Prefer a citation of the source to a restatement.
If a restatement is clearer, expect the copy to go stale, and give it a mechanical check.

No script checks this. Review it by hand.

### Never write a bare count

A count ("58 occurrences", "22 tools", "the seven handlers") is a measurement of something that
changes. The count is wrong from the moment the thing changes, and the stale number does not look
stale.

**State the method, not the number.** "Every test class that uses `Process`
(`rg -l 'Process' tests/`)" stays true; a bare count of the same set goes stale.

A number is admissible **only** in this form, and both halves are mandatory:

> As of writing this, the count is 22 — re-derive it with `rg -l 'Process' tests/`.

A stamp without the re-derivation command is still a trap. It tells a reader that the number is old,
but it gives the reader no way to get the current number.

**A measurement from a run that you performed is different. We recommend that you report it.** Examples are a test
total, the hit count of a search, and the red count of a mutation. It is evidence about what you did, not a claim
about the current state. Give it its command and its date, so that a reader can tell the two apart.

**Prefer a mechanical check to either.** If the size of a set is load-bearing, use a test that derives
the set and fails when the set changes. It is better than every number in prose. See
[testing-discipline.md](testing-discipline.md).

**When you change a decision, sweep for the words that described the old decision.** The mechanical
check covers numbers, not prose. If you rename a term in one document and another document still uses
the old term, this is the same defect, and only a reader finds it. A change is finished when the old text is gone.

### A count is also wrong when its unit is wrong

A number can be fresh and correctly derived, and still be wrong. It can count something other than
what the sentence around it claims. The total of a tool is a fact about the unit of work of that tool.
This unit is often not the unit that the question is about.

**Derive a denominator from the artifact that defines it.** Do not use the nearest summary count.
Use the solution file (`ClaudeTeam.slnx`), the registry, or the enumeration, not a count that a tool
gave for its own purposes. State what the denominator counts in the same sentence as the
number.

## Searching with `rg`: pass `--hidden` to reach dot-directories

Use `rg --hidden --glob '!.git/**'` for repository searches.
Without `--hidden`, `rg` omits paths with a leading dot, including `.claude/` and `.github/`.

**Do not write `rg -rn`.** `-r` takes a value, so `-rn` parses as `-r n` — "replace every match with
the letter n". Then `rg` prints the *replacement* in place of what you searched for, with exit code 0 and no
warning. Write `-n -r` apart, or write only `-n`.

## Commit quality

- Follow the root `.gitattributes`. Do not commit changes that break line-ending or diff behavior.
- Follow the committed hooks in `.githooks/`. [worktree-workflow.md](worktree-workflow.md) says how to set `core.hooksPath` once.
- Keep the commit subject at or below the `subjectMax` value in `.girconfig`. The `gir` commit-msg hook rejects a longer subject. Read the value with `git config -f .girconfig --get gir.subjectMax`.

### Fix an earlier commit with a fixup commit

1. Make the fix as a `fixup!` commit for the commit that it corrects: `git commit --fixup=<commit>`.
2. If later commits changed the same lines, aim the fixup at the last commit that touched those lines. Do not aim it at the commit that first wrote them. Find the last commit with `git log -L <start>,<end>:<file>` or `git blame`. A fixup aimed at an older commit conflicts during the autosquash.
3. Before the autosquash, record the tree: `git rev-parse HEAD^{tree}`.
4. Before you push, fold the fixups: `GIT_SEQUENCE_EDITOR=: git rebase -i --autosquash <base>`.
5. After the autosquash, run `git rev-parse HEAD^{tree}` again. If the tree is not the same, stop and report to the team lead.

The history then reads as if each commit was right from the start.

Why: teammates do not see these conventions unless a rule delivers them. A wrong fixup target or a long subject costs a retry.

## Secrets and configuration

- **No secrets in the repository**: Do not store API keys, license keys, or authentication information.
- **Samples only**: Commit sample configuration with placeholder values only. Make sure `.gitignore` excludes real local secret files.

## Enforcement

Behavioral: caught in code review or a fresh-eyes pass.
