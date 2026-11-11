# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open


### Q1 · 2026-11-11 · EventSourcing · Where do the Entity Framework store and the three database projects live?

You asked for Entity Framework with PostgreSQL, SQL Server and SQLite. The layering rule asks for the project homes before work starts. Precedent: `OoBDev.ComplexEvents.EntityFrameworkCore` is in Framework.

**Options**
- A. (recommended) Stores and model in `OoBDev.EventSourcing.EntityFrameworkCore` (Framework, EF Core only), one small project per database (`.Postgres`, `.SqlServer`, `.Sqlite`) in Framework beside it. A deployment references only the database it needs, and each holds its own migrations.
- B. One project with all three database packages. Fewer projects, but every deployment carries all three drivers.
- C. Stores in Framework, database projects in ExternalServices (where the other database providers are). Matches the database-provider convention but splits one feature across layers.

**Default:** A.

**Your answer:**
