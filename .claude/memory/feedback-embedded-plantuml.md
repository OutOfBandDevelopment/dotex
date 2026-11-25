---
name: feedback-embedded-plantuml
description: Every diagram in markdown is an embedded PlantUML block, never ASCII art; enforced by a hook
metadata:
  type: feedback
---

All diagrams in docs are embedded PlantUML fenced blocks (Salt for mockups), never box-drawing or ASCII art, including in moved or imported documents.

**Why:** the owner flagged ASCII diagrams in `docs/design/Communications/ChannelAbstraction/architecture.md` and said "same with all diagrams", and wanted it enforced, not remembered (2026-11-05).
**How to apply:** the `no-ascii-diagrams` PreToolUse hook blocks it on Write/Edit; convert existing ones when touching a file (BACKLOG item for the rest). Related: [[feedback-docs-plantuml]].
