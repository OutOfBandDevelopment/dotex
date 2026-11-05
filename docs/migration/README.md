# Migration documentation

**Last Updated:** 2026-10-29

Plans, mappings and analyses for code brought into the framework from other repositories. A migration that is finished lives on as feature documents (`docs/design/`, `docs/changes/`) and project readmes; its notes here are removed (owner request, 2026-10-29). `git log -- docs/migration` has the removed files.

## Contents

- [Inventory](#inventory)
- [Rules](#rules)
- [Removed on 2026-10-29](#removed-on-2026-10-29)

## Inventory

**Table 1 — Migration notes and what is left**

| Document | Subject | State |
|----------|---------|-------|
| [binarydatadecoders-rereview-2026-10-29.md](binarydatadecoders-rereview-2026-10-29.md) | BinaryDataDecoders against `C:\repo\mine\binary-data-decoders` (replaces the January mapping, plan, decision sheet and critical questions) | Current; read first |
| [device-decoding-migration-plan.md](device-decoding-migration-plan.md), [device-parity.md](device-parity.md) | dev-term device decoding | Packages done; transports, manifests and the shells are tracked as "DevTerm 100% parity" in the backlog |
| [overlap-review-2026-10-29.md](overlap-review-2026-10-29.md) | BinaryDataDecoders against dev-term against dotex | Current; dev-term is the base wherever both have the code |
| [sharedframework.md](sharedframework.md) | SharedFramework | Replaced by design documents (Epics 2, 6, 7, 10) |
| [oobtainium.md](oobtainium.md) | Oobtainium mocking framework | Not migrated; evaluate later (decision D7) |
| [incoming-recommendation-plan.md](incoming-recommendation-plan.md), [incoming-harvest-crosscheck.md](incoming-harvest-crosscheck.md) | Cross-project plan and rejected merges | Current |

## Rules

- Everything from BinaryDataDecoders migrates; phases are priority order, not a selection (`CLAUDE.md`).
- Compare every source and keep the best-covered implementation (`owner-decisions`); document rejections with the reason.
- Open work is tracked in [BACKLOG.md](../../BACKLOG.md) and [TODO.md](../../TODO.md); finished work in [docs/changes](../changes/README.md).

## Removed on 2026-10-29

- BotChat mapping and plan: the patterns were extracted on 2026-10-09 and the sources removed.
- Vector comparison: the application-level `Vector` type is in `OoBDev.System.Abstractions` (`Math`), documented in its readme and tested; the cosine distance versus similarity difference is recorded there.
- MailKit and SendGrid integration analysis: both integrations are complete and described in `README.MailKit.md` and `README.MailKit.Hosting.md`.
- Consolidated on 2026-10-29: the BinaryDataDecoders feature mapping, migration plan, decision sheet and critical questions (now the re-review), the four SharedFramework analyses (now `sharedframework.md`) and the two Oobtainium notes (now `oobtainium.md`).
