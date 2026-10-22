# TODO consolidation (2026-10-22)

Eleven `TODO*.md` files were reviewed against the current code and docs and merged into one backlog, [TODO.md](../../TODO.md). The removed files stay in git history.

## Removed files

**Table 1 — Removed TODO files and why**

| File | Reason |
|------|--------|
| `TODO-migrations-index.md`, `TODO-migrations.md` | Index pages that only pointed at the BinaryDataDecoders file; now one entry in `TODO.md` section 4 |
| `TODO-migrations-binarydatadecoders.md` | Phases and decisions kept in `TODO.md`; decision detail is in `docs/migration/binarydatadecoders-decision-sheet.md` |
| `TODO-decisions.md` | BotChat decided (extract patterns, 2026-10-09); BinaryDataDecoders list duplicated the decision sheet |
| `TODO-testing-local-integration.md` | Docker docs were written under `docs/architecture/testing/integration/` (with a different layout than the old plan); the Week 3 GitHub run is obsolete because GitHub is no longer primary |
| `TODO-testing-live-integration.md` | Groq tests, template, readme and docs done 2026-10-09; Azure B2C and Application Insights stacks no longer exist; cost and credential text is covered by `docs/architecture/testing/live-integration/` |
| `Features/Caching/TODO-migrations-caching.md` | Complete and archived 2026-01-20 ([details](migration-caching-framework-2026-01-20.md)) |
| `src/Framework/OoBDev.DacFx.Tests/TODO.md` (empty) | Moved to `TODO.md` section 1 |
| `src/Framework/OoBDev.System.Abstractions/ComponentModel/Data/TODO.md` | Moved to `TODO.md` section 2 |

## Dropped items

- Weeks 3 and 4 plans: GitHub workflow run, `validated-v{version}` tag, scheduled-run watch, per-stack pages (`stacks/…`), LocalStack page (Moto replaced it), Azure B2C and Application Insights stack pages, `.env.liveintegration.template` for services other than Groq.
- Done items from the old work-order (templates, Swashbuckle cleanup, AsyncAPI, port hardening, Docker docs, Groq tests, options validation rollout, naming, `TimeProvider`, most analyzer rules).
- BotChat options 1 and 2 (archive, enhance); option 3 was chosen.

## Kept

`docs/todo.md` stays as the owner's own wish list (including the CGI middleware request); `OPEN_QUESTIONS.md` keeps the owner decisions.
