# OoBDev.DataLoader.EntityFrameworkCore

Database `IRowSink` for [OoBDev.DataLoader](../OoBDev.DataLoader/README.DataLoader.md). The consumer picks the EF Core provider.

```csharp
services.TryAddDataLoader();
services.TryAddEntityFrameworkRowSink("people", "id", (sp, o) => o.UseSqlServer(connectionString));
```

- `EfRowSink`: writes a row by dataset and key column and applies the duplicate policy (skip, replace, fail); `GetAsync` reads a row back.
- `LoadedRow` / `LoadedRowsDbContext`: table `LoadedRows`, key `(Dataset, Key)`, the columns kept as a JSON object, so one table serves any shape. A typed table per dataset is a consumer sink.
- Tests run on in-memory SQLite (`Simulate`); SQL Server is not exercised yet.
