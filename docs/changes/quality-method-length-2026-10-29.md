# Method length rule MA0051 promoted (2026-10-29)

`MA0051` (method longer than 60 lines) is now a warning in `src/.editorconfig`. The 26 long methods were split into private helpers with the same behavior; a clean `OoBDev.slnx` build reports no `MA0051`.

Split: the example API `Main`, `CgiProcessRunner`, `ReflectionExtensions` (two), `HealthCheckOptionsFactory`, `CachedProxy`, `DacPacValidator`, `DocumentConversion`, `MessageReceiverProviderFactory`, `SegmentPipeFactory`, `ExpressionTreeBuilder` (three methods), `SortBuilder`, `LorenzMachine`, `PlayFair`, ISO 9660 `DirectoryRecord` and `VolumeDescription`, `PathNavigatorFactory`, `ObjectXmlExtensions`, `ExtensibleElementNode`, `XsltTransformer` (two), `FileRagEngineService`, `FixSourceLinks` and `ResponseFileExtractor`, `EmailMessageReceiverHost`, and `EmbeddingSentenceTransformerQueueReader`.

Kept as written: `Archives/Tar/Entry.Sample`, an uncalled migrated sample, carries a `#pragma warning disable MA0051` with the reason.

Verified: Unit and Simulate tests of the whole solution pass (0 failed). Not covered by an automated test: `EmbeddingSentenceTransformerQueueReader` (Service Broker; no test project exercises it) and `EmailMessageReceiverHost` (in development, throws in release builds), so those two were checked by build only.
