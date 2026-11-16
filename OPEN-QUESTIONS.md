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

### Q2 (2026-11-16, Web, DevTerm sign-in): where does desktop client sign-in and refresh live?

DevTerm and the other desktop clients have no sign-in code (nothing under `src/Applications` acquires a token), so refresh-token handling has nothing to attach to. A client needs the OAuth 2.0 public-client flow with Keycloak: authorization code with PKCE on a loopback redirect (or device code for the Tui), a token cache that refreshes the access token before it expires, and a secure store for the refresh token.

**Options**
- A. A new Framework library `OoBDev.Security.OAuthClient` (host-free: PKCE, loopback listener, device code, `ITokenCache` with refresh and `ISecretStore` abstraction; Windows DPAPI store in an Extension). Reusable by every client. (recommended: matches "reusable means Framework" and the Keycloak decision; needs your confirmation because it creates a project.)
- B. Put the flow inside DevTerm only. Quicker, but the next client repeats it.
- C. Leave sign-in to the consumer; document the flow only.

**Default:** C until answered; the BACKLOG item waits.

**Your answer:**

### Q3 (2026-11-16, Web, right audit): where does the query endpoint for the durable right audit trail live?

`EfRightAuditStore.QueryAsync` exists, but nothing exposes it. A right-guarded endpoint (rights such as `audit.read`) needs ASP.NET, and `OoBDev.AspNetCore.EntityFrameworkCore` is host-free (EF Core Relational and hosting abstractions only). The example web API has no EF reference and no database.

**Options**
- A. Add the endpoint to `OoBDev.AspNetCore.Mvc` behind a small `IRightAuditQuery` contract in `OoBDev.AspNetCore.Abstractions`; the EF store implements it. The library stays host-free, the controller is reusable, and any store (not only EF) can serve it. (recommended: follows "reusable means Framework" and keeps the host reference where it already is.)
- B. Add a `MapRightAudit()` minimal-API extension to the EF project and give it a `FrameworkReference` to ASP.NET. One package, but a host-bound library holding data access.
- C. Only document how to expose it in the consumer; the example stays without it.

**Default:** C until answered; the BACKLOG item waits.

**Your answer:**

### Q4 (2026-11-16, Messaging, GDPR erasure): where does the right-guarded erasure entry point live?

`ErasureService.RequestAsync` and `CompactAsync` exist, but nothing lets an authorised person start them. `OoBDev.EventSourcing` has no reference to the rights contracts (`OoBDev.AspNetCore.Abstractions`), and the design says compaction runs behind its own right (for example `erasure.request`, `erasure.compact`).

**Options**
- A. A small `GuardedErasureService` decorator in a new host-free Framework project that references `OoBDev.EventSourcing` and `OoBDev.AspNetCore.Abstractions` (`IApplicationRightChecker`); an MVC controller in `OoBDev.AspNetCore.Mvc` calls it. (recommended: same shape as Q3, no host reference in the event library; needs your yes because it creates a project.)
- B. Add the `OoBDev.AspNetCore.Abstractions` reference to `OoBDev.EventSourcing` and check rights inside `ErasureService`. No new project, but event sourcing then depends on the web contracts.
- C. Leave it to the consumer; document the pattern only.

**Default:** C until answered; the BACKLOG item waits.

**Your answer:**

### Q5 (2026-11-16, Build, vulnerable packages): should NU1901 to NU1904 break the build?

Restore is clean today. Turning the NuGet audit warnings into errors (`WarningsAsErrors` in `src/Directory.Build.props`) makes a new advisory stop every build until the package is bumped.

**Options**
- A. Keep warnings and run `scripts/docs/sbom.ps1` plus `dotnet list package --vulnerable` in the manual CI. (recommended: CI is manual and local-only, so a hard failure would hit whoever builds next, mid-task; the manual pass still catches advisories.)
- B. Errors for High and Critical only (`NuGetAuditLevel` high plus `WarningsAsErrors`), warnings for the rest. Stops work only for serious advisories.
- C. Errors for all four codes. Strictest, most disruptive.

**Default:** A until answered.

**Your answer:**
