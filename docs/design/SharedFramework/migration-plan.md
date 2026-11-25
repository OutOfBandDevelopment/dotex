# SharedFramework proposals - migration plan into `docs/design/`

**Last Updated:** 2026-11-05
**Status:** proposed; waits for the owner's answer to [OPEN-QUESTIONS.md](../../../OPEN-QUESTIONS.md) Q4 (rule `layering-and-migration-plans`)

The eight epic folders under `Features/Proposals/` hold the SharedFramework design sets (134 markdown files). Design documents belong in `docs/design/<Feature>/` (rule `docs-and-diagrams`). This plan moves them without changing content.

## Document move

**Table 1 — Folders to move**

| Current (`Features/Proposals/`) | Target (`docs/design/`) | Files |
|---|---|---|
| `02-Communications` | `Communications` | 15 |
| `04-Caching` | `SharedCaching` | 12 |
| `05-MasterData` | `MasterData` | 10 |
| `06-DocumentServices` | `DocumentServices` | 44 |
| `07-Identity` | `SharedIdentity` | 16 |
| `10-TextTemplating` | `TextTemplating` | 14 |
| `11-DataEnhancement` | `DataEnhancement` | 19 |
| `12-MessageComposition` | `MessageComposition` | 4 |
| `Features/Proposals/*.md` (epic review, consolidated design, progress, revisions) | `docs/design/SharedFramework/` | 7 |

`SharedCaching` and `SharedIdentity` carry a prefix because `docs/design` already may hold framework-level caching and identity designs; the number prefixes go (reading-order numbers are for `docs/patterns-discovery` only). Steps: `git mv` per folder, rewrite relative links with `scripts/docs/` link fixing, run `DOCS_VALIDATE_PATHS=docs/design dotnet test src/Tools/OoBDev.Docs.Tests`, update every link in `BACKLOG.md`, `TODO.md`, `CLAUDE.md` and the change documents, and leave no `Features/` folder behind.

## Projects

The projects named by the Communications design already exist in `src/Framework` (`OoBDev.Communications.Abstractions`, `OoBDev.Communications`, `OoBDev.Communications.MessageQueueing`, `OoBDev.Communications.Tests`), as do the Caching, Documents, Identity and MessageQueueing counterparts of the other epics, so no project is created, moved or renamed by this plan. What each existing project holds versus its design is a separate reconciliation (skill `reconcile-designs-and-code`), tracked in `BACKLOG.md`. New vendor adapters would go to `src/ExternalServices/<Vendor>` and reference Abstractions only.

## What does not change

Document content, file names inside each folder and the four-document set per feature stay as they are. Nothing under `src/` moves in this plan.
