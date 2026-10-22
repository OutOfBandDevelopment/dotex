---
name: write-docs
description: Write or restructure project documentation (readmes, design documents, architecture docs, how-tos) to the repo's standards. Use when creating or substantially editing anything under docs/ or a README.{Project}.md, or when asked about documentation structure or style.
---

# Write documentation

The enforceable rules (PlantUML only, captions, TOC, splitting, validation, root file limits) are in `.claude/rules/docs-and-diagrams.md`; follow them. Then:

1. Look for an existing document first and extend it. Add new ones to the nearest `README.md` index.
2. Features get four design documents (requirements, architecture, api-design, testing-strategy) per `docs/patterns-discovery/06-design-document-standard.md`.
3. Include code examples, relative links to related documents, and a "Last Updated" date. Project readmes are `README.{Project}.md`.
4. Run `OoBDev.Docs.Tests` with `DOCS_VALIDATE_PATHS=<folder>` (the render test needs the existing `plantuml` container on port 8086; do not start another), fix what you introduced, and report remaining problems. The default scope `docs/patterns-discovery` must stay at 0 problems; the whole `docs/` tree has old problems.
5. Prefer scripts in `scripts/docs/` (index, captions, link fixes) over hand edits.

Review checklist before you finish a document:
- Rules and constraints say why, not just what the code does.
- Where a doc describes enforcement, the code has a one-line pointer to it; move long explanatory code comments into the doc when you touch the file.
- A decision is a dated row that names the rejected alternative.
- Never hand-edit generated references; hand-copied schema or option blocks are compared with the code.
- Measured numbers carry the host and the date.
- Claims match the running code or a test; open the code instead of trusting the old doc.
- A feature doc starts with an at-a-glance summary: benefit, state, one diagram, one risk, in words a non-specialist follows.

References (older, from the former protocols; the rule file wins on conflicts such as ASCII diagrams): `references/style-guide.md` (content standards and checklist), `references/structure.md` (templates), `references/standards.md` (file organization and naming).
