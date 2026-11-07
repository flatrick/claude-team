---
paths:
  - "tests/**/*.cs"
---

# Temp directories in tests

## Rule

Test code that needs a real filesystem working directory or file MUST create it with
**`Directory.CreateTempSubdirectory(prefix)`** and build any child paths under its
`.FullName`. Do **not** use `Path.GetTempPath()` or `Path.GetTempFileName()` to mint a
working directory/file, and never hardcode a fixed temp path (`"/tmp/diagnostics.jsonl"`,
`Path.Combine(Path.GetTempPath(), "fixed-name")`).

```csharp
// Good — unique, process-owned, auto-namable, cleaned up in finally
var dir = Directory.CreateTempSubdirectory("my-feature-");
try
{
    var logPath = Path.Combine(dir.FullName, "diagnostics.jsonl");
    // ... exercise code that writes logPath ...
}
finally
{
    dir.Delete(recursive: true);
}

// Bad — fixed shared path: collides between parallel tests and fails under a
// sandbox that restricts writes to a pre-existing /tmp entry
DiagnosticsLogPath = "/tmp/diagnostics.jsonl";

// Bad — GetTempPath()/GetTempFileName() as a working-dir factory
var root = Path.Combine(Path.GetTempPath(), "x-" + Guid.NewGuid());
Directory.CreateDirectory(root);
```

For test code, this rule wins over [artifacts-location-policy.md](artifacts-location-policy.md).
Logs and other non-test run output stay under `.scratch/`.

## Why

A fixed or shared temp path collides when tests run in parallel, and it fails under a
sandbox that restricts writes to a pre-existing `/tmp` entry.
`Directory.CreateTempSubdirectory` makes a fresh, uniquely named directory on each call.

## Enforcement

Behavioral. Code review catches a call to `Path.GetTempPath()` or `Path.GetTempFileName()` that makes a working directory.
A fixed literal such as `"/tmp/x"` is also a review finding.
