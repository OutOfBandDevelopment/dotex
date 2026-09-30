# Pattern 17 — Attribute + mapper data access

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 16 — Injectable non-determinism](./16-injectable-non-determinism.md) · [Pattern 18 — Replace-to-override in host layers →](./18-replace-to-override-in-host-layers.md)
<!-- nav -->

`OoBDev.Data.Common`: `IDatabaseQuery<TDbOptions>.ExecuteStoredProcedureAsync<TQuery,TResult>(query)` returns `IAsyncEnumerable<TResult>`. The **type parameter `TDbOptions` selects the connection** (via `IDatabaseMapper.GetConnection<TDbOptions>()`), attributes on the query type (`[StoredProcedure]`, `[QueryParameter]`, `[QueryResult]`, `[ConnectionStringName]`) define the command, and a reader-to-object mapper is created lazily from the first row. Connection/command timeouts come from the mapper (config).

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 16 — Injectable non-determinism](./16-injectable-non-determinism.md) · [Pattern 18 — Replace-to-override in host layers →](./18-replace-to-override-in-host-layers.md)
<!-- nav -->
