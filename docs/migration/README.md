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
| [binarydatadecoders-rereview-2026-10-29.md](binarydatadecoders-rereview-2026-10-29.md) | Current state of the BinaryDataDecoders port against `C:\repo\mine\binary-data-decoders` | Current; read first |
| [binarydatadecoders-feature-mapping.md](binarydatadecoders-feature-mapping.md), [binarydatadecoders-migration-plan.md](binarydatadecoders-migration-plan.md) | Original mapping and phases (January 2026) | Open: the port is partly done, the phases are priority order |
| [binarydatadecoders-decision-sheet.md](binarydatadecoders-decision-sheet.md), [binarydatadecoders-critical-questions.md](binarydatadecoders-critical-questions.md) | Owner decisions | Open: awaiting decisions |
| [device-decoding-migration-plan.md](device-decoding-migration-plan.md), [device-parity.md](device-parity.md) | dev-term device decoding | Packages done; transports and the shell wait on the Q3 scope |
| [sharedframework-feature-mapping.md](sharedframework-feature-mapping.md), [sharedframework-migration-plan.md](sharedframework-migration-plan.md), [sharedframework-differs-detailed-comparison.md](sharedframework-differs-detailed-comparison.md), [sharedframework-phase0-coverage-analysis.md](sharedframework-phase0-coverage-analysis.md) | SharedFramework | Open: design first (Epics 2, 6, 7, 10) |
| [oobtainium-feature-mapping.md](oobtainium-feature-mapping.md), [oobtainium-migration-plan.md](oobtainium-migration-plan.md) | Oobtainium mocking and proxy framework | Open: evaluate later (decision D7) |
| [vector-comparison.md](vector-comparison.md) | Application-level vector math beside the SQL vectors | Open: recommendation to migrate, not done |
| [incoming-recommendation-plan.md](incoming-recommendation-plan.md), [incoming-harvest-crosscheck.md](incoming-harvest-crosscheck.md) | Cross-project plan and rejected merges | Current |

## Rules

- Everything from BinaryDataDecoders migrates; phases are priority order, not a selection (`CLAUDE.md`).
- Compare every source and keep the best-covered implementation (`owner-decisions`); document rejections with the reason.
- Open work is tracked in [BACKLOG.md](../../BACKLOG.md) and [TODO.md](../../TODO.md); finished work in [docs/changes](../changes/README.md).

## Removed on 2026-10-29

- BotChat mapping and plan: the patterns were extracted on 2026-10-09 and the sources removed.
- MailKit and SendGrid integration analysis: both integrations are complete and described in `README.MailKit.md` and `README.MailKit.Hosting.md`.
