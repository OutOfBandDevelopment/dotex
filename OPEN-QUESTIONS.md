# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q20 · 2026-11-20 · Persistence · Where do the EF migration sets live?

Q1 decided that every relational store ships EF migrations per provider. Event sourcing already does it with one provider project per database (`OoBDev.EventSourcing.Postgres`, `.Sqlite`, a dacpac for SQL Server). Seven more stores have a `DbContext` (messaging queue, suppression list, templates, right audit, complex-event schedule, data loader rows). Layering and project-creation questions go to the owner first (rule `layering-and-migration-plans`).

**Options**

- A. One provider project per store and database (about 12 new projects with the existing event store pattern): fully consistent, each deployment references only what it uses; many tiny projects.
- B. (recommended) One migrations project per database that holds the migration sets of all the stores: `OoBDev.Data.Migrations.Postgres` and `OoBDev.Data.Migrations.Sqlite` (one folder and one `MigrationsHistoryTable` per context), plus one SQL project/dacpac for SQL Server. Two new EF projects instead of twelve, one place to run `dotnet ef`, a deployment picks its database once. The event store keeps its own provider projects (already built).
- C. Migrations inside each store project, one set per provider selected at design time: no new projects, but the store project then references every provider package.

**Default:** B, starting with the right audit store (smallest, newest); nothing is created until you confirm.

**Your answer:**
