# Agent findings ledgers — durable output, read before you hunt

## Rule

Every dispatched agent **SHALL** write its findings to a file on disk **before** it composes its
reply, and **SHALL** read the existing ledger for its subject **before** it starts work.

1. **Write before returning.** An agent's reply lives only in the driver's context. A context
   clear, a compaction, a harness restart, or a killed sub-agent destroys it. A file survives all
   four.
2. **Read before hunting.** Writing alone does not stop rediscovery — the driver still has to
   remember to look. An agent that finds its finding already recorded reports it as a
   **recurrence**, citing the prior entry and its disposition, instead of re-raising it.

### Reading the ledger

List the subject directory and read the entries whose `key` matches your assignment, plus their latest disposition.
Say what you read ("all entries" or "entries matching `<prefix>`").
"Read the ledger" does not tell the next reader what scope you covered.

### The driver writes too, for the same reasons

**A finding the driver produces itself SHALL be written to the same ledger, in the same format, as
one an agent returns.** So SHALL a disposition — every review round gets a `-disposition.md`
entry, not only the first.

The driver's context is the *least* durable thing in the system, and driver-originated findings are
often the freshest, coming straight from adjudicating what agents just returned. Dispatched agents read `.scratch/agent-ledgers/<subject>/` only.
A note in the driver's own context or in a human-facing file does not satisfy this rule.

## Why

A finding that lives only in a reply is lost with the context, and the next agent rediscovers it.

## Layout

```
<checkout-root>/.scratch/agent-ledgers/<subject-slug>/<UTC-timestamp>-<agent>[-<discriminator>].md
```

- **`<checkout-root>` is the checkout the work is happening in, and the dispatcher SHALL state it
  as an absolute path.** A relative `.scratch/` resolves against whatever directory the agent
  inherits, not reliably the one meant. In a worktree, the ledger is the worktree's `.scratch/`,
  never the main checkout's.

  ```
  FINDINGS LEDGER (read before starting, write before replying):
    /abs/path/to/checkout/.scratch/agent-ledgers/<subject-slug>/
  ```

  A brief naming the worktree for the *work* has not named it for the *ledger* — state both. If a
  dispatch arrives without an absolute path, the agent resolves it per **Resolving the path**
  below and says in its reply which directory and command it used.

### Resolving the path

The discriminator is **who reads it next**:

- An agent working on this branch, for itself or a sibling on the same branch, resolves against
  the worktree's own root: `git rev-parse --show-toplevel`.
- The human, or an agent on another branch — reading across branches, or filing a finding meant to
  survive this worktree's removal — resolves against the main checkout:
  `git worktree list --porcelain | head -1` (its first entry is always the main checkout, correct
  from inside any linked worktree).

Never write "the main checkout" without the command that resolves it — the working directory a
session happens to start in is not reliable evidence of which checkout is meant.

**The terminus.** `.scratch/agent-ledgers/<subject>/` is git-ignored run state.
Nothing under `.scratch/` survives a worktree's removal.
Copy the conclusions that matter into the final report before you remove a worktree.

- **Recovering a split ledger**: copy the stray entries in, never move or edit them — the log is
  append-only and a moved file breaks any reply that cited its path. Leave a note entry recording
  the split. Once every entry is verified present at the destination, replace the source entries
  with a single `<timestamp>-relocated.md` naming the authoritative directory — an emptied
  directory with no note invites a second ledger to start there and re-create the split.
- The same applies when a subject's work moves checkout deliberately (a worktree split, a merge).
  A subject's ledger belongs where its work is performed; work that has merged belongs where it
  landed, not with a worktree that no longer exists.
- **Copy the timestamp from a command, never compose it** — the filename is the ordering protocol,
  and a stamp that isn't really UTC silently reorders the log:

  ```bash
  date -u +%Y%m%dT%H%M%SZ
  ```

  ```powershell
  [DateTime]::UtcNow.ToString('yyyyMMddTHHmmssZ')
  ```

  A locally-stamped entry with a `Z` appended sorts hours away from where it belongs and looks
  well-formed; it has recurred twice in this repository, both times misdiagnosed as a host clock
  skew that did not exist. The tell: the log is append-only, so an entry stamped after the current
  `date -u` is mis-stamped by definition — order such a subject by content, not by name, until
  trustworthy.

- **Keyed by subject, not by agent.** "Has this been found before?" is about the subject, and
  cross-agent duplication is exactly what needs deduping.
- **`<subject-slug>` is the change name** — the task name, the bug slug, or the run id the
  work belongs to, and nothing narrower. One change gets one directory for its whole life across
  every agent, dimension, and round. Sub-topics are key prefixes (see Entry format), not sibling
  directories. If the dispatcher omits the slug, derive it from the change, use it, and say which
  you chose.

- **One file per invocation, never a shared append target.** Parallel batches are normal here, and
  concurrent appends to one file are not atomic.
- Add a `<discriminator>` when one agent runs many times in a batch — the claim id, the dimension,
  the slice.
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

`key` is the dedup handle: derive it from *what is wrong and where*, so it matches again next
month (`retry-missing-on-launch`, not `finding-3`). A sub-topic is a key **prefix**
(`launch-retry-missing-on-launch`), never a second subject directory — splitting into a
sibling slug defeats the dedup. A `key:` block may sit deeper than `###` when an entry groups
findings under its own sections; match on the `key:` prefix regardless of heading depth.

## State the method, not just the conclusion

**Every claim SHALL carry the method that produced it**, in the entry and in the reply: (1) what
you ran, verbatim and copy-pasteable; (2) what it returned, quoted; (3) why that method can return
the answer — a text search only answers for text matching the pattern chosen, and proves nothing
about phrasings it didn't try.

**Label the basis** — `read`, `text-search`, or `inferred` — because a reader
cannot recover which one you used from the prose. A code claim cites the file and line range that you read.

### A negative result needs its method more than a positive one

"I found nothing" is unfalsifiable without the pattern, and reads as assurance. State what you
searched, over what set, and what you did not examine. A search-based absence claim keyed to one
phrasing proves nothing about a differently-worded instance of the same thing — this has produced
a false "missing" finding when the pattern matched one file's wording but not its twin's
semantically-identical phrasing. Search the part of the claim that can't be paraphrased away, or
read the file and label the basis `read`.

### A completeness claim states its verification, or states that it has none

An entry asserting a set is exhaustive ("every producer", "no delta is owed") carries a `verified:`
line naming the independent reviewer verdict and its entry, or states plainly that none was run.
Unverified must be a visible state, not an inference — a well-stated method reads identically
whether or not the claim was actually checked.

### Say what you did not do

Name the scope you didn't cover: files skipped, an arm not staged, a tool unavailable. A degraded
query (read → text search) says so and marks affected claims unverified. An unrecorded gap reads as
covered ground.

## Append-only, including dispositions

Never edit or delete the content of a prior entry: its findings, keys, verdicts, and dispositions. **A disposition is a new file, not an edit**:

```
.scratch/agent-ledgers/<subject>/<timestamp>-disposition.md
```

recording, per key: applied / rejected with the reason / tracked, and where it now lives. The
current state of a key is its most recent entry — a finding read, agreed with, and left in no
artifact is indistinguishable from one nobody raised.

```markdown
# <agent-or-driver> · <subject> · <UTC ISO-8601>

## Index
| key | disposition | where it landed |
|-----|-------------|-----------------|
| retry-missing-on-launch | applied | `LaunchPlan` now retries on both panes |
| second-pane-name-ignored | rejected | by design — the name is per session, not per pane |
| tracked-example-key | tracked | issue or file where it now lives |
```

`disposition` is applied / rejected / tracked; anything else is prose, in the sections below the
index. Keep `key` first — a `rejected` with no stated reason is not a disposition.
Fix a format or syntax error in an entry, for example a wrong table header, in place.

**Reuse the reporting entry's key verbatim — copy the `key` column cell directly out of its `## Index` table, never retype it, even to fix wording.** Retyping is exactly where a paraphrase creeps in, and a paraphrased key leaves the original finding open forever.

After you write a disposition entry, check that every key it closes is the reporter's exact key spelling.
List the subject directory and compare the key cells.

### A disposition that claims work not done is worse than an open finding

Write the disposition **after** making the change, for only what you actually made. A disposition
saying `applied` is a claim the next reader trusts and stops checking; a false one makes a live
defect invisible rather than merely unfixed.

- **List sub-items individually**, each with its own outcome, when a finding covers more than one
  thing — a "two-part NIT" dispositioned `applied` after only one part landed has shipped a live
  defect as closed.
- **Before writing `applied`, enumerate the finding's class and record what the command
  returned** — the rule above only catches a finding with *visible* sub-items; it misses a finding
  that looks singular and is actually one member of a set (e.g. "the reserved-key set omits X",
  found in three places, still missing from three more that a class-wide `rg` would have caught).
  State the class as a property, name the enumerating command, paste the result.
- **`suggested disposition:` in a findings entry is not a disposition.** Only a
  `<timestamp>-disposition.md` entry is; a suggestion left un-acted-on reads exactly like one
  nobody touched. Derive the gap mechanically before closing out a subject, rather than trusting
  memory:

  ```bash
  all=$(grep -hoE '^[|] [a-zA-Z0-9][a-zA-Z0-9._-]+ +[|]' "$D"/*.md            | tr -d '| ' | sort -u)
  dis=$(grep -hoE '^[|] [a-zA-Z0-9][a-zA-Z0-9._-]+ +[|]' "$D"/*disposition*.md | tr -d '| ' | sort -u)
  comm -23 <(echo "$all") <(echo "$dis")
  ```

### A caveat attached to a finding is part of the finding

A qualifier on a finding ("two of six are grep-only", "this count is a floor") travels with every
derived artifact or the artifact is wrong — dropping it during transcription silently converts an
N-item finding into a smaller one that looks complete. A caveat is applied, closed, or explicitly
carried forward, never merely dropped.

## Recurrences

When the ledger already holds your finding's key:

- **Dispositioned** → report it as a **recurrence** and honor that disposition. Re-raise only if
  you can show the *reason* no longer holds, and say what changed.
- **Open, undispositioned** → say so rather than restating it; add only genuinely new evidence.
- **Your evidence contradicts the prior entry** → that is a finding in its own right: file a new
  key that `SUPERSEDES` the old one and state what proves the earlier call wrong.

Reporting "this recurred and was already settled" is a success, not an empty result.

## The write boundary for read-only agents

Most reviewing and auditing agents are read-only; this rule is the single, narrowly-stated
exception:

> Never modify tracked repository content — no source, tests, specs, documents, or change
> artifacts. Write only inside `.scratch/agent-ledgers/<subject-slug>/`: your own findings
> entry, plus a `<timestamp>-disposition.md` entry when your dispatcher asks you to record
> dispositions. Any write outside that directory is a violation.

## What the dispatcher owes the agent

A command or driver dispatching an agent **SHALL** state, in the brief:

1. **The subject slug** — the task name, fixed for the task's whole life, not the dimension or
   round. Where scoped to a sub-topic, name the **key prefix** to use inside that one subject.
2. **The absolute ledger directory**, per Layout above.

Both are one line; omitting either fails silently. Example:

```
FINDINGS LEDGER (read before starting, write before replying):
  /abs/path/to/checkout/.scratch/agent-ledgers/peer-rule-test/
  Key prefix for your findings: launch-
  Read only the entries whose key matches your assignment.
```

## Enforcement

Behavioral. No test can tell a skipped ledger from a run that genuinely found nothing — an agent
that finds nothing still writes its entry, recording what it examined. Verified at review time and
by the next agent on the same subject, the first party to notice a missing predecessor.

Two failure modes need a direct check, since nothing else surfaces them: a driver that reads the
ledger but never writes to it (tell: a subject whose newest entry is always an agent's, never a
disposition, on a run where findings are plainly being adjudicated); and a split ledger (tell: a
subject directory with a gap in its rounds, or an agent reporting "first entry for this subject" on
one that's been running for rounds — check the sibling checkout before believing either).
