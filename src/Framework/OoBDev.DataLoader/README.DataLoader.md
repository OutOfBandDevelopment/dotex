# OoBDev.DataLoader

File loader: rows from a CSV or JSON source go through ordered pipeline handlers (validate, map, look up) into an `IRowSink`, and the result is a `LoadReport` (read, written, replaced, skipped, errors with source line). A bad row never throws; `LoadOptions.StopOnError` stops at the first one. `DuplicatePolicy` (`Skip`, `Replace`, `Fail`) is applied by the sink.

```csharp
var loader = new Loader([new CsvRowReader(), new JsonRowReader()], handlers, new InMemoryRowSink("id"));
var report = await loader.LoadAsync(stream, "csv", new LoadOptions { Duplicates = DuplicatePolicy.Replace }, ct);
```

CSV supports a header row and quoted fields (commas, doubled quotes, line breaks); JSON is an array of flat objects. Handlers run in ascending `Priority`. Not built yet: the EF Core sink (`OoBDev.DataLoader.EntityFrameworkCore`), batched transactions, and DI registration. Design: [DataLoader](../../../docs/design/DataLoader/README.md).
