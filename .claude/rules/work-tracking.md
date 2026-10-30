# Work tracking (always apply)

One place per kind of information; move work between them as it progresses.

| File | Holds |
|------|-------|
| `OPEN-QUESTIONS.md` | Only questions still waiting for the owner (rule `open-questions`). |
| `BACKLOG.md` | The full list of open, valid work, grouped by area. |
| `TODO.md` | Active work only: what is in progress and the few items next. Keep it to about 15 lines of items. |
| `docs/changes/` | Finished and verified work, one file per feature (`{epic}-{feature}-{YYYY-MM-DD}.md`), indexed in `docs/changes/README.md`. |
| Feature docs (`docs/design/<Feature>/`, project `README.{Project}.md`, `docs/patterns-discovery/`) | The current truth about the feature; update them in the same change as the code. |

Lifecycle:
0. Feature requests from others arrive as files in `docs/features/request/` (template and process in its `README.md`). Triage each file with `Status: new`: merge it into `docs/design/<Feature>/` and `BACKLOG.md`, then set `triaged` (or `declined` with the reason) and link both; set `done` with the change document when it ships.
1. New work or a new finding goes to `BACKLOG.md`. A question for the owner goes to `OPEN-QUESTIONS.md` with a default.
2. Starting an item moves it from `BACKLOG.md` to `TODO.md`.
3. Finishing an item (build and tests verified, outcome stated plainly) triggers the `complete-work` skill: change document, index entry, feature docs, removal from `TODO.md` and `BACKLOG.md`.
4. An answered question leaves `OPEN-QUESTIONS.md`: its answer is written into the matching feature doc, `BACKLOG.md` or `TODO.md`, and (when work finished) a change document. The questions file keeps open questions only; history is in `git log`.

Rules:
- Do the tracking in the same commit series as the work, not afterwards. A commit that changes `src/` and finishes an item includes the tracking files; a hook reminds you at commit time.
- Never mark something done that was not built and tested; say what was not run.
- Generated user guides and their screenshots (`docs/applications/devterm/user-guide/`) are committed in the same series as the protocol or application change that updates them (owner, 2026-10-30), by explicit paths.
- Application documentation lives in `docs/applications/{app-name}/` as living documents (requirements, architecture, API design, testing strategy), updated with the code; no decision records (owner, 2026-10-30). A new user-facing screen or flow needs a screen spec there and a user guide entry with real captured screenshots, never hand-typed mockups. Reusable capability designs stay in `docs/design/<Feature>/`, protocol references in `docs/protocols/`. A migration keeps a transient map (`docs/migration/`) that is deleted once the move is complete.
- Guides are generated, not written by hand (owner, 2026-10-30): user guides and screenshots for every front end (WPF, Tui, Web) come from test runs of the user journeys, API guides from the OpenAPI, AsyncAPI and ksy files by deterministic MSTest scripts (npm test for JavaScript projects). Output goes next to the application docs. Design: `docs/design/GeneratedGuides/`.
- Rough edges found while working go to `BACKLOG.md`, not into the current change.
## Keeping the files correct

- `TODO.md`: at most about 15 items, one line each (about 400 characters). Detail goes in the feature doc or the change document, history in `git log`. Every item is something started or next, never a log of what was done.
- `BACKLOG.md`: grouped by area (for example DevTerm, Devices, Test infrastructure, Migrations), not by when it was found. Each item lives in exactly one place; before adding one, search for an overlapping item and merge it. A decision the owner already made is recorded as one line in the matching feature doc, not as a backlog item. Keep "waiting for the owner" apart from "decided, waiting its turn".
- When you start, finish, add or drop an item, set the `Last Updated` date (today) of the file you touched in the same edit. Delete an item the moment it is done or superseded; a done item that is still listed is a defect.
- The "Current Work Context" priorities in `CLAUDE.md` must name what `TODO.md` In progress is working on. When the focus changes, change both in the same commit.
- A `SessionStart` hook (`.claude/hooks/tracking-hygiene.mjs`) reports an over-long `TODO.md`, run-on items and stale dates; fix what it reports before new work.

- The root holds only `README.md`, `CLAUDE.md`, `TODO.md`, `BACKLOG.md`, `OPEN-QUESTIONS.md`, `TEST_VARIABLES.md` and `CONFIGURATION_SETTINGS.md`.
