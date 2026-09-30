# Architecture § 10 — Architectural Principles (Distilled from the Code)

[↑ Architecture](./README.md) · [← 9. Build & Packaging Architecture](./09-build-and-packaging.md) · [Index →](./README.md)

1. **Contracts first, then adapters.** A new capability starts as `*.Abstractions`; vendors plug in from outside.
2. **Composition by convention, wiring by extension method.** Globs and suffixes group projects; one `Try…` method per project wires them.
3. **Opt-out, not opt-in, at the top; opt-in at the leaf.** `TryAllCommonExtensions` enables everything; individual adapters self-disable when their config section is absent (e.g. `TryAddOllamaServices` returns early if `Url` is missing).
4. **Configuration is the runtime switchboard.** Provider selection, section names, feature flags all flow from `IConfiguration`.
5. **Every hard-to-test dependency is an interface** (time, GUIDs, temp files, serializers, current user).
6. **Attributes declare intent; infrastructure reads them.** `[IsCacheable]`, `[FlushCache]`, `[MessageQueue]`, `[Searchable]`, `[ApplicationRight]`, `[StoredProcedure]`.
7. **Failure isolation for optional infrastructure.** Caching failures are swallowed (Release) so business calls continue; receivers restart themselves.
8. **Documentation ships with the code.** Per-project `ReadMe.*.md`, packed and copied by the build.

---

[↑ Architecture](./README.md) · [← 9. Build & Packaging Architecture](./09-build-and-packaging.md) · [Index →](./README.md)
