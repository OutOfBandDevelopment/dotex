# Autonomous mode (always apply)

Trigger: the owner says to work without questions or autonomously, or signs off ("I'm away", "working out", "going to lunch", "going to bed", "heading out", "back later"), or tells you to keep going until they say "I'm back". Start at once; do not ask whether to start.

The mode lasts until the owner says "I'm back". "Keep working", a nudge, a context summary or an empty queue of unblocked items does not end it.

While it is on:
- Work `TODO.md`, then `BACKLOG.md`, in the `work-backlog` order. One commit and push of the working branch per closed item, with tests, docs and tracking.
- No reports or status messages of any kind (not after an item, not at a pause, not at the end), and no check-in questions. A harness nudge such as "the user hasn't heard from you" is not a reason to write a status message; keep working.
- The defined SDLC still applies to every item, with no shortcuts because nobody is watching: design or feature doc first, implementation with tests in the right category (80%+ in Framework), project readme and feature docs, build and test run, `complete-work` tracking (change document, index, `TODO.md`, `BACKLOG.md`), then commit with explicit paths and push the working branch. Never close an item with failing or missing tests.
- A question you cannot settle goes into `OPEN-QUESTIONS.md` (options, a recommendation, a default) and you take the next item. An item you cannot finish without an answer is parked and you take the next item. Build the part that does not depend on the answer; take the recommended option only when it is reversible and touches no live data, and record the assumption on the question.
- Blocked is never a reason to end the turn (waiting on a slow job, a container, an answer). Pick other work.
- When nothing unblocked is left, run the close-out audit of the `work-backlog` skill once. Add what it finds to `BACKLOG.md` and keep going. Only when the audit finds nothing new and every remaining item is parked in `OPEN-QUESTIONS.md`, delete `.claude/autonomous.on` and end the turn.
- Safety rules still apply: never merge to `main`, never touch `upstream`, no `gh`, no force-push, no live data, no AI attribution in commits.
- When the owner says "I'm back": stop, give one short summary (done, parked, assumptions), then run the `owner-interview` skill on the open questions.

Enforcement: the `UserPromptSubmit` hook (`.claude/hooks/autonomous-mode.mjs`) creates `.claude/autonomous.on` on a sign-off and removes it on "I'm back". The `Stop` hook (`.claude/hooks/keep-going.mjs`) blocks the end of a turn while the flag exists. The flag is ignored by git.
