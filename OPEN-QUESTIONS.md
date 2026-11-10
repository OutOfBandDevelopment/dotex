# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 - 2026-11-10 - Example app: Identity management screen
The example app lists users and roles on a screen only if the host may call the Keycloak admin API. Should the example API get an admin-scoped service account?

**Options:**
- A. (recommended) Read-only: a service account with `view-users` and `view-realm` only. Shows users, roles and the role-to-right translation without any write path; matches the owner's rule that applications never store passwords.
- B. Full admin: also create and disable users. Useful demo, but gives the example API write power over the identity provider.
- C. Skip the screen; Rights and roles already shows the authorization model.

**Default:** C until answered (no code written).

**Your answer:**

### Q2 - 2026-11-10 - Markdown PlantUML renderer: which server renders diagrams
`OoBDev.System.Text.Markdown` renders every diagram on the public plantuml.com server, so the diagram text (which can describe internal systems) leaves the machine. What should the default be?

**Options:**
- A. (recommended) A `PlantUmlRendererOptions` with `RemoteUrl`, defaulting to the test stack's `plantuml` container URL when configured and failing with a clear message when none is set. No document text leaves the machine unless someone opts in; matches the owner's "no live data" and local-first rules.
- B. Same option, but keep plantuml.com as the default. No behaviour change for existing users, but data still leaves by default.
- C. Leave the hard-coded URL and only document it.

**Default:** A in code is not started; the renderer keeps its current URL until answered (the constructor taking an `IPlantUmlRenderer` already allows a local back end).

**Your answer:**
