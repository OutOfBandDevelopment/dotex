---
name: feedback-change-doc-by-date
description: Change documents go in docs/changes/{today}.md, named for the day the work finished, not the day the previous file started
metadata:
  type: feedback
---

Write each finished task to `docs/changes/{YYYY-MM-DD}.md` for the current date (owner, 2026-11-05).

**Why:** work after midnight was appended to the previous day's file with "(2026-11-05)" in the section titles; the owner asked "where is it? it should be docs\changes\2026-11-05.md".

**How to apply:** check today's date before `complete-work`; start a new day file (title, contents list, sections) when the date changes and add the `By day` line in `docs/changes/README.md`. See [[feedback-define-once-issues]] for the other tracking conventions.
