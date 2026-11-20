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

### Q25 (2026-11-19, NetworkDevices): should the network device probes move out of DevTerm into a Framework library?

The probes (`INetworkDeviceProbe`, mDNS, SSDP, USR bridge, `NetworkDiscovery`) live in `src/Applications/DevTerm/OoBDev.DevTerm.Core/Configuration/Discovery/` and have no DevTerm-specific names. The VXI-11 portmapper scan is already Framework (`OoBDev.System.IO.Tcp.LxiDiscovery`). Rule `layering-and-migration-plans`: reusable code is Framework, and a move needs a confirmed plan.

**Options**
- A. Move them to a new Framework library `OoBDev.System.Net.Discovery` (namespace and project reference changes in DevTerm; plan written first) (recommended: other tools such as the file inventory CLI or a device registry can reuse them, and nothing there is host-bound).
- B. Keep them in DevTerm until a second consumer exists.
- C. Put them next to `LxiDiscovery` in `OoBDev.System.IO.Tcp`.

**Default:** B.

**Your answer:**

### Q26 (2026-11-19, DevTerm): is a Blazor front end for DevTerm wanted, and as a separate application or inside the example web app?

[Request](docs/features/request/2026-11-10-devterm-dockable-layout-and-blazor-frontend.md). The WPF dockable layout is built; the Blazor half is open. It is a layering and application-boundary choice, so no code starts without an answer (rule `layering-and-migration-plans`).

**Options**
- A. Defer until a web user for DevTerm exists; the generated-form language already keeps the view models host-neutral (recommended: no consumer yet, nothing to rework later).
- B. A separate `OoBDev.DevTerm.Web` Blazor application under `src/Applications/DevTerm`, reusing `DevTerm.Core`.
- C. Pages inside the example web app.

**Default:** A.

**Your answer:**
