# OoBDev.Data.Common.Abstractions

Contracts for running stored procedures and scalar functions without tying the caller to a database: `IDatabaseQuery<TDbOptions>` for callers and `IDatabaseMapper` for database adapters.

## Contracts

**Table 1 — Contracts**

| Type | Role |
|------|------|
| `IDatabaseQuery<TDbOptions>` | What application code injects: `GetConnection`, `ExecuteStoredProcedureAsync<T, TResult>` (with an optional return-value callback) and `ExecuteFunctionAsync<TResult>`. `TDbOptions` selects the database. The implementation is `DatabaseQuery<>` in `OoBDev.Data.Common`. |
| `IDatabaseMapper` | What a database adapter implements: `GetConnectionString<TDbOptions>`, `GetConnection<TDbOptions>`, `GetCommandTimeout<TDbOptions>` (seconds, `null` for the provider default), `GetStoredProcedureName<T>`, `GetStoredProcedure<T>(connection, query)`, `GetCommandParameters<T>(query)` and `GetReaderMapper<TResult>(reader)`. |

## Writing an adapter

Implement `IDatabaseMapper` for one provider, return a provider connection from `GetConnection<TDbOptions>`, and register it with the framework registration of the adapter project. `OoBDev.Microsoft.SqlServer.Server` (`SqlDatabaseMapper`) is the SQL Server implementation and the reference to copy.

```csharp
services.TryAddDataCommonServices();            // OoBDev.Data.Common: IDatabaseQuery<>
services.TryAddMicrosoftSqlServerExtensions();  // the SQL Server IDatabaseMapper
```
