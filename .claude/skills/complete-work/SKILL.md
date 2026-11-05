---
name: complete-work
description: Close out a finished piece of work: write the change document, update the index and feature docs, and move the item out of TODO.md and BACKLOG.md. Use whenever an item in TODO.md is finished and verified, when the owner says work is "done" or "completed", or before the last commit of a feature.
---

# Complete work

Only for work that was built and tested. If something was not run, finish only the verified part and leave the rest in `TODO.md` with a note.

1. **Verify.** Run the build and the tests that cover the change (`cd src && dotnet build --nologo -v q`, `dotnet test <project>`). Note the real results, including failures and what was not run.
2. **Change document.** Add a `##` section to today's `docs/changes/{YYYY-MM-DD}.md` (create it with `# Changes {date}` when it is the first task of the day; examples in that folder): status, summary, a table of what was delivered, owner decisions, verification, follow-up. Keep it short and factual.
3. **Index.** Add the day to "By day" in `docs/changes/README.md` if new, and an entry under the matching heading (bold link to `{date}.md#section`, one summary line, `Status:` line).
4. **Feature docs.** Update the docs that describe the feature so they match the code: `docs/design/<Feature>/` (requirements, api-design, testing-strategy), the project's `README.{Project}.md`, and any `docs/patterns-discovery/` page it touches. Mark design docs `Implemented` once code exists.
5. **Tracking.** Remove the finished item from `TODO.md` and `BACKLOG.md`, and any duplicate or superseded line about the same work (search both files for the feature name). Set `Last Updated` to today in each file you touch. Move leftovers into `BACKLOG.md` (and the next ones into `TODO.md`). If an owner question was answered by the work, drop it from `OPEN-QUESTIONS.md` and put the answer in the feature doc.
6. **`CLAUDE.md`.** If the focus changed, update "Current Work Context" to match `TODO.md`. Add one line under "Recently Completed Work" only for work the next session must know about; link the change document.
7. **Commit** in small steps on the working branch, staging explicit paths (`.claude/rules/git-workflow.md`): code and tests first, then the tracking and docs files, or together when small. No push or merge without the owner.
8. Run `OoBDev.Docs.Tests` with `DOCS_VALIDATE_PATHS=<changed docs folder>`; say so if it was not run.
