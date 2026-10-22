---
name: work-backlog
description: Work TODO.md and BACKLOG.md item by item with tests, docs and tracking, commit as each item closes, then continue until nothing is left. Use when the owner says to work the backlog, keep going, work through the todo, or signs off (see the autonomous-mode rule).
---

# Work the backlog

Loop until the stop conditions below. No progress reports between items. Standing rules: `work-tracking`, `open-questions`, `git-workflow`, `coding-standards`, `testing`, `owner-decisions`, `autonomous-mode`.

## Pick
1. Read `OPEN-QUESTIONS.md` and act on answered entries first. Then read `TODO.md` and `BACKLOG.md`.
2. Pick the next item you can finish without an owner answer: the owner's newest requests, then items that unblock others, then file order. If an item needs a decision, add a question with options and a recommendation, and move on.

## Work the item
1. Move it from `BACKLOG.md` to `TODO.md`.
2. If the design is not written, write or update the feature doc first (`docs/design/<Feature>/`), recording the decision there.
3. Implement with tests in the right category (`testing` rule). Services outside the process run in Docker containers.
4. Update the project readme and feature docs in the same change; keep `docs/` conventions (`docs-and-diagrams`).
5. Build and run the relevant tests. Never close an item with failing or missing tests; say what failed or was not run.
6. Commit on the working branch with explicit paths and push it to `origin`.

## Close the item
Run the `complete-work` skill: change document, index entry, feature docs, removal from `TODO.md` and `BACKLOG.md`. Then go back to Pick.

## Close-out audit (when nothing is left)
Verify each item closed this run: the code exists, a test covers it, the docs match (`reconcile-designs-and-code` skill). Add missed work to `BACKLOG.md`, reopen items that fail the check, and loop to Pick. Stop only when an audit pass finds nothing new.

## Stop when
- Nothing is left and an audit pass found nothing new; or
- the owner interrupts or asks a question (answer it, then resume); or
- every remaining item waits on an owner answer. With `autonomous-mode` on, record the questions and stop only after one audit; otherwise offer the `owner-interview` skill.
