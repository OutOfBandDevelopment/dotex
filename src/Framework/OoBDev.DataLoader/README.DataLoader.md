# OoBDev.DataLoader

File loader: rows from a CSV or JSON source go through ordered pipeline handlers (validate, map, look up) into an `IRowSink`, and the result is a `LoadReport` (read, written, replaced, skipped, errors with source line). A bad row never throws; `LoadOptions.StopOnError` stops at the first one. `DuplicatePolicy` (`Skip`, `Replace`, `Fail`) is applied by the sink.

```csharp
var loader = new Loader([new CsvRowReader(), new JsonRowReader()], handlers, new InMemoryRowSink("id"));
var report = await loader.LoadAsync(stream, "csv", new LoadOptions { Duplicates = DuplicatePolicy.Replace }, ct);
```

CSV supports a header row and quoted fields (commas, doubled quotes, line breaks); JSON is an array of flat objects. Handlers run in ascending `Priority`. `services.TryAddDataLoader()` registers both readers and `ILoader` (register an `IRowSink` and handlers yourself). `LoadOptions.BatchSize` above 1 writes through an `IBatchRowSink` (the EF Core sink in `OoBDev.DataLoader.EntityFrameworkCore` implements it: one context and one save per batch). Design: [DataLoader](../../../docs/design/DataLoader/README.md).

Lookups: `LookupHandler(priority, ILookupSource, keyColumn, targetColumn, required)` resolves a column through `DictionaryLookupSource` (table, optional comparer) or `DelegateLookupSource` (a query supplied by the host) and rejects an empty or unknown key when required.

Files named in a row: `SourceFileResolver(folder)` resolves a path against the folder of the data file and refuses one that leaves it. `TextFileHandler(priority, files, columns, required)` replaces a file-name column with the file's text (the source `GetTextData`). `DocumentUploadHandler(priority, files, IDocumentStore, options)` uploads the named file, sets `StorageKey` (and `Name` when empty) and skips a row that already has a key (the source `DocumentUpload`); a refused document rejects the row. Column names are in `DocumentUploadOptions`. The project references `OoBDev.Documents.Abstractions` for the store contract.
