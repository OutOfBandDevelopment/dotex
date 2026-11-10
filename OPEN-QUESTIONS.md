# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 — 2026-11-09 — DevTerm manifest editor: how should Pick work on one channel's expression segment?

In Channels mode the picker edits the whole `id[:label[:#RRGGBB[:expression]]]; ...` list, and choosing a value appends a new channel, so it cannot also insert `{id}` into one channel's expression (the fourth segment). Parameter-field picking is already built (`ExpressionList` mode).

**Options**
- A. A second **Pick...** per channel row (recommended): the chart's Channels field stays as is; the picker gets a channel selector, and when a channel is selected the dialog switches to Expression mode for its fourth segment (functions, sample result) and writes it back. Trade-off: the most work, but it reuses Expression mode wholesale.
- B. Let Channels mode insert at the caret when the caret is inside a fourth segment, and append otherwise. Trade-off: small change, but the same double-click does two things depending on the caret.
- C. Leave channel expressions typed by hand (diagnostics already check them). Trade-off: no work, no help building them.

**Default:** C until answered (nothing is built on a guess).

**Your answer:**
