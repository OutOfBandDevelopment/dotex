# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

- convert from .sln to .slnx for the solution
- reuse the plantuml container on this system isntead of creating your own.
- aspire is missing the sql server instance with the vector extensions.  this should be deployed with docker and sql2019

## Open

### Q1 — 2026-10-22 — Migration — Incoming code recommendation plan

Review [docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md). It compares all incoming code and the harvest candidates with main and proposes what to migrate, defer or reject (with reasons).

**Options:** reply per decision D1 to D9 in section 7 (D2, D8 and D9 come from the [cross-check](docs/migration/incoming-harvest-crosscheck.md)) of the plan, or **A** (recommended): approve all recommendations, which starts the order in section 6; **B**: change individual decisions.

**Default:** nothing migrates; only the D7 backlog entries exist.

**Your answer:**

