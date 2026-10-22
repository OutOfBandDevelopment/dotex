---
name: archive-completed-work
description: Archive completed work from TODO files and CLAUDE.md into docs/changes to cut context overhead. Use when asked to "clean up your task list", "archive completed work", "reduce context overhead", or when TODO.md or a TODO file passes about 400 lines.
---

# Archive completed work

Only archive items that are marked complete AND were verified (build passes, tests pass). Do not archive anything unverified, and do not touch the owner's unrelated pending edits.

1. Read `TODO.md` (and any `TODO-{epic}.md`) and list the verified-complete items. Ask when completeness is unclear.
2. Create `docs/changes/{category}-{feature}-{YYYY-MM-DD}.md` using the template in `reference.md`: summary, what changed, files, decisions, tests, follow-ups. Existing files in `docs/changes/` show the naming (`migration-`, `testing-`, `bug-fixes-`, `documentation-` prefixes).
3. In the TODO file replace the detail with a one-line summary plus a link to the change document. Keep open items and every `Backlog:` section.
4. Add a line under "Recently Completed Work" in `CLAUDE.md` (newest date first, one line each with a details link). Trim older entries that now live only in `docs/changes/`.
5. Add the document to the index in `docs/changes/README.md`.
6. Run `python scripts/docs/validate-docs.py docs/changes` if the document has diagrams or links (PlantUML only; no ASCII art).
7. Report what moved, what stayed, and the line counts before and after. Do not commit unless asked.

`reference.md` has the full original protocol and template. Its examples (for example the Swashbuckle entry) are historical.
