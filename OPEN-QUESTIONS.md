# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q9 (2026-11-19, Security, auth translation gateway): how is the gateway built, and does it live in dotex?

[Request](docs/features/request/2026-11-10-auth-translation-gateway.md). Services understand only JWT (rule `owner-decisions`); the gateway converts other credentials at the edge. It holds secrets and mints tokens, so the shape matters before any design.

**Options**
- A. Build on YARP (forwarding is done) as a dotex library, inbound schemes in the order API key, client certificate, then RFC 8693 token exchange (recommended: it reuses the proxy that exists and follows your JWT-only rule).
- B. A hand-written `HttpClient` gateway. More control, more code to secure.
- C. A separate application that only references dotex.

**Default:** A for the design document only; no code until a consumer appears.

**Your answer:**

### Q10 (2026-11-19, Consolidation, event, document and history stores): where do they live?

[Request](docs/features/request/2026-11-10-event-document-history-stores.md). Open: separate package or inside `OoBDev.EventSourcing` and the message queue project; a LiteDB or LMDB-style second provider; whether the history store belongs here or with OpenTelemetry export.

**Options**
- A. Separate package family beside `OoBDev.EventSourcing`, history store split out as its own request, no second embedded provider yet (recommended: keeps the event-sourcing packages small and the request reviewable).
- B. Inside `OoBDev.EventSourcing` with the history store included.
- C. Decline; the existing SQLite, SQL Server and PostgreSQL stores are enough.

**Default:** A for the design only.

**Your answer:**

### Q11 (2026-11-19, Consolidation, Postgres support): first-class target, and wait for the schema tool?

[Request](docs/features/request/2026-11-10-postgres-support.md). `OoBDev.EventSourcing.Postgres`, the pgvector store and the query adapter already use PostgreSQL.

**Options**
- A. Close the request as covered by those packages and keep schema tooling in the desired-state request, which waits for its bake-off (Q4) (recommended: nothing separate is left to build).
- B. Keep it open as a first-class target with its own test matrix.
- C. Schema tooling target only.

**Default:** A is proposed; the file stays in `request/` until you answer.

**Your answer:**

### Q12 (2026-11-19, Embeddings, container image): does dotex publish an embedding server image?

[Request](docs/features/request/2026-11-11-embedding-container-and-client.md). The client speaks the OpenAI-compatible route and is built. No server in the stack or the catalog serves images.

**Options**
- A. No image: the client works with any compatible server, the stack's Ollama is the reference, and the image variant stays deferred (recommended: no new thing to maintain; the cosine comparison runs against Ollama).
- B. Publish a small text and image embedding image from this repository (needs a model choice and licence check).
- C. Keep it with the catalog application.

**Default:** A; the request then closes once the Ollama comparison test passes.

**Your answer:**

### Q13 (2026-11-19, Devices, GPIB): which controller first, and is SRQ in scope?

[Request](docs/features/request/2026-11-10-gpib-transport.md). Needs a controller on the bench before a design.

**Options**
- A. Serial-attached USB-GPIB adapter first, polling only (recommended: simplest to test; SRQ and VXI-11 follow).
- B. VXI-11 gateway first (a Framework item the migration lists as open).
- C. Your own adapter (no design exists).
- D. Decline until a controller is attached.

**Default:** none built; stays in the Devices ideas.

**Your answer:**

### Q14 (2026-11-19, Devices, LANC): spec first, and is a recorded exchange enough?

[Request](docs/features/request/2026-11-10-lanc-frame-builder-and-transport.md). The bridge's serial format is not specified, and no camera is attached.

**Options**
- A. Build the frame builder from the published table, accept recorded-exchange tests until a camera is attached, and leave the transport until you provide the bridge format (recommended).
- B. Wait for the bench.
- C. Decline.

**Default:** A is not started until you confirm.

**Your answer:**

### Q15 (2026-11-19, Devices, XBee): package, library and gateway form

[Request](docs/features/request/2026-11-10-xbee-api-and-mqtt-gateway.md).

**Options**
- A. Own package `OoBDev.Digi.XBee`, frames written from the Digi documentation, first pass AT, transmit, receive and I/O sample; the MQTT gateway is a documented sample (recommended: no new third-party dependency, gateway stays optional).
- B. Thin adapter over an existing NuGet XBee library.
- C. Inside `OoBDev.System.IO.Ports`, gateway as a DevTerm plugin.

**Default:** A for a design only; frames are testable without hardware.

**Your answer:**

### Q16 (2026-11-19, Devices, USB passthrough with Ser2NetWin): scope

[Request](docs/features/request/2026-11-10-ser2netwin-usb-device-passthrough.md). Four open points: selection by USB id in the bridge configuration, where the container recipe lives, whether non-serial USB is out of scope, and whether the Greaseweazle case needs a bench pass first.

**Options**
- A. USB-id selection in the configuration, recipe in the Ser2NetWin docs only, non-serial USB out of scope, Greaseweazle claimed only after a bench pass (recommended: smallest honest claim).
- B. As A, plus a helper image in the `containers` repository.
- C. Also a USB/IP bridge.

**Default:** A.

**Your answer:**

### Q17 (2026-11-19, DevTerm, BYTECC bridge and BLE on Linux): is the hardware or machine available?

Requests: [BYTECC](docs/features/request/2026-11-10-devterm-bytecc-usb-bridge.md) ("without the hardware, decline") and [BLE on Linux](docs/features/request/2026-11-10-devterm-ble-linux-macos.md) (macOS deferred, 2026-11-10).

**Options**
- A. Decline BYTECC (move to `rejected/`) and park Linux BLE until a Linux machine with a BLE adapter is available (recommended).
- B. You have both: say which and when.
- C. Keep both open as they are.

**Default:** C; nothing moves.

**Your answer:**

### Q18 (2026-11-19, SharedFramework, complex events and data loader): relation to your own material and to MasterData

Requests: [complex events](docs/features/request/2026-11-10-complex-events-and-scheduling.md) (overlaps your `event-sourcing` and `sync-mesh` material; copying needs your say-so; is `ComplexEvents.DatabaseExtensions` wanted?) and [data loader](docs/features/request/2026-11-10-data-loader-etl.md) (wait for the MasterData design, Epic 5? CLI wanted?).

**Options**
- A. Complex events: design only, separate from event sourcing, no database project; data loader: stand alone, library only, no CLI (recommended: smallest designs; both wait for a consumer to build).
- B. Fold complex events into `OoBDev.EventSourcing` and the data loader into MasterData.
- C. Decline both.

**Default:** A for the design documents.

**Your answer:**

### Q19 (2026-11-19, SharedFramework, remaining entries): which wish-list, roadmap, spatial and test-data lines are still wanted?

Requests: [wish list](docs/features/request/2026-11-10-framework-wish-list.md), [roadmap lines](docs/features/request/2026-11-10-roadmap-open-items.md) (workflow engine, rules engine, DAPR, S3 and other blob stores, SMS, Teams, Slack), [spatial services](docs/features/request/2026-11-10-spatial-services.md) (vendors; Bing Maps is being retired), [test data generation](docs/features/request/2026-11-10-procedural-test-data-generation.md) (own or `Bogus`), [reference designs](docs/features/request/2026-11-10-reference-designs.md) (the patterns-discovery blueprint under another name?).

**Options**
- A. Close the wish list and roadmap as stale except lines you name; spatial waits for a consumer and drops Bing; test data uses the CSV utility project, no `Bogus`; reference designs closes as covered by the blueprint (recommended: most are old and unowned).
- B. Keep every line open.
- C. Answer line by line (say which).

**Default:** B (nothing closes).

**Your answer:**

### Q20 (2026-11-19, Documents and files): scope of file inventory and the HP-GL audio sink

Requests: [file inventory](docs/features/request/2026-11-10-file-inventory-and-deduplication.md) (library only or also a CLI; default hash; share interfaces with the NAS catalog or keep its scanner) and [HP-GL stereo audio sink](docs/features/request/2026-11-10-hpgl-vector-display-sink.md) (own project or an output format of the Hpgl library; sample rate and blanking).

**Options**
- A. Inventory: library plus a tool in `src/Tools`, fast non-cryptographic hash first and SHA-512 to confirm, shared interfaces only; audio sink: an output format of the Hpgl library, 48 kHz, fast jumps without a Z track (recommended).
- B. Inventory library only; audio sink as its own project with an optional Z channel.
- C. Decline both.

**Default:** A for design only; both stay unscheduled.

**Your answer:**

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
