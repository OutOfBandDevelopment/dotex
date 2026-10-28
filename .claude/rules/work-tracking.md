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
- Rough edges found while working go to `BACKLOG.md`, not into the current change.
- The root holds only `README.md`, `CLAUDE.md`, `TODO.md`, `BACKLOG.md`, `OPEN-QUESTIONS.md`, `TEST_VARIABLES.md` and `CONFIGURATION_SETTINGS.md`.
