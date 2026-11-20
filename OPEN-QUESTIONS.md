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

### Q21 · 2026-11-20 · DevTerm · Should the Connection Editor's saved-profile list become the generated `List` control?

The `List` control now renders in WPF, Avalonia, WinForms, Blazor and the Tui (single selection, add and delete as sibling buttons). The WPF Connection Editor's profile list does more: multi-select (delete selected, export selected), double-click to load, and Load, Refresh, export and import buttons beside it ([spec](docs/applications/devterm/specs/connection-editor.md)). Rebuilding it on the control changes behaviour and needs a multi-select design first.

**Options**

- A. (recommended) Keep the profile list hand-built (it is list management with commands, as the spec table already says) and close the backlog item; the `List` control serves single-select master-detail forms elsewhere.
- B. Add a multi-select mode and double-click command to `ListControl` (model, view model, five renderers), then move the editor onto it: one vocabulary everywhere, a larger change touching every renderer.

**Default:** A; the backlog item is closed with the reason recorded.

**Your answer:**

### Q22 · 2026-11-20 · Example app · How is the Data access query runner kept read-only?

The Data access screen (missing-screens.md, "Read-only guard design") would run text queries against the example's databases from a browser, so the guard is a security decision (rule `security-audit`).

**Options**

- A. (recommended) Defence in depth: a dedicated read-only database login (the connection string `OoBDev:DataAccess:ConnectionString` must be a test or reporting instance, as in the Integration rule), `ApplicationIntent=ReadOnly`, a single statement that must parse as SELECT (ScriptDom, already used by `OoBDev.DacFx`), a row cap and a timeout, and the `data.query` right on admin only. The login permissions are the real guard; the parser is the second layer.
- B. Parse check only (SELECT-only via ScriptDom) on the existing connection: least setup, but one parser gap runs a write.
- C. No free-text queries: only a fixed list of named, parameterised reports: safest, much less useful as a demo.

**Default:** A, built only after you confirm; the screen stays unbuilt meanwhile.

**Your answer:**
