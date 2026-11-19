# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 (2026-11-18, Security, OAuth client): confirm the OAuthClient project layout

You chose a Framework library `OoBDev.Security.OAuthClient` with a Windows DPAPI leaf (Q2, 2026-11-17). The rule `layering-and-migration-plans` needs the table in [the migration plan](docs/design/OAuthClient/migration-plan.md) confirmed before any project is created: abstractions in Common, the library in Framework, `OoBDev.Security.OAuthClient.Windows` in Extensions, and tests beside each.

**Options**
- A. Confirm as written (recommended: it follows your Q2 answer and the Abstractions-only rule; the work then builds the first slice from [the design](docs/design/OAuthClient/README.md)).
- B. One Framework project without the Common abstractions split. Fewer projects, but adapters and clients then reference the implementation.
- C. Change the layout (say what).

**Default:** A is assumed for the design only; no project is created until you answer.

**Your answer:**

### Q2 (2026-11-19, Example app, rights audit screen): should the example host store the right audit trail?

The audit screen in [the missing screens](docs/design/example-ui/missing-screens.md) needs `MapRightAuditQuery` and a store. The example host does not reference `OoBDev.AspNetCore.EntityFrameworkCore`, does not call `AddRightAuditLog()` and runs mostly anonymous, so a screen would show an empty trail unless the host gains a database provider and a rights-bearing sign-in. That is a layering and persistence choice (a host reference to an EF provider), so it is yours.

**Options**
- A. Config-gated: the host references the EF project and maps the endpoint only when `OoBDev:RightAudit:ConnectionString` (SQL Server) is set; the screen says "not configured" otherwise (recommended: reversible, no new startup dependency, matches how the SQL vector store is gated).
- B. In-memory `IRightAuditQuery` for the example only (no database; the trail is lost on restart, and the screen demonstrates the shape, not the durable store).
- C. Leave the screen out of the example app and keep the endpoint documented only.

**Default:** none built until you answer; the item stays in BACKLOG section 5.

**Your answer:**

### Q3 (2026-11-19, Event sourcing, erasure entry point): where does the right-guarded erasure entry point live?

You decided (Q4, 2026-11-17) that the entry point is an interface in a security core library with default implementations in the related leaf libraries. No library named as a security core exists today (`ls src/Framework | grep -i security` finds only the crypto and identity libraries), and the layering rule needs the project table confirmed before a project is created.

**Options**
- A. Add the interface `IErasureEntryPoint` (request an erasure, read its status) to `OoBDev.EventSourcing.Abstractions`, with the default implementation in `OoBDev.EventSourcing`, and a controller in an ASP.NET adapter guarded by `[ApplicationRight("erasure.request")]` (recommended: no new project, the right check stays at the HTTP edge like every other surface, reversible).
- B. Create a new Common library `OoBDev.Security.Abstractions` for such entry-point interfaces now. Matches your wording most literally; adds a project with one interface.
- C. Something else (say what).

**Default:** A is assumed for the design only; nothing is built until you answer. The hash-chain re-seal stays open until chaining exists (requirement R9).

**Your answer:**
