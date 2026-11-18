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

### Q5 (2026-11-16, Build, vulnerable packages): should NU1901 to NU1904 break the build?

Restore is clean today. Turning the NuGet audit warnings into errors (`WarningsAsErrors` in `src/Directory.Build.props`) makes a new advisory stop every build until the package is bumped.

**Options**
- A. Keep warnings and run `scripts/docs/sbom.ps1` plus `dotnet list package --vulnerable` in the manual CI. (recommended: CI is manual and local-only, so a hard failure would hit whoever builds next, mid-task; the manual pass still catches advisories.)
- B. Errors for High and Critical only (`NuGetAuditLevel` high plus `WarningsAsErrors`), warnings for the rest. Stops work only for serious advisories.
- C. Errors for all four codes. Strictest, most disruptive.

**Default:** B, they should break the build and be fixed when discovered.

**Your answer:**
