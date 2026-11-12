# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open



### Q-CLOUDEVENTS-EDGE — 2026-11-12 — CqrsEventSourcing (layering)

Where does the CloudEvents `POST /events` edge live? Plan: [cloudevents-edge-plan.md](docs/design/CqrsEventSourcing/cloudevents-edge-plan.md).

**Options**
- A (recommended): new `OoBDev.EventSourcing.Web` in `src/Extensions`, beside the other ASP.NET libraries; keeps Framework host-free.
- B: new project in `src/Framework` with an ASP.NET `FrameworkReference`; breaks the Framework-has-no-host rule.
- C: add the endpoint to an existing web library; mixes unrelated concerns.

**Default:** A (building nothing until confirmed).

**Your answer:**


### Q-ZWAVE-ENUMERATE — 2026-11-12 — Z-Wave panel (contract)

Enumerate and Describe node must read replies (the init-data node list, then each node's answers), but `IControlSurface` only sends and `IDeviceDecoder` only decodes, and the host creates them separately. How does a command see replies?

**Options**
- A (recommended): the panel shares one per-session `ZWaveNodeTable` between `CreateSurface` and `CreateDecoder` (the host calls both for one session); Enumerate waits on the table with a timeout. No contract change, but the panel instance becomes stateful.
- B: extend `IDevicePanel` with a session-scoped `CreateSession(DeviceSend)` returning the surface and decoder together; cleaner, changes the shared UiDefinitions contract and the three front ends.
- C: Enumerate sends GetNodeProtocolInfo for a typed node range (default 1-10) without reading replies; no state, but the user must know the range.

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-BASICSTAMP-FORMAT — 2026-11-12 — Basic Stamp tokenizer (scope and source)

The request asks for a Basic Stamp tokenizer and detokenizer but the repo holds no format reference, and Parallax does not publish the BS2 EEPROM token format (it is reverse-engineered in third-party tools such as the open-source `pbasic` and `stampy` projects). Which generation, and where does the format come from?

**Options**
- A (recommended): BS1 only first, from the published BS1 token tables in the Parallax manual; small, documented, testable with hand-built bytes. BS2 waits for a reference.
- B: BS2 family from a reference project you point to (add it to `docs/harvest-candidates.md`); larger, format is undocumented and needs real EEPROM dumps to verify.
- C: drop the item (no consumer, no hardware on the bench).

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**


### Q-MIDI-PANEL — 2026-11-12 — MIDI device panel for DevTerm (layering)

`OoBDev.Midi` (Extensions) is pure and references nothing. A DevTerm device panel (`IDevicePanel`, decoder, control surface, UI definition) needs `OoBDev.System.Abstractions` (`UiDefinitions`), as the ZWave and device projects do. MIDI runs on a plain serial line at 31250 baud (or USB-MIDI), so no new transport is needed. Where does the panel live?

**Options**
- A (recommended): a `Panels` folder in `OoBDev.Midi` with a reference to `OoBDev.System.Abstractions`, like `OoBDev.ZWave`; one project, same pattern as the other panels; the pure parser now carries a UiDefinitions reference.
- B: a separate `OoBDev.Midi.Panels` project; keeps `OoBDev.Midi` reference-free, one more project and package.
- C: no MIDI panel until there is a MIDI device on the bench (the request lists no consumer).

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-MEDIAMETADATA-HOME — 2026-11-12 — EXIF reader (layering)

The storage catalog has a small first-party EXIF reader and tag dump (about 640 lines, BCL only) that the request wants in the framework ([design](docs/design/MediaMetadata/README.md), [request](docs/features/request/2026-11-11-exif-metadata-extraction.md)). No third-party library is needed. Where does it live?

**Options**
- A (recommended): a new Framework project `OoBDev.Media.Metadata` (BCL only) with its own tests project; generic name, usable by any host; matches the rule "reusable means Framework" and how `OoBDev.Documents.Guides` was placed.
- B: inside `OoBDev.Extensions.Drawing` (Extensions layer) beside `JpegSegmentReader`; no new project, but that project is Windows-drawing flavoured and a catalog worker would reference it for byte parsing.
- C: leave it in the catalog application (no framework version).

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-IMAGEANALYSIS-HOME — 2026-11-12 — Image colour analysis (layering)

Histograms, colour facets and exposure metrics are pure functions over an RGB pixel buffer ([design](docs/design/ImageAnalysis/README.md), [request](docs/features/request/2026-11-11-image-color-analysis-and-histograms.md)). No library for the analysis itself is needed. The burst ranking and dHash code from the catalog ([design](docs/design/BurstRanking/README.md)) would live in the same project. Where does it live, and which pixel type does it take?

**Options**
- A (recommended): a new Framework project `OoBDev.Imaging.Analysis` (BCL plus `OoBDev.System` for `ColorConversion`) with its own small `RgbImage` view; decoders (Skia, ONNX `DecodedImage`) stay in their adapters and convert; generic name, no image-library dependency.
- B: add the analysis to `OoBDev.System` next to `ColorConversion` (`Graphics` folder); no new project, but that project is already very large.
- C: put it in `OoBDev.Onnx.ImageEmbeddings.Skia` and reuse `SkiaImageDecoder`; fewer types, but analysis then needs Skia and ONNX packages and is not reusable without them.

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-DOCMARKDOWN-HOME — 2026-11-12 — Document to Markdown and OCR (layering, dependency)

The catalog converts documents to page-anchored Markdown and calls a Docling OCR service ([design](docs/design/DocumentToMarkdown/README.md), [request](docs/features/request/2026-11-11-ocr-document-text-extraction.md)). The catalog reads PDF text with the third-party PdfPig package (a dependency choice is yours). Where does the code live?

**Options**
- A (recommended): a new Framework project `OoBDev.Documents.Markdown` (no PDF package; PDF text comes from an injected `IPdfTextReader`) plus a Docling adapter `OoBDev.Docling` under ExternalServices; PdfPig, if wanted, goes into a third small adapter you approve separately.
- B: one Framework project that references PdfPig directly; fewer projects, but adds a third-party dependency to Framework.
- C: leave it in the catalog (no framework version).

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-RAW-PROVIDER — 2026-11-12 — RAW development provider (dependency)

True RAW development needs a native decoder ([design](docs/design/RawDevelopment/README.md), [request](docs/features/request/2026-11-11-raw-camera-file-development.md)). Dependency choices are yours. Which provider, if any?

**Options**
- A (recommended): build the managed part now (detection, embedded-preview extraction, white-balance scaling) and add a LibRaw adapter project later once you approve a wrapper package; no native code in Framework.
- B: ImageMagick (Magick.NET) as the provider; one package covers many formats, large native payload and a heavier licence review.
- C: managed only, no true development (previews only).

**Default:** A for the managed part once Q-MEDIAMETADATA-HOME is answered; no native package is added meanwhile (the item is parked).

**Your answer:**

### Q-FACEMATCH-SCOPE — 2026-11-12 — Face and pet matching (personal data, dependency)

The request ([face and pet matching](docs/features/request/2026-11-11-face-and-pet-matching.md)) detects faces in private photos, stores embeddings and names people. Face embeddings are biometric personal data, and a detection model carries a licence. Should the framework take this on?

**Options**
- A (recommended): not now. Keep it out of the framework until a consumer needs it; the catalog keeps its own copy. Burst ranking already works without face signals.
- B: build detection and embedding behind an interface with no bundled model, storage opt-in, embeddings never logged, delete-by-person supported; you choose the model and licence later.
- C: pets and objects only (no human faces), using a general image embedder that already exists in the framework.

**Default:** A; nothing built (the item is parked).

**Your answer:**

### Q-QUERYENGINE-HOME — 2026-11-12 — Metadata-driven query engine (layering)

The catalog's engine declares queryable kinds by attributes and answers every read screen with one request ([design](docs/design/MetadataQuery/README.md), [request](docs/features/request/2026-11-11-metadata-driven-query-engine.md)). Its attributes hold SQL fragments written by the application author; only request values are bound. Where does it live, and is that acceptable?

**Options**
- A (recommended): a Framework project `OoBDev.Query.Metadata` (model, attributes, registry, parser, compiler; no database reference) plus an adapter `OoBDev.Query.Postgres` (dialect and runner; Npgsql only there); one dialect first. Answers the request's question "model plus a dialect interface".
- B: one project that references Npgsql; simpler, but Framework then depends on a database driver.
- C: attributes without SQL (a declarative expression language compiled per dialect); safer but a much larger design and a second language to maintain.
- D: leave it in the catalog.

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-WORKQUEUE-HOME — 2026-11-12 — Database work queue (layering)

The catalog's PostgreSQL work queue with leases, fenced completion and dead letters ([design](docs/design/WorkQueue/README.md), [request](docs/features/request/2026-11-11-database-stage-work-queue.md)) is the answer to the Postgres request for a queue. Where does it live?

**Options**
- A (recommended): a Framework project `OoBDev.WorkQueue` (contracts, SLA, hosting; no database reference) plus an adapter `OoBDev.WorkQueue.Postgres` (Npgsql only there) and a PostgreSQL service added to the integration stack for its tests.
- B: one project referencing Npgsql; fewer projects, Framework depends on a driver.
- C: extend `OoBDev.MessageQueueing` with a row-level mode; one family, but it blurs a broker abstraction with a task table.
- D: leave it in the catalog.

**Default:** A once confirmed; nothing built meanwhile (the item is parked).

**Your answer:**

### Q-PGVECTOR-HOME — 2026-11-12 — HTTP embedding client and pgvector store (layering, dependency)

The design ([EmbeddingClientAndPgVector](docs/design/EmbeddingClientAndPgVector/README.md)) adds an OpenAI-compatible embedding client and a PostgreSQL pgvector store. The pgvector store needs Npgsql (and possibly the Pgvector package), which are third-party dependency choices. Where do they live, and may they be added?

**Options**
- A (recommended): two new ExternalServices projects, `OoBDev.Embeddings.Http` (no new package) and `OoBDev.Data.Vectors.Postgres` (Npgsql, Pgvector only there), plus a pgvector PostgreSQL service in the test stack.
- B: HTTP client only now; defer the pgvector store until a consumer needs it.
- C: leave both in the catalog.

**Default:** B for the HTTP client is safe, but nothing is built while this is open (the item is parked).

**Your answer:**
