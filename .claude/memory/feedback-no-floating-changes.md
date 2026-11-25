---
name: feedback-no-floating-changes
description: Never leave modified files uncommitted for long; a missed related change is committed later as its own commit
metadata:
  type: feedback
---

Do not leave generated or derived files (catalog pages, regenerated screenshots, readmes) sitting modified in the working tree. If a change was missed in the commit of the work it belongs to, commit it afterwards as a small commit with explicit paths.

**Why:** owner, 2026-11-25: "if you miss the related change then commit them later, it's that easy. Don't leave the stuff floating forever." Stray modifications make `git status` noisy and hide real changes.

**How to apply:** before ending a task run `git status`; commit what belongs to the work (explicit paths), restore noise (for example regenerated screenshots with no content change, see [[feedback-commit-user-guides]]), and never leave the rest. Only files the owner edited themselves stay untouched (rule `git-workflow`).
