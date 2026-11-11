# OoBDev.Microsoft.SqlServer.Server

The SQL Server `IDatabaseMapper` (`SqlDatabaseMapper`): it turns an attributed query class into a stored procedure call and maps the result rows to objects. `IDatabaseQuery<>` in [OoBDev.Data.Common](../../../Framework/OoBDev.Data.Common/README.Data.Common.md) runs it.

## Registration

```csharp
services.TryAddDataCommonServices();
services.TryAddMicrosoftSqlServerExtensions();
```

The mapper is registered as `IDatabaseMapper` and as a keyed service under `SqlServerGlobals.ProviderKey` (`"mssql"`). It needs `IConfiguration` and an `IJsonSerializer`.

## Describing a query

The attributes are in `OoBDev.System.ComponentModel.Data`.

```csharp
[ConnectionStringName("MyDatabase")]
[StoredProcedure("dbo.GetUsers")]
public class GetUsersQuery
{
    [QueryParameter(Name = "userId")]
    public int UserId { get; set; }

    [QueryParameter(IsJson = true)]
    public Filter? Filter { get; set; }
}

public class UserRow
{
    [QueryResult(Name = "user_name")] public string? Name { get; set; }
    public int Id { get; set; }
}

await foreach (var row in databaseQuery.ExecuteStoredProcedureAsync<GetUsersQuery, UserRow>(query, ct)) { }
```

**Table 1 — What the mapper does**

| Step | Behaviour |
|------|-----------|
| Connection string | `ConnectionStrings:<name>` from the `ConnectionStringName` attribute; a missing attribute throws `NotSupportedException`, a missing string `InvalidOperationException`. |
| Command timeout | `CommandTimeouts:<name>` in seconds; unset means the provider default. |
| Procedure | The `StoredProcedure` attribute, run as a stored procedure command. |
| Parameters | Every property with `QueryParameter` becomes `@name` (the property name when `Name` is not set, a leading `@` is optional); `null` becomes `DBNull`; arrays and properties with `IsJson = true` are sent as JSON text. |
| Result rows | A compiled expression builds `TResult` (needs a default constructor) and fills each property from the column with the same name, or the `QueryResult` name or `Position`; a value of another type becomes the default. |

`MessageQueueing/SqlServiceBrokerQueueMessageProvider` is a placeholder for a Service Broker sender and receiver: every member throws `NotSupportedException`. The Service Broker queues of the vector database are used through the Hosting project instead (see `OoBDev.Data.Vectors.Hosting`).
