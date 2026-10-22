# Open questions

Questions Claude has for the owner. Write your answer under **Your answer** (or answer in chat, which counts the same). Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Open

### Q6 — HTTP querying and rights middleware
- **Asked:** 2026-10-22 · **Area:** BACKLOG.md (HTTP querying)
- **Question:** OData over `QUERY`, GraphQL, RBAC-to-rights translation: which direction should be built? Your answer so far was "yes", which does not pick a direction.
- **Default:** build all three in order: the rights middleware first (roles to `app__application_right` claims, same check on every API surface), then `QUERY`/OData with `POST` fallback, then GraphQL; none breaks the custom search syntax. Not started until confirmed.
- **Your answer:**

### Q8 — `.github/workflows/`
- **Asked:** 2026-10-22 · **Area:** CI
- **Question:** Should the workflows eventually be replaced by the local runner in `scripts/ci`?
- **Default:** left untouched.
- **Your answer:**

### Q9 — Azure Monitor export
- **Asked:** 2026-10-22 · **Area:** `OoBDev.OpenTelemetry`
- **Question:** Still wanted, given Azure coverage is emulator-only?
- **Default:** skipped.
- **Your answer:**

### Q10 — BinaryDataDecoders
- **Asked:** 2026-10-22 · **Area:** Incoming
- **Question:** Decisions pending (about 14 items). You said you will review it later.
- **Default:** not worked on.
- **Your answer:**

### Q12 — Analyzer severities and `#warning` markers
- **Asked:** 2026-10-22 · **Area:** build
- **Question:** Promote more `suggestion` rules to `warning`, and remove the `#warning` markers (Mailkit hosting, RabbitMQ test)?
- **Default:** leave as is.
- **Your answer:**

### Q13 — Doc validation without Python
- **Asked:** 2026-10-22 · **Area:** docs
- **Question:** `validate-docs.py` cannot run here, so new design docs (`docs/design/CgiMiddleware`) are unvalidated. Will you run it, or should this stay flagged?
- **Default:** stays flagged in `TODO.md`.
- **Your answer:**
