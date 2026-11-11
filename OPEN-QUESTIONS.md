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

