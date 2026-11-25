---
name: feedback-final-state-over-migrations
description: Database schema is managed as a final (desired) state, not as a chain of migrations
metadata:
  type: feedback
---

Owner said (2026-11-24): "I would prefer it to be final states/desired state versus migration based."

**Why:** the owner wants the schema to be one readable final definition that tooling diffs and applies (the desired-state design: sqldef wrapped, drop and type-change gates), not a growing pile of generated migration files that must be kept equal to the model.

**How to apply:** new stores and schema changes edit the final schema (plain `CREATE` scripts, or the script generated from the model) and run through `OoBDev.Data.DesiredState`; do not add `dotnet ef migrations add` migrations. The existing EF migration projects are retired by the plan in `docs/design/DesiredStateDatabase/final-state-plan.md` once the owner confirms it (Q37). Data transformations are not schema and are a separate question. See [[feedback-breaking-changes-ok]] and [[feedback-layering-questions-early]].
