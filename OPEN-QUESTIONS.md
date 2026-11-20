# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q21 (2026-11-19, Identity and DevTerm design): claims pipeline and control-module schema

Requests: [claims enhancement](docs/features/request/2026-11-10-claims-enhancement-and-user-session.md) (is a pipeline needed beyond `IClaimsTransformation`, which `TryAddRoleRightMapping` now uses?) and [SCPI control-module schema](docs/features/request/2026-11-10-devterm-scpi-control-module-schema.md) (do JSON profile panels cover enough?).

**Options**
- A. Close both as covered: the platform primitive and the profile panels (recommended: matches your preference for platform primitives; reopen when a gap shows).
- B. Keep the claims pipeline, close the schema.
- C. Keep both.

**Default:** C (nothing closes).

**Your answer:**

### Q22 (2026-11-19, Imaging, face and pet detection): which detection model and licence?

[Request](docs/features/request/2026-11-11-face-and-pet-matching.md). Clustering is built; detectors and embedders are not, and the catalog's model is the starting point.

**Options**
- A. Use the catalog's models if their licences allow redistribution (you confirm the licence), downloaded through the existing model cache, never bundled (recommended).
- B. Pick a different permissive model (name it).
- C. Keep it detector-free: the library takes embeddings from an adapter you supply.

**Default:** C is built (it needs no model); A waits for your answer.

**Your answer:**

### Q23 (2026-11-19, Query and logging examples): do the audit attribute and the query framework example still have a consumer?

Requests: [audit attribute](docs/features/request/2026-01-20-audit-request-attribute.md) and [query framework example](docs/features/request/2026-01-20-query-framework-example.md), both triaged "no code until a consumer appears". The right audit trail and the metadata query engine now cover parts of both.

**Options**
- A. Decline both as superseded and move them to `rejected/` (recommended: the audit trail records right checks and MetadataQuery replaces hand-written query records).
- B. Keep both waiting for a consumer.
- C. Build the audit attribute for logging middleware now.

**Default:** B.

**Your answer:**

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
