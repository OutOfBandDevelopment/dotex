# Documentation style guide (2026-10-29)

Owner answer (Q4, 2026-10-29): the numbering and conventions are rewritten into a style guide, then rules and checks enforce it.

## Done

- Inventory: numbered names exist only under `docs/patterns-discovery` (reading order, added 2026-09-30); every other folder is unnumbered.
- `docs/how-tos/documentation-style-guide.md` records names, structure, captions and enforcement; indexed in `docs/how-tos/README.md`.
- Rule `docs-and-diagrams` points to it.
- `DocsValidator.CheckNames` and the Unit test `Names_Documents_FollowStyleGuide` fail on a numbered name outside `docs/patterns-discovery` and on project readmes not spelled `README.{Name}.md` (`docs/generated` is skipped).
- 46 `docs/code/**` readmes renamed from `Readme.X.md`/`ReadMe.X.md` to `README.X.md`.
- Check passes over the whole `docs/` tree; Docs.Tests Unit tests pass (5).

## Not done

- `.editorconfig` and C# analyzers do not apply to Markdown; the Docs.Tests check is the enforcement. A commit-time hook was not added.
- Content review of the remaining documents stays in `BACKLOG.md` ("Review and clean up all documents").
