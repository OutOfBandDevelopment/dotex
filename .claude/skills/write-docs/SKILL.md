---
name: write-docs
description: Write or restructure project documentation (readmes, design documents, architecture docs, how-tos) to the repo's standards. Use when creating or substantially editing anything under docs/ or a README.{Project}.md, or when asked about documentation structure or style.
---

# Write documentation

The enforceable rules (PlantUML only, captions, TOC, splitting, validation, root file limits) are in `.claude/rules/docs-and-diagrams.md`; follow them. Then:

1. Look for an existing document first and extend it. Add new ones to the nearest `README.md` index.
2. Features get four design documents (requirements, architecture, api-design, testing-strategy) per `docs/patterns-discovery/06-design-document-standard.md`.
3. Include code examples, relative links to related documents, and a "Last Updated" date. Project readmes are `README.{Project}.md`.
4. Run `python scripts/docs/validate-docs.py <folder>` (needs docker `plantuml/plantuml-server`), fix what you introduced, and report remaining problems. The whole `docs/` tree has about 566 old problems; `docs/patterns-discovery` should be 0.
5. Prefer scripts in `scripts/docs/` (index, captions, link fixes) over hand edits.

References (older, from the former protocols; the rule file wins on conflicts such as ASCII diagrams): `references/style-guide.md` (content standards and checklist), `references/structure.md` (templates), `references/standards.md` (file organization and naming).
