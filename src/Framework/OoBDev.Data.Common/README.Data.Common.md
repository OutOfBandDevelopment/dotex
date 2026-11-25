# OoBDev.Data.Common

Executes stored procedures and scalar functions through an `IDatabaseMapper`, with logging of every command. The mapper decides the database; this library only runs the command.

## Registration

```csharp
services.TryAddDataCommonServices();
```

This registers the open generic `IDatabaseQuery<>` as `DatabaseQuery<>` (transient). An `IDatabaseMapper` must be registered by a database adapter; `OoBDev.Microsoft.SqlServer.Server` does it for SQL Server (`services.TryAddMicrosoftSqlServerExtensions()`). The type argument is an options type that tells the mapper which connection string and timeout to use.

## Using a query

```csharp
public sealed class VectorDb;   // marker the mapper resolves a connection string for

public sealed class Worker(IDatabaseQuery<VectorDb> db)
{
    public async Task RunAsync(SearchQuery query, CancellationToken ct)
    {
        await foreach (var row in db.ExecuteStoredProcedureAsync<SearchQuery, SearchRow>(query, ct))
        {
            // each row is mapped by the mapper's reader mapper
        }

        var count = await db.ExecuteFunctionAsync<int>("dbo.CountRows", "documents");
    }
}
```

**Table 1 — `IDatabaseQuery<TDbOptions>` members**

| Member | Behaviour |
|--------|-----------|
| `GetConnection()` | A new connection from the mapper for `TDbOptions`; the caller disposes it. |
| `ExecuteStoredProcedureAsync<T, TResult>(query, ct)` | Builds the command from the query object (`GetStoredProcedure`), applies the mapper's timeout, opens the connection, and yields one mapped `TResult` per row. The connection and reader are released when the enumeration ends. |
| `ExecuteStoredProcedureAsync<T, TResult>(query, callback, ct)` | The same, and after the last row calls `callback` with the procedure return value (`@RETURN_VALUE`) when it is an `int`. |
| `ExecuteFunctionAsync<TResult>(function, arguments)` | Runs `SELECT function(@p0, @p1, ...)` with the arguments as parameters (`null` becomes `DBNull`) and returns the scalar if it is a `TResult`, otherwise `default`. The function name is placed in the text, so it must come from code, never from user input. |

Enumeration stops at the first row read after cancellation without throwing. Commands are logged at Information (type and text) and, at Debug, with the parameter values.

The mapper contract (`IDatabaseMapper`) is in the [abstractions readme](../OoBDev.Data.Common.Abstractions/README.Data.Common.Abstractions.md).
