# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open


### Q-CQRS-EDGE — Where does the CloudEvents edge live? (2026-11-11, CqrsEventSourcing)

`CloudEventMapper` (EventRecord <-> CloudEvents 1.0 JSON) is built and tested but connected to nothing.

**Options**
- A. A queue-side option in `OoBDev.EventSourcing` (Framework, host-free): a `CloudEventsEventsHandler` that accepts a CloudEvent string from any queue and appends it to the event store, plus a sender wrapper that publishes CloudEvents instead of `EventRecord` JSON, chosen by config (recommended: no ASP.NET dependency, matches the "edge option on the queue pattern" decision and `layering-and-migration-plans`).
- B. An HTTP endpoint (`POST /events` with `application/cloudevents+json`) in an ASP.NET host library. Needs a web-bound project; where it lives is a layering decision (new `OoBDev.EventSourcing.Web` vs an application).
- C. Both A and B.

**Default:** A is built later; B waits for your answer.

**Your answer:**

### Q-GRAPH-CONTRACT — Where does the graph retrieval contract live? (2026-11-11, GraphQueries / HybridSearch)

The Neo4j adapter needs a provider-neutral contract (`IGraphStore`: upsert node, connect, neighbors within N steps, shortest path, delete) so SQL Server node/edge tables or Apache AGE can sit behind it later.

**Options**
- A. `OoBDev.Search.Abstractions` (namespace `OoBDev.Search.Graph`), beside `IVectorStore`: no new project, adapters already reference it, and graph retrieval is a search source for the hybrid pipeline. Cost: the Search abstraction grows past vectors.
- B. A new `OoBDev.Graph.Abstractions` Framework project: clean separation, but one more package for two small interfaces. (recommended once a second provider exists)
- C. Adapter-only (no shared contract): fastest, but locks callers to Neo4j.

**Default:** A (reversible: moving the namespace to B later is a rename, the framework is unreleased).

**Your answer:**
