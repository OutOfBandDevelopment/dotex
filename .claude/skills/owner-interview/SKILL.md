---
name: owner-interview
description: Interview the owner to settle the open entries in OPEN-QUESTIONS.md and other owner-blocked items. Use when the owner says interview me, ask me the questions, or unblock the backlog, and when the owner returns after an autonomous run.
---

# Owner interview

Run a short discovery interview. Goal: turn each open question into a decision, a scope and a way to verify it, then write it down so `work-backlog` can proceed.

## Prepare
1. Start from `OPEN-QUESTIONS.md`. Add other owner-blocked items you find (`TODO.md` or `BACKLOG.md` lines that wait on a decision, such as the migration decisions in `docs/migration/`) as entries first, so the file stays the single list.
2. Group by theme (safety and approvals, what unblocks the most work, then design details). Read the relevant design doc or code first so you ask about the real situation, with facts at hand.

## Interview rules
- One theme at a time, one to three questions per turn, using `AskUserQuestion` with concrete options. Recommendation first, marked "(Recommended)", with the trade-off in each description.
- Follow the question standard in `.claude/rules/open-questions.md`: plain words, one decision, consequences of each option, a recommendation grounded in the owner's earlier stated preferences (cite them; check `owner-decisions`, the memory index and the docs first; if there is none, say the recommendation is yours).
- Ask about the job before the feature: who does this, how often, what goes wrong, what must never happen.
- Probe once on a vague answer, then move on. Do not re-ask a decided question.
- A "yes" approves exactly what was named.

## Record
After each theme, in one commit: quote the answer in the entry, process it per the `open-questions` rule (feature doc, `BACKLOG.md`, `TODO.md`, rule), delete the entry from the file, and add a change document if work finished.

## Finish
Short summary: decisions made, approvals granted with their scope, entries still open and why. Offer to resume `work-backlog`.
