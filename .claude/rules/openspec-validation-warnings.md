# OpenSpec validation warnings

Added: 2026-11-08.

## Rule

Fix every warning from OpenSpec validation as soon as possible.
Both of these commands must pass with no warnings and no errors:

```bash
openspec validate --all --strict
openspec validate --archived
```

- If a warning is in a file of your current change, fix it in that change.
- If a warning is in any other file, report it to the team lead, who tracks it. Do not ignore it.
- `--archived` checks only that every task of each archived change is complete. OpenSpec 1.16.1 cannot check the content of archived changes again.
- Do not combine `--archived` with `--all`. With `--all`, it replaces the spec and change checks instead of adding to them.

## Why

An OpenSpec CLI update can change the validation rules, so a spec or change that passed before can start to give warnings.
Every warning is noise that can hide a real problem.

## Enforcement

Mechanical when someone runs the two commands; there is no hook.
Run both at the end of each change that touches `openspec/`, and put their output in the report.
