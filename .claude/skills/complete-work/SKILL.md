---
name: complete-work
description: Close out a finished piece of work: write the change document, update the index and feature docs, and move the item out of TODO.md and BACKLOG.md. Use whenever an item in TODO.md is finished and verified, when the owner says work is "done" or "completed", or before the last commit of a feature.
---

# Complete work

Only for work that was built and tested. If something was not run, finish only the verified part and leave the rest in `TODO.md` with a note.

1. **Verify.** Run the build and the tests that cover the change (`cd src && dotnet build --nologo -v q`, `dotnet test <project>`). Note the real results, including failures and what was not run.
2. **Change document.** Create `docs/changes/{epic}-{feature}-{YYYY-MM-DD}.md` (examples in that folder): date, epic, status, summary, a table of what was delivered, owner decisions, verification, follow-up. Keep it short and factual.
3. **Index.** Add an entry to `docs/changes/README.md` under the matching heading (bold link, one summary line, `Status:` line).
4. **Feature docs.** Update the docs that describe the feature so they match the code: `docs/design/<Feature>/` (requirements, api-design, testing-strategy), the project's `README.{Project}.md`, and any `docs/patterns-discovery/` page it touches. Mark design docs `Implemented` once code exists.
5. **Tracking.** Remove the finished item from `TODO.md` and `BACKLOG.md`. Move leftovers into `BACKLOG.md` (and the next ones into `TODO.md`). If an owner question was answered by the work, drop it from `OPEN-QUESTIONS.md` and put the answer in the feature doc.
6. **`CLAUDE.md`.** Add one line under "Recently Completed Work" only for work the next session must know about; link the change document.
7. **Commit** in small steps on the working branch, staging explicit paths (`.claude/rules/git-workflow.md`): code and tests first, then the tracking and docs files, or together when small. No push or merge without the owner.
8. Run `python scripts/docs/validate-docs.py <changed docs folder>` when Python is available; otherwise say it was not run.
