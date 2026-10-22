# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Open

### Q6 — HTTP querying and rights middleware
- **Asked:** 2026-10-22 · **Area:** BACKLOG.md (HTTP querying) · **Earlier answer:** "yes" (does not pick an option)
- **Question:** Which of the three do we build, and in what order?
- **Options:**
  - **A. Rights middleware, then `QUERY`/OData with `POST` fallback, then GraphQL (recommended).** The rights check is needed by every surface, so it comes first and everything after reuses it; `QUERY`/OData extends the existing search, and GraphQL is the largest and least certain.
  - B. Rights middleware only; decide the query styles after seeing it in use. Smallest step, but delays the evaluation you wanted.
  - C. `QUERY`/OData first. Visible sooner, but each surface then needs authorization retrofitted.
- **Default:** A; the rights middleware is started first, the rest waits.
- **Your answer:**

### Q9 — Azure Monitor export
- **Asked:** 2026-10-22 · **Area:** `OoBDev.OpenTelemetry`
- **Question:** Add an Azure Monitor exporter?
- **Options:**
  - **A. Skip (recommended).** Azure coverage is emulator-only and no emulator accepts Azure Monitor export, so it could not be tested; OTLP already reaches any collector.
  - B. Add it as an optional package with unit tests only. Works for real Azure users but ships unverified.
- **Default:** A.
- **Your answer:**

### Q10 — BinaryDataDecoders
- **Asked:** 2026-10-22 · **Area:** Incoming
- **Question:** About 14 migration decisions are pending. You said you will review it later; is that still the plan?
- **Options:**
  - **A. Keep waiting for your review (recommended).** The decisions shape the foundation phase, so guessing risks rework on about 50,000 lines.
  - B. I list the decisions as options with recommendations and proceed on my picks. Faster, with a rework risk.
- **Default:** A; not worked on.
- **Your answer:**

### Q12 — Analyzer severities and `#warning` markers
- **Asked:** 2026-10-22 · **Area:** build
- **Question:** Promote more `suggestion` analyzer rules to `warning`, and remove the `#warning` markers (Mailkit hosting, RabbitMQ test)?
- **Options:**
  - **A. Promote the rules that are currently clean and fix or file the `#warning` markers (recommended).** Stops regressions with no cleanup cost now.
  - B. Leave as is. No churn, but new code can drift.
  - C. Promote everything. Strictest, but needs a large cleanup first.
- **Default:** B until answered.
- **Your answer:**

### Q13 — Doc validation without Python
- **Asked:** 2026-10-22 · **Area:** docs
- **Question:** `validate-docs.py` cannot run on this machine, so `docs/design/CgiMiddleware` is unvalidated. How should it be handled?
- **Options:**
  - **A. Install Python here so I can run it (recommended).** Validation then belongs to the normal docs workflow, as the docs rule requires.
  - B. You run it occasionally; I keep the item flagged. No setup, but problems surface late.
  - C. Port the validator to Node, which the hooks already use. Removes the dependency at the cost of a port.
- **Default:** B.
- **Your answer:**
