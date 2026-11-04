# Change history as one file per day

**Status:** in progress (2026-11-04) · **Backlog:** [BACKLOG.md](../../BACKLOG.md) section 7 · **Request:** owner, 2026-11-04

Owner request: the change history becomes a daily file with the completed tasks of that day, instead of one dated file per task.

## Target

- `docs/changes/{YYYY-MM-DD}.md`: one file per day, one `##` section per finished task (the task's former title), with the task's own headings one level lower. A day that already has a file keeps it and gains the sections.
- `docs/changes/README.md` indexes days, newest first, with one line per task linking the section anchor.
- Every link in the repository to a former per-task file points at `{date}.md#{section-anchor}`.
- Rules and skills (`work-tracking`, `complete-work`, `archive-completed-work`) name the daily file.

## Steps

1. A deterministic script (`scripts/docs/`) merges the per-task files by date, rewrites links and regenerates the index.
2. Run the docs tests over `docs` and the build-independent link checks.
3. Update the rule, the skills, `CLAUDE.md` and the change-document template.
4. Delete this file when done.
