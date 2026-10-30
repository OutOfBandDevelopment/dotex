---
name: feedback-merge-no-ff
description: "Merges into main are always merge commits (git merge --no-ff), never fast-forward"
metadata:
  node_type: memory
  type: feedback
  originSessionId: 630b5676-695e-448d-af8f-982a4737d775
  modified: 2026-10-29T17:03:55.378Z
---

Always merge into `main` with `git merge --no-ff`; a fast-forward into `main` is not allowed.

**Why:** the owner said "this is very important for versioning": GitVersion bumps the minor on each merge into main, and a fast-forward leaves no merge commit to count.

**How to apply:** when asked to merge into main (rule `merge-to-main`), reset nothing and never run `git merge --ff-only` on main, not even to sync it with origin/main when the branch is also ahead. Create the merge commit, test the result, push main, delete merged origin branches, then start a new `dev/<topic>` branch (the old name can be reused). See [[feedback-stack-one-branch]].
