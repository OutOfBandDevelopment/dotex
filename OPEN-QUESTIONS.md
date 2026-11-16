# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q-OFFICE-MARKDOWN — 2026-11-16 — DocumentToMarkdown (layering)

Office files (docx, odt, rtf, epub, doc) to Markdown need a reader behind `IFileMarkdownReader` (built 2026-11-16, HTML first). The only office converter in the repo is Apache Tika (a server, async HTTP, output HTML), and HTML to Markdown is `OoBDev.MysticMind`. Adapters reference Abstractions only, so neither can call the other.

**Options**
- A (recommended): a small Extensions or Framework composition project `OoBDev.Documents.Markdown.Office` that implements `IFileMarkdownReader` by running the registered `IDocumentConversionHandler` pair (Tika to HTML, then MysticMind HTML to Markdown) through the existing conversion pipeline. No new third-party package, Tika stays optional, needs a Tika container (already in the stack). Reader runs async work synchronously (the interface is sync).
- B: a new `IFileMarkdownReader` in the Tika adapter that asks Tika for plain text only. Simplest, loses headings, lists and tables.
- C: add a direct OpenXml/NPOI package reader (no server). Best fidelity offline, new dependency to approve (rule `coding-standards`), more code per format.

**Default:** A once confirmed; nothing built meanwhile (the item is parked).
**Your answer:**

### Q-ZWAVE-SESSION — 2026-11-16 — DevTerm Z-Wave panel (layering)

The Z-Wave Enumerate and Describe node buttons need the decoder's frames and the surface's requests to share one `ZWaveNodeTable` per session (your 2026-11-12 choice). The hosts build them apart: `CreateSurface(DeviceSend)` runs when a panel opens (WPF, Tui, Web each), while `CreateDecoder()` runs from DI as a transient presenter (`DeviceDecoderPresenter`). Neither call knows the session, so the panel cannot pair them.

**Options**
- A (recommended): `IDevicePanel` gets `CreateSurface(DeviceSend send, IDeviceDecoder decoder)` so a host passes the decoder it already holds for that session; the Z-Wave surface casts it to read the shared table. One interface change, all panels updated, hosts pass what they have.
- B: `ZWavePanel` keeps one table for all sessions. No interface change, but two sticks would mix nodes.
- C: the surface opens its own `ZWaveSession` on the transport. Duplicates the reader and fights the host for the stream.

**Default:** A once confirmed; the slice is parked (BACKLOG line says so). The panel-lifetime `ZWaveAckResponder` waits on the same decision.
**Your answer:**

### Q-SCANNER — 2026-11-16 — Code quality scanner

Request [code-quality-scanner](docs/features/request/2026-11-10-code-quality-scanner.md): the earlier SonarQube is gone; the analyzers in `src/.editorconfig` (Meziantou, NetAnalyzers, `OoBDev.Analyzers`) give build-time findings with no history. Standing up a server scanner adds a container and a tool the repository does not have.

**Options**
- A (recommended): keep the build analyzers as the gate and add SonarAnalyzer.CSharp to them (a NuGet analyzer, no server, no secrets); history comes from `git log` and the committed coverage reports. Smallest change, nothing to run.
- B: SonarQube Community in the integration stack (about 2 GB memory, PostgreSQL, the `dotnet-sonarscanner` dotnet tool, a token in a user secret), `scripts/ci/sonar.ps1` for the end-to-end scan and a switchable gate. Gives the dashboard and trends the request asks for.
- C: Semgrep or Qodana as a container run on demand: security and bug rules without a long-lived server, no trend history.

**Default:** A once confirmed; nothing built meanwhile.
**Your answer:**

### Q-VECTOR-DELETE — 2026-11-16 — Re-embedding stale vectors (interface)

`EmbeddingVersion.StaleIds` finds rows made by an old model, but `IVectorStore` can only store, list and search: no delete or upsert by id. A re-embed command would add new rows beside the stale ones (duplicates in every search) until the store can replace a row. Changing `IVectorStore` touches every provider (in-memory, SQL Server, Qdrant, OpenSearch).

**Options**
- A (recommended): add `DeleteAsync(IEnumerable<string> ids)` to `IVectorStore` and implement it in each provider; the re-embed helper stores the new row then deletes the stale id. Small, additive, and the framework is unreleased so the break is allowed (breaking changes OK).
- B: add `UpsertAsync(id, embedding, metadata)`. Cleaner for callers, but providers differ in id handling (SQL generates ids), so more work per provider.
- C: no interface change; re-embed means rebuilding the whole collection from source. Nothing to implement, but slow and needs the source texts.

**Default:** A once confirmed; nothing built meanwhile (the stale-row detector already works).
**Your answer:**

### Q-LLM-USAGE — 2026-11-16 — Token and cost accounting for LLM calls (interface, layering)

`ILanguageModelProvider` returns plain strings, so callers never see the token counts that Ollama, OpenAI, Groq and Azure report. No Framework project implements the interface (only adapters do), so a shared meter has no home yet.

**Options**
- A (recommended): add a usage result to the interface (`LanguageModelResponse` with text plus input and output token counts) and have each adapter fill it; a `System.Diagnostics.Metrics` counter pair (`oobdev.llm.tokens`) is then emitted once, in a Framework decorator that OpenTelemetry already exports. Exact numbers; breaking change to the interface and adapters, allowed while unreleased.
- B: a decorator that estimates tokens (characters divided by four) with no interface change. Cheap, works for every adapter, but the numbers are approximate and a cost figure would mislead.
- C: leave accounting to each provider dashboard. No code, no cross-provider view.

**Default:** A once confirmed; nothing built meanwhile.
**Your answer:**

### Q-GDPR-ERASURE — 2026-11-16 — Erasing personal data from append-only event stores

`OoBDev.EventSourcing` stores events append-only, so a "delete my data" request cannot delete rows without breaking replay and the sequence check. The way personal data is stored decides this and changes the event data model.

**Options**
- A (recommended): crypto-shredding. Personal fields are encrypted with a per-subject key held in a separate key store; erasure deletes the key, the events stay and replay shows the field as erased. Keeps history and sequence intact; needs a key store, a marker on personal fields and a decision on which fields count. Fits the owner's no-live-data and security preferences.
- B: keep personal data out of events (events hold an id; the data lives in a normal table that supports delete). Simplest and standard, but every existing event type must be reviewed, and events that already carry personal data need a migration.
- C: rewrite history (a tombstone event plus a compaction job that blanks the fields). No key store, but it breaks the append-only guarantee and any hash chain or external copy.

**Default:** none built; A is designed when you confirm (the event store stays unchanged meanwhile).
**Your answer:**
