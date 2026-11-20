# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q24 (2026-11-19, MetadataQuery): are snapshot sources and the anchor store wanted, and with which transport?

[Design](docs/design/MetadataQuery/README.md#not-in-the-first-slice). Left open: snapshot sources and text anchors (the catalog's caching), and the GraphQL and OData transports. The design has no requirements for the first, and the second waits on the transport evaluation.

**Options**
- A. Drop snapshot sources and the anchor store until a consumer shows a measured need; take the transports up in the HTTP query evaluation (recommended: caching without a consumer is guesswork, and `Candidates` already covers the vector case).
- B. Port the catalog's anchor store now as an optional cache project (needs you to name which catalog code is the reference).
- C. Keep them listed as is.

**Default:** C.

**Your answer:**
