# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q42 — 2026-11-25 — Desired state, store instances: where does the shared instance type live?

Store instances ([design](docs/design/DesiredStateDatabase/instances.md)) need a small shared piece every store uses: an options record (`SchemaName`, `TablePrefix`), a helper that applies it in `OnModelCreating`, and an `IModelCacheKeyFactory` so two instances of one context get two models. It must be Framework (no host), and only EF Core.

**Options**
- A (recommended): a new small project `OoBDev.Data.EntityFrameworkCore` in `src/Framework` (references EF Core Relational only; stores reference it). Reason: it is reusable without pulling the sqldef tool wrapper into every store, and a store keeps its current references plus one.
- B: put it in `OoBDev.Data.DesiredState` (already references EF). No new project, but every store would depend on the sqldef wrapper just to name a schema.
- C: copy the few lines into each store. No coupling, but five copies of the cache key factory.

**Default:** build the first slice (Suppressions) with the type inside `OoBDev.Communications.EntityFrameworkCore` as `internal`, which is reversible; move it when you answer. A migration plan row for A: project `OoBDev.Data.EntityFrameworkCore`, `src/Framework`, namespace `OoBDev.Data.EntityFrameworkCore`, NuGet package of the same name, referenced by Communications, AspNetCore, ComplexEvents and EventSourcing EntityFrameworkCore projects.

**Your answer:**

### Q43 — 2026-11-25 — Remote DevTerm: what does a remote connection in the Tui and WPF apps connect to?

The web host's `/ws` tunnel today is a text session onto the host's own single shared connection (lines in, lines out). Slice 1 (token and JWT) is built ([design](docs/design/RemoteDevTerm/README.md)). Slices 2 to 4 need a decision on what the remote client attaches to, because it changes the protocol.

**Options**
- A (recommended): the remote client drives a device on the host machine: it asks the host to open a connection (serial port, HID, USBTMC, by the same connection string the local apps use), and the host relays frames both ways, one client per device claim. Reason: this is what the raw USB bridge did and what you decided to replace (Q41 A); the local transport model stays unchanged on the client (a `RemoteTransport` beside serial/TCP/HID/USBTMC), and device panels work as if local.
- B: the remote client only mirrors the host's own shared session (today's `/ws` behaviour plus panels). Small, no new frames, but HID and USBTMC are not reachable unless the host already opened them, so the bridge cannot be removed.
- C: both: B first (cheap), A after. Two protocols to keep until A lands.

**Default:** A. Build slice 2 for serial only (framed binary over a new `/ws/device` route, same JWT), then HID and USBTMC; the existing text `/ws` stays as it is. Reversible: nothing is removed until slice 5.

**Your answer:**
