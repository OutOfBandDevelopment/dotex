---
name: feedback-stack-one-branch
description: "Local bare repo origin is primary (not GitHub); commit as you work on a branch, stacked on one branch; no PRs, no gh"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 3173075a-b625-4d91-a395-70b17ac78d0a
  modified: 2026-10-22T11:21:00.619Z
---

The primary repo is the local bare repo `origin` (`G:/oobdev/dotex.git`). GitHub (`upstream`) is no longer primary: do not push there or use `gh`. Commit as you work, on a working branch (never directly on `main`), stack related work on that one branch, and never open or ask about a pull request.

**Why:** The owner said "I told you to just stack all changes in one branch, I will do the PR later", then on 2026-10-22 "gh is no longer the primary repo ... everything is done locally in a bare repo" and "you can commit your changes as you work... but you should probably do it in a branch".

**How to apply:** Create `dev/<topic>` if on `main`, commit small steps, push to `origin` and merge into `main` only when asked, verify locally instead of waiting on GitHub Actions. Enforced by `.claude/hooks/guard-git.mjs` and `.claude/rules/git-workflow.md`. The Groq key was revoked on 2026-10-09, so stop reminding about it.
