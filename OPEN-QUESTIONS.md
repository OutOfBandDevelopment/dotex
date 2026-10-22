# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Open

### Q10 — BinaryDataDecoders
- **Asked:** 2026-10-22 · **Area:** Incoming · **Owner, 2026-10-22:** still pending (waiting for your review)
- **Question:** About 14 migration decisions are pending. When you review it, do you want the decisions as an options-with-recommendation list?
- **Options:**
  - **A. Yes, I prepare the list so you can answer quickly (recommended).** Same format as this file; no source changes.
  - B. No, you review the source directly. Nothing for me to prepare.
- **Default:** A is prepared only when you ask; not worked on.
- **Your answer:**

### Q14 — Azure Monitor and OpenTelemetry testing
- **Asked:** 2026-10-22 · **Area:** `OoBDev.OpenTelemetry` · **Follows:** Q9 ("use otel and aspire for testing")
- **Question:** I read your answer as: no Azure Monitor exporter, and verify OpenTelemetry through the Aspire dashboard container. Is that right, and how deep should the tests go?
- **Options:**
  - **A. Skip the exporter; add Integration tests that emit traces, logs and metrics through `OoBDev.OpenTelemetry` and read them back from the Aspire dashboard API (recommended).** The stack already has the dashboard and its telemetry API, so the real OTLP path is verified.
  - B. Skip the exporter; keep the current tests. Nothing new to maintain.
  - C. Also add the exporter and test it through an Aspire-hosted collector. More surface for something that cannot reach Azure here.
- **Default:** A.
- **Your answer:**

### Q15 — Docs validation as a .NET test
- **Asked:** 2026-10-22 · **Area:** docs · **Follows:** Q13 ("rewrite this as a .net test")
- **Question:** Where should the .NET docs validation live, and how should it render PlantUML?
- **Options:**
  - **A. New project `src/Tools/OoBDev.Docs.Tests`, Integration category, rendering through the `plantuml/plantuml-server` container (recommended).** Matches the rule that anything outside the process runs in Docker; the link and caption checks also run as Unit tests without the container.
  - B. Same project, but link and caption checks as Unit and rendering as DevLocal. Cheaper to run, but rendering is only checked by hand.
  - C. A command-line tool instead of a test. Easier to call from hooks, but not part of `dotnet test`.
- **Default:** A; `validate-docs.py` stays until the test covers the same checks, then it is removed.
- **Your answer:**

### Q16 — Rights middleware: where roles map to rights
- **Asked:** 2026-10-22 · **Area:** HTTP querying and rights · **Follows:** Q6 (A)
- **Question:** Where is the role-to-right mapping defined?
- **Options:**
  - **A. Configuration (`OoBDev:Authorization:Roles:<role>` lists rights), applied by a claims transformation that adds `app__application_right` claims (recommended).** A remap needs no code change, it runs before authorization so every surface gets it, and tests need no database.
  - B. A database table read through a provider. Editable at runtime, but needs a store and caching before first use.
  - C. Attributes in code on the roles. Simple, but a remap needs a rebuild.
- **Default:** A.
- **Your answer:**
