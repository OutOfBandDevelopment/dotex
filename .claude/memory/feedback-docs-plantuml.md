---
name: feedback-docs-plantuml
description: Diagrams in markdown docs must be PlantUML; UI mockups must be PlantUML+Salt
metadata:
  node_type: memory
  type: feedback
  originSessionId: 9ea789d6-f84c-46cc-8a8c-3bc487e0ad56
  modified: 2026-09-30T13:11:23.331Z
---

All diagrams in markdown docs are PlantUML (no ASCII art/Mermaid); mockups are PlantUML + Salt embedded in the markdown. Design docs follow docs/patterns-discovery/06-design-document-standard.md.

**Why:** User stated preference during patterns-discovery work; asked it be recorded in CLAUDE.md.
**How to apply:** Any doc I write or touch; convert old ASCII diagrams.

Also: include Table of Contents, List of Figures and List of Tables where useful; caption figures/tables (see CLAUDE.md "Documentation Diagram Standards").

Also: split long docs into topic folder + README index + one file per headline; prefer valid relative cross-references (rules in CLAUDE.md).

Also: architecture diagrams are C4-style but plain PlantUML — never `!include` the C4 templates (they break in production).

Also: always validate PlantUML by rendering (python scripts/docs/validate-docs.py docs; docker plantuml-server on :18080). Prefer deterministic scripts over agentic LLM work. Gotcha: typing backslash-n in tool input can turn into a real newline; use chr(92)+'n' in scripts.
