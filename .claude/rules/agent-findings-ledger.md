# Agent findings ledgers — durable output, read before you hunt

## Rule

Every dispatched agent **SHALL** write its findings to a file on disk **before** it composes its
reply, and **SHALL** read the existing ledger for its subject **before** it starts work.

1. **Write before returning.** An agent's reply lives only in the driver's context. A context
   clear, a compaction, a harness restart, or a killed sub-agent destroys it. A file survives all
   four.
2. **Read before hunting.** Writing alone does not stop rediscovery, because the driver must
   remember to look. If a finding is already recorded, report it as a
   **recurrence**. Cite the prior entry and its disposition. Do not raise it again.

### Reading the ledger

List the subject directory and read the entries whose `key` matches your assignment, plus their latest disposition.
Say what you read ("all entries" or "entries matching `<prefix>`").
"Read the ledger" does not tell the next reader what scope you covered.

### The driver writes too, for the same reasons

**A finding the driver produces itself SHALL be written to the same ledger, in the same format, as
one an agent returns.** So SHALL a disposition — every review round gets a `-disposition.md`
entry, not only the first.

The driver's context is the *least* durable thing in the system. Findings that the driver produces are
often the freshest, because they come from the driver's judgment of what agents just returned. Dispatched agents read `.scratch/agent-ledgers/<subject>/` only.
A note in the driver's own context or in a human-facing file does not satisfy this rule.

## Why

A finding that lives only in a reply is lost with the context, and the next agent rediscovers it.

## Layout

```
<checkout-root>/.scratch/agent-ledgers/<subject-slug>/<UTC-timestamp>-<agent>[-<discriminator>].md
```

- **`<checkout-root>` is the checkout the work is happening in, and the dispatcher SHALL state it
  as an absolute path.** A relative `.scratch/` resolves against the directory that the agent
  inherits, and this is not always the directory that you mean. In a worktree, the ledger is the worktree's `.scratch/`,
  never the main checkout's.

  ```
  FINDINGS LEDGER (read before starting, write before replying):
    /abs/path/to/checkout/.scratch/agent-ledgers/<subject-slug>/
  ```

  A brief that names the worktree for the *work* does not name it for the *ledger*. State both. If a
  dispatch has no absolute path, resolve it as **Resolving the path**
  below says. State in your reply which directory and command you used.

### Resolving the path

The discriminator is **who reads it next**:

- An agent on this branch, for itself or a sibling on the same branch, resolves against
  the worktree's own root: `git rev-parse --show-toplevel`.
- The human, or an agent on another branch, resolves against the main checkout. This applies when the reader reads across branches,
  or when a finding must survive the removal of this worktree:
  `git worktree list --porcelain | head -1` (its first entry is always the main checkout, correct
  from inside any linked worktree).

Never write "the main checkout" without the command that resolves it. The working directory where a
session starts is not reliable evidence of which checkout you mean.

**The terminus.** `.scratch/agent-ledgers/<subject>/` is git-ignored run state.
Nothing under `.scratch/` survives a worktree's removal.
Copy the conclusions that matter into the final report before you remove a worktree.

- **Recovering a split ledger**: copy the stray entries in. Never move or edit them. The log is
  append-only, and a moved file breaks any reply that cited its path. Write a note entry that records
  the split. Only after you verify that every entry is at the destination, replace the source entries
  with a single `<timestamp>-relocated.md` that names the authoritative directory. An emptied
  directory with no note invites a second ledger to start there and re-create the split.
- The same applies when the work of a subject moves checkout on purpose (a worktree split, a merge).
  The ledger of a subject belongs where its work is performed. Work that has merged belongs where it
  landed, not with a worktree that no longer exists.
- **Copy the timestamp from a command. Never compose it.** The filename is the ordering protocol,
  and a stamp that is not really UTC reorders the log without a warning:

  ```bash
  date -u +%Y%m%dT%H%M%SZ
  ```

  ```powershell
  [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')
  ```

  A local time with a `Z` appended sorts hours away from where it belongs and looks
  well-formed. This has happened twice in this repository. Both times, people wrongly blamed a host clock
  skew that did not exist. The sign: the log is append-only, so an entry that has a stamp after the current
  `date -u` is mis-stamped by definition. Order such a subject by content, not by name, until
  the stamps are trustworthy.

- **Key by subject, not by agent.** The question "Has this been found before?" is about the subject.
  Duplication across agents is the thing that you must remove.
- **`<subject-slug>` is the change name**: the task name, the bug slug, or the run id that
  the work belongs to, and nothing narrower. One change gets one directory for its whole life across
  every agent, dimension, and round. Sub-topics are key prefixes (see Entry format), not sibling
  directories. If the dispatcher omits the slug, derive it from the change, use it, and state which
  slug you chose.

- **Write one file for each invocation. Never use a shared append target.** Parallel batches are normal here, and
  concurrent appends to one file are not atomic.
- Add a `<discriminator>` when one agent runs many times in a batch: the claim id, the dimension,
  or the slice.
- `.scratch/` is git-ignored local scratch by
  [artifacts-location-policy.md](artifacts-location-policy.md): run state, not repository content.
  The committed artifact is the human-written conclusion, never the raw ledger.

## Entry format

Start every file with a compact index, then the detail.

```markdown
# <agent> · <subject> · <UTC ISO-8601>

## Index
| key | severity | one-line |
|-----|----------|----------|
| retry-missing-on-launch | IMPORTANT | launch skips the retry on the second pane |

## Findings

### key: retry-missing-on-launch
severity: IMPORTANT
status: NEW | RECURRENCE of <prior file> | SUPERSEDES <prior key>
finding: <one sentence>
method: <what you ran, verbatim and copy-pasteable, and why it can return the answer>
result: <its raw output, or the specific excerpt the finding rests on>
basis: read | text-search | inferred
verified: <independent reviewer verdict + its entry, when this asserts a set is complete; otherwise `n/a`>
suggested disposition: <hint only — the driver decides>
```

`key` is the dedup handle. Derive it from *what is wrong and where*, so that it matches again next
month (`retry-missing-on-launch`, not `finding-3`). A sub-topic is a key **prefix**
(`launch-retry-missing-on-launch`), never a second subject directory. A
sibling slug defeats the dedup. A `key:` block can sit deeper than `###` when an entry groups
findings under its own sections. Match on the `key:` prefix at any heading depth.

## State the method, not just the conclusion

**Every claim SHALL carry the method that produced it**, in the entry and in the reply: (1) what
you ran, verbatim and copy-pasteable; (2) what it returned, quoted; (3) why that method can return
the answer. A text search answers only for text that matches the chosen pattern. It proves nothing
about phrasings that you did not try.

**Label the basis** (`read`, `text-search`, or `inferred`), because a reader
cannot find out from the prose which basis you used. A code claim cites the file and line range that you read.

### A negative result needs its method more than a positive one

"I found nothing" cannot be falsified without the pattern, and it reads as assurance. State what you
searched, over what set, and what you did not examine. A search-based absence claim that uses one
phrasing proves nothing about an instance with different wording. This has produced
a false "missing" finding: the pattern matched the wording of one file but not the
semantically identical wording of its twin. Search the part of the claim that no paraphrase can change, or
read the file and label the basis `read`.

### A completeness claim states its verification, or states that it has none

An entry that asserts a set is exhaustive ("every producer", "no delta is owed") carries a `verified:`
line. The line names the verdict of an independent reviewer and its entry, or states plainly that none was run.
Unverified must be a visible state, not an inference. A well-stated method reads the same
whether or not someone checked the claim.

### Say what you did not do

Name the scope that you did not cover: files that you skipped, an arm that you did not stage, a tool that was unavailable. A degraded
query (read to text search) says so and marks the affected claims unverified. An unrecorded gap reads as
covered ground.

## Append-only, including dispositions

Never edit or delete the content of a prior entry: its findings, keys, verdicts, and dispositions. **A disposition is a new file, not an edit**:

```
.scratch/agent-ledgers/<subject>/<timestamp>-disposition.md
```

The entry records, for each key: applied / rejected with the reason / tracked, and where it now lives. The
current state of a key is its most recent entry. A finding that someone read, agreed with, and left in no
artifact looks the same as a finding that nobody raised.

```markdown
# <agent-or-driver> · <subject> · <UTC ISO-8601>

## Index
| key | disposition | where it landed |
|-----|-------------|-----------------|
| retry-missing-on-launch | applied | `LaunchPlan` now retries on both panes |
| second-pane-name-ignored | rejected | by design — the name is per session, not per pane |
| tracked-example-key | tracked | issue or file where it now lives |
```

`disposition` is applied / rejected / tracked. Put anything else as prose in the sections below the
index. Keep `key` first. A `rejected` with no stated reason is not a disposition.
Fix a format or syntax error in an entry, for example a wrong table header, in place.

**Reuse the key of the reporting entry verbatim.** Copy the `key` column cell directly out of its `## Index` table. Never retype it, even to fix the wording. A paraphrase enters when you retype, and a paraphrased key leaves the original finding open forever.

After you write a disposition entry, check that every key it closes is the reporter's exact key spelling.
List the subject directory and compare the key cells.

### A disposition that claims work not done is worse than an open finding

Write the disposition **after** you make the change, and only for what you made. The next reader trusts a disposition
that says `applied` and stops checking. A false one hides a live
defect, which is worse than an unfixed defect.

- **List sub-items individually**, each with its own outcome, when a finding covers more than one
  thing. A "two-part NIT" with the disposition `applied` after only one part landed has shipped a live
  defect as closed.
- **Before you write `applied`, enumerate the class of the finding and record what the command
  returned.** The rule above catches only a finding with *visible* sub-items. It misses a finding
  that looks singular and is one member of a set. Example: "the reserved-key set omits X"
  was found in three places, and X was still missing from three more places that a class-wide `rg` would have caught.
  State the class as a property, name the enumerating command, and paste the result.
- **`suggested disposition:` in a findings entry is not a disposition.** Only a
  `<timestamp>-disposition.md` entry is a disposition. A suggestion that nobody acts on looks the same as one
  that nobody touched. Before you close a subject, derive the gap with a command. Do not trust
  memory:

  ```bash
  all=$(grep -hoE '^[|] [a-zA-Z0-9][a-zA-Z0-9._-]+ +[|]' "$D"/*.md            | tr -d '| ' | sort -u)
  dis=$(grep -hoE '^[|] [a-zA-Z0-9][a-zA-Z0-9._-]+ +[|]' "$D"/*disposition*.md | tr -d '| ' | sort -u)
  comm -23 <(echo "$all") <(echo "$dis")
  ```

### A caveat attached to a finding is part of the finding

A qualifier on a finding ("two of six are grep-only", "this count is a floor") goes with every
derived artifact, or the artifact is wrong. If you drop it during transcription, an
N-item finding becomes a smaller finding that looks complete. Apply a caveat, close it, or explicitly
carry it forward. Never drop it.

## Recurrences

When the ledger already holds your finding's key:

- **Dispositioned**: report it as a **recurrence** and honor that disposition. Raise it again only if
  you can show that the *reason* no longer holds. State what changed.
- **Open, undispositioned**: say so. Do not restate it. Add only new evidence.
- **Your evidence contradicts the prior entry**: this is a finding in its own right. File a new
  key that `SUPERSEDES` the old one, and state what proves that the earlier call was wrong.

A report that "this recurred and was already settled" is a success, not an empty result.

## The write boundary for read-only agents

Most reviewing and auditing agents are read-only. This rule is the single, narrow
exception:

> Never modify tracked repository content — no source, tests, specs, documents, or change
> artifacts. Write only inside `.scratch/agent-ledgers/<subject-slug>/`: your own findings
> entry, plus a `<timestamp>-disposition.md` entry when your dispatcher asks you to record
> dispositions. Any write outside that directory is a violation.

## What the dispatcher owes the agent

A command or driver dispatching an agent **SHALL** state, in the brief:

1. **The subject slug**: the task name. It stays fixed for the whole life of the task. It is not the dimension or
   round. For a sub-topic, name the **key prefix** to use inside that one subject.
2. **The absolute ledger directory**, as Layout above says.

Each item is one line. If you omit either item, the failure gives no signal. Example:

```
FINDINGS LEDGER (read before starting, write before replying):
  /abs/path/to/checkout/.scratch/agent-ledgers/peer-rule-test/
  Key prefix for your findings: launch-
  Read only the entries whose key matches your assignment.
```

## Verify a finding before you act on it

A **finding** is a review comment, a claimed bug, a "the SDK does X" assertion, or a hypothesis about
why something behaves in a certain way.
A finding is a *claim to check*, never a directive to obey.
Verify it against **ground truth** before you act on it, and before you commit any code that acts on it.

- **Use ground truth, not authority or confidence.** Check the claim against the thing that decides it:
  the source, the SDK, a runtime observation, or a focused test.
  A confident finding can be wrong, and a tentative finding can be right.
- **Resolve the finding. Do not comply by reflex.** Apply a finding only after verification confirms it.
  If verification refutes it, reject it and state the reason. Record both outcomes.
- **Exploration is cheap. Large rewrites are not.** Validate a premise with throwaway code before
  you commit to a pattern or rewrite a large section.
  Do not commit the throwaway code as the solution.
  Keep the conclusion reproducible, for example a recorded command and its output.

## Enforcement

Behavioral. No test can tell a skipped ledger from a run that found nothing. An agent
that finds nothing still writes its entry and records what it examined. The review and
the next agent on the same subject verify this. The next agent is the first to notice a missing predecessor.

Two failure modes need a direct check, because nothing else shows them. The first is a driver that reads the
ledger but never writes to it. The sign: the newest entry of a subject is always an agent's, never a
disposition, on a run where someone plainly judges findings. The second is a split ledger. The sign: a
subject directory with a gap in its rounds, or an agent that reports "first entry for this subject" on
a subject that has run for many rounds. Check the sibling checkout before you believe either.
