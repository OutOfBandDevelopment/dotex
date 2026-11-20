# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q24 (2026-11-19, MetadataQuery): do you want "search by description" and cached snapshots in the query engine?

[Design](docs/design/MetadataQuery/README.md#not-in-the-first-slice). Two ideas were left out of the first slice:

- **Text anchor**: today "find rows similar to row 42" works. A text anchor would let a caller say "find rows similar to the words *red barn at sunset*". The engine would turn those words into numbers with an embedding model, then rank by closeness. It needs an embedder at query time.
- **Snapshot source**: keep a saved copy of a query result so repeat requests are quick. It is a cache. No screen needs it yet.

**Options**
- A. Drop both until a screen needs them (recommended: no consumer, and a cache without measurements is guesswork).
- B. Build text anchors only (adds an embedder dependency to the query engine).
- C. Leave them listed as they are.

**Default:** C.

**Your answer:**
