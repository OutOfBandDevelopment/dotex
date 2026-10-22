---
paths:
  - "docs/**"
  - "**/*.md"
  - "templates/**"
---

# Documentation rules

- All diagrams are PlantUML fenced blocks (`@startuml`). UI mockups are PlantUML Salt (`@startsalt`). Never ASCII art or Mermaid. Convert existing ASCII diagrams when touching a document. Directory trees in code blocks are fine.
- Architecture diagrams use C4 style (Context/Container/Component) in plain PlantUML: rectangles, stereotypes, skinparam. Never `!include` C4-PlantUML templates.
- Add a table of contents, list of figures and list of tables when a document has 3+ sections, 2+ figures or 2+ tables. Captions: `*Figure N — title*` after a figure, `**Table N — title**` before a table.
- Split documents longer than about 250 lines or 6 major sections into a folder with a `README.md` index and one file per headline, with nav links. Use relative cross references (file plus `#anchor`).
- Design documents are four files per feature (requirements, architecture, api-design, testing-strategy); see `docs/patterns-discovery/06-design-document-standard.md`.
- Only `README.md`, `CLAUDE.md` and `TODO.md` live at the repo root, besides the existing `TEST_VARIABLES.md` and `CONFIGURATION_SETTINGS.md`. Everything else goes under `docs/`.
- Patterns docs record the owner's preferences; do not "correct" them. Rough edges go to the `TODO.md` backlogs.
- Prefer deterministic scripts in `scripts/docs/` over hand edits for validation, indexes, captions and link fixing.
- Before finishing any doc change run `python scripts/docs/validate-docs.py <changed folder>`. It needs the docker `plantuml/plantuml-server` image (port 18080). Report problems you did not fix. Write helper scripts with the Write tool, not shell heredocs.
