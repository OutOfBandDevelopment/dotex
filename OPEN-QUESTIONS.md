# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open


### Q1 (2026-11-16, Messaging and EF stores): how do the EF stores ship their schema?

The EF stores (messaging inbox/outbox, suppression list, template source, event stores, right audit) create their schema only through the consumer's `EnsureCreated` or own migrations. A migration is provider-specific (SQL Server, PostgreSQL and SQLite differ), so a shipped migration needs a provider choice.

**Options**
- A. Ship generated SQL scripts per provider (SQL Server, PostgreSQL) under each project's `Schema/` folder, produced by a script from the model, plus a test that the script matches the model. No provider package is referenced by the library. (recommended: keeps the library provider-neutral and fits "adapters reference Abstractions only"; the consumer runs the script or copies the table into its own migration.)
- B. Ship one migrations assembly per provider (`...EntityFrameworkCore.SqlServer`). Applied with `Migrate()`, but it adds about five projects per store and ties versions to EF providers.
- C. Keep it as is: document `EnsureCreated` and the table shape only. Cheapest, no upgrade path.

**Default:** C until answered; the item stays in BACKLOG as "waiting for the owner".

**Your answer:**
