---
name: reconcile-designs-and-code
description: Compare the feature design documents with the code and fix the drift in both directions. Use when asked to "compare designs to code", "reconcile docs and code", before closing a feature with the complete-work skill, or when a design doc looks stale.
---

# Reconcile designs and code

Designs are feature documents under `docs/design/<Feature>/` (requirements, architecture, api-design, testing-strategy) and the project `README.{Project}.md`. The rule: code is the truth for what exists; designs are the truth for what is wanted.

1. **List the pairs.** One design folder or document against its project(s) in `src/`. Note the commit you compared.
2. **Extract both sides.** Design side: requirement ids, public API, options, configuration keys, tests promised. Code side: public types and members, options classes and defaults, `ServiceCollectionExtensions`, configuration keys, tests (names and categories). Use search, not memory; cite file paths.
3. **Code has a feature the design lacks:** update the design (requirements, api-design, testing-strategy rows, readme) in the same change. Mark the status honestly.
4. **Design has a feature the code lacks:** do not delete it from the design. Add it to `BACKLOG.md` as an item that links the requirement id, and mark the requirement `Planned` in the design.
5. **They disagree** (different default, name or behavior): ask the owner only if the intent is unclear; otherwise follow the owner-decisions and coding-standards rules, fix the side that is wrong, and note the decision in the design.
6. Record the result in a `docs/changes/` document, then run the `complete-work` tracking steps. Run `OoBDev.Docs.Tests` on the changed folders (or say it was not run).
