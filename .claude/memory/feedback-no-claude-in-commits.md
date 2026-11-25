---
name: feedback-no-claude-in-commits
description: Commit messages must never mention Claude/AI (no Co-Authored-By trailer)
metadata:
  node_type: memory
  type: feedback
  originSessionId: 9ea789d6-f84c-46cc-8a8c-3bc487e0ad56
  modified: 2026-09-30T13:54:56.148Z
---

Never reference Claude or AI in commit messages: no `Co-Authored-By: Claude...` trailer, no "generated with" line.

**Why:** the user stated it directly after a commit carried the trailer; it overrides the default attribution and the old CLAUDE.md co-author line.

**How to apply:** write plain commit messages; also applies to PR bodies. Commit only when asked. See [[feedback-docs-plantuml]] for the other standing rule.
