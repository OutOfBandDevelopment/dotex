---
name: feedback-migration-docs-pattern
description: Pattern for migrating a project's docs - import under docs/applications/{app}, keep change history, transient map, cleanup later
metadata:
  type: feedback
---

When migrating a project, import its docs under `docs/applications/{app-name}/` (reusable designs to `docs/design/<Feature>/`, protocol references to `docs/protocols/`), keep its change history and bug reports as retained information, and keep a transient map in `docs/migration/` (deleted once integrated). Documentation cleanup and integration is a separate plan after all migrations finish.

**Why:** owner (2026-10-30): the dev-term migration "looks pretty good"; change history is kept so no important feature is lost, cleanup comes later, and the pattern should carry to future migrations. Living documents, never ADRs.

**How to apply:** repeat this for BinaryDataDecoders and other projects; do not delete imported history or fold it into living docs unless asked. See [[feedback-migrate-whole-project]].
