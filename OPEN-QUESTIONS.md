# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

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

**Your answer:** it should be abstracted so it can be easily sweapped out later but a default implmentation based on EF should be included in the framework. 

### Q4 (2026-11-16, Messaging, GDPR erasure): where does the right-guarded erasure entry point live?

`ErasureService.RequestAsync` and `CompactAsync` exist, but nothing lets an authorised person start them. `OoBDev.EventSourcing` has no reference to the rights contracts (`OoBDev.AspNetCore.Abstractions`), and the design says compaction runs behind its own right (for example `erasure.request`, `erasure.compact`).

**Options**
- A. A small `GuardedErasureService` decorator in a new host-free Framework project that references `OoBDev.EventSourcing` and `OoBDev.AspNetCore.Abstractions` (`IApplicationRightChecker`); an MVC controller in `OoBDev.AspNetCore.Mvc` calls it. (recommended: same shape as Q3, no host reference in the event library; needs your yes because it creates a project.)
- B. Add the `OoBDev.AspNetCore.Abstractions` reference to `OoBDev.EventSourcing` and check rights inside `ErasureService`. No new project, but event sourcing then depends on the web contracts.
- C. Leave it to the consumer; document the pattern only.

**Default:** C until answered; the BACKLOG item waits.

**Your answer:**  it should be abstracted with an interface in a security core library and then have default implmnetantions in the related leaf libraries.  (this is how all these sorts of features should be implmented)

### Q5 (2026-11-16, Build, vulnerable packages): should NU1901 to NU1904 break the build?

Restore is clean today. Turning the NuGet audit warnings into errors (`WarningsAsErrors` in `src/Directory.Build.props`) makes a new advisory stop every build until the package is bumped.

**Options**
- A. Keep warnings and run `scripts/docs/sbom.ps1` plus `dotnet list package --vulnerable` in the manual CI. (recommended: CI is manual and local-only, so a hard failure would hit whoever builds next, mid-task; the manual pass still catches advisories.)
- B. Errors for High and Critical only (`NuGetAuditLevel` high plus `WarningsAsErrors`), warnings for the rest. Stops work only for serious advisories.
- C. Errors for all four codes. Strictest, most disruptive.

**Default:** B, they should break the build and be fixed when discovered.

**Your answer:**

### Q6 (2026-11-16, DevTerm WPF, Connection Editor): replace the hand-written editor XAML with the generic form?

`ConnectionEditorViewModel.FormDefinition` is a subset of the hand-written editor: it has no scan buttons (network, Bluetooth, characteristics), no BLE uuid fields, no saved-profile list, import/export or save-name rows. Rendering it with `FormView` would drop those unless the form model grows buttons and the editor keeps the profile panel beside it. The Tui already uses the generated form.

**Options**
- A. Keep the hand-written XAML; use `FormView` only where the form is complete (manifest editor, panels). (recommended: nothing is lost, and the spec's field list stays tested by the XAML accessibility checks.)
- B. Extend the form model with action controls and the missing fields, then render the connection part with `FormView` and keep the profile panel as XAML. Removes duplicated field lists, but is a Framework change plus a screenshot pass.
- C. Replace the whole window with the generated form and move profiles into a separate window.

**Default:** A; the BACKLOG item is parked on this question.

**Your answer:**  is possible.  something like a property grid editor or whatever,
