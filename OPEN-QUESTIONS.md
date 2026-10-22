# Open questions

Questions Claude has for the owner. Write your answer under **Your answer** (or answer in chat, which counts the same; Claude copies it here). Claude keeps working on the **Default** until you answer, then moves answered items to [Answered](#answered).

How this file is kept: see `.claude/rules/open-questions.md`.

## Open

### Q1 — CGI: concurrency limits
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** A single global cap (2 x CPU) is in place. Do you also want per-script or per-client limits?
- **Default:** global cap only.
- **Your answer:**

### Q2 — CGI: NPH scripts
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** Non-parsed-header scripts are rejected. Acceptable?
- **Default:** reject.
- **Your answer:**

### Q3 — CGI: run as another user
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** Leave this to the deployment's service account, or build it in?
- **Default:** leave to the deployer.
- **Your answer:**

### Q4 — CGI: Linux fixtures
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi.Tests`
- **Question:** Tests only have Windows `.cmd` fixtures (inconclusive elsewhere). Add `.sh` fixtures and require both platforms, or stay Windows-only for now?
- **Default:** add `.sh` fixtures when time allows; do not block on it.
- **Your answer:**

### Q5 — CGI: authorization
- **Asked:** 2026-10-22 · **Area:** `OoBDev.AspNetCore.Cgi`
- **Question:** Authorization is the host's job, ahead of `UseCgi`. Do you want a built-in `[ApplicationRight]`-style check instead?
- **Default:** no built-in check.
- **Your answer:**

### Q6 — HTTP querying and rights middleware
- **Asked:** 2026-10-22 · **Area:** TODO.md backlog
- **Question:** OData over `QUERY`, GraphQL, RBAC-to-rights translation: which direction should be built?
- **Default:** not started; skipped.
- **Your answer:**

### Q7 — Options validation default
- **Asked:** 2026-10-22 · **Area:** `AddValidatedOptions<T>()`
- **Question:** Strict (fail at startup) or relaxed (fail on first use) as the default?
- **Default:** strict, as coded today.
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

### Q10 — BinaryDataDecoders and ContractParser
- **Asked:** 2026-10-22 · **Area:** Incoming
- **Question:** Decisions pending (BinaryDataDecoders ~14 items; ContractParser implement now, later or keep as spec).
- **Default:** not worked on.
- **Your answer:**

### Q11 — Push and merge
- **Asked:** 2026-10-22 · **Area:** git
- **Question:** `dev/claude-config` is local only. When should it be pushed to `origin`, and merged to `main` or stacked on `dev/patterns-discovery`?
- **Default:** no push, no merge.
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

## Answered

None yet.
