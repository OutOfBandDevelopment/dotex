# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 — 2026-10-22 — Migration — Incoming code recommendation plan

Review [docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md). It compares all incoming code and the harvest candidates with main and proposes what to migrate, defer or reject (with reasons).

**Options:** reply per decision D1 to D9 in section 7 (D2, D8 and D9 come from the [cross-check](docs/migration/incoming-harvest-crosscheck.md)) of the plan, or **A** (recommended): approve all recommendations, which starts the order in section 6; **B**: change individual decisions.

**Default:** nothing migrates; only the D7 backlog entries exist.

**Your answer:**


### Q2 — 2026-10-22 — HTTP API — OData and GraphQL libraries

The `QUERY` verb slice is built ([design](docs/design/HttpQueryVerb/README.md)). OData and GraphQL need a library decision before design continues: both are large surfaces and both would be new third-party dependencies (rejected-dependency rules apply to IoC, logging and Polly only).

**Options:**
- **A (recommended):** OData through `Microsoft.AspNetCore.OData` on the existing `IQueryable<T>` seam, GraphQL later through HotChocolate. First-party-grade OData support and the nested `$expand` the custom syntax lacks; two new packages, each behind the shared rights checker.
- **B:** Extend the custom search syntax with nested filters instead of OData; no new dependency, but a bespoke language to keep.
- **C:** OData only now; decide GraphQL after real client demand.

**Default:** nothing is added; the `QUERY` verb serves the custom syntax.

**Your answer:** A: OData now, GraphQL later (answered in chat 2026-10-22).


### Q3 — 2026-10-22 — HTTP API — Where does Accept-Query go?

[HttpQueryVerb](docs/design/HttpQueryVerb/requirements.md) REQ-006 wants collection responses to advertise `Accept-Query: "application/json"`. The middleware cannot tell a collection endpoint from any other, so the header needs a rule for which responses carry it.

**Options:**
- **A (recommended):** an `[AcceptsQuery]` attribute (result filter) on the search actions; the header is added only where the owner marks it. Explicit, no false claims.
- **B:** the middleware adds it to every successful GET response; one line, but advertises `QUERY` on endpoints that do not honor it.
- **C:** only in the OpenAPI document, no header.

**Default:** nothing is added (option A is what I will build once confirmed).

**Your answer:** A: `[AcceptsQuery]` attribute (answered in chat 2026-10-22).


### Q4 — 2026-10-22 — Code quality — Two old #warning markers

The `#warning` markers are replaced by TODO comments so the build stays warning-free. Their intent is the owner's to settle:

1. `OoBDev.Common.Hosting` `TryAddCommonHosting`: MailKit hosting is registered in DEBUG builds only, with the note "Mailkit hosting is not supported at this time".
2. `RabbitMQQueueMessageSenderProviderTests`: marked "is this needed".

**Options:**
- **A (recommended):** (1) register MailKit hosting in every build, as the other hosts are, with the `DisableMailKit` switch; (2) keep the test, since it is the only RabbitMQ sender round trip.
- **B:** (1) remove the MailKit registration and the `DisableMailKit` switch until it is supported; (2) delete the test.
- **C:** leave both as they are.

**Default:** C (comments only).

**Your answer:** A: register MailKit hosting in every build, keep the RabbitMQ sender test (answered in chat 2026-10-22).
