# OoBDev.Data.Migrations.Sqlite

EF Core migration sets for the OoBDev stores on Sqlite. One migrations project per database, one folder and one history table per context ([plan](../../../docs/design/CqrsEventSourcing/migrations-plan.md)).

**Table 1 — Contexts in this package**

| Context | Folder | History table |
|---------|--------|---------------|
| `RightAuditDbContext` (`OoBDev.AspNetCore.EntityFrameworkCore`) | `Migrations/RightAudit` | `__EFMigrationsHistory_RightAudit` |

## Use

```csharp
services.TryAddEntityFrameworkRightAudit(configuration,
    (_, options) => options.UseRightAuditMigrations(connectionString));

await using var db = await factory.CreateDbContextAsync();
await db.Database.MigrateAsync();
```

`UseRightAuditMigrations` sets the migrations assembly and the history table. Add a migration after a model change from this folder:

```
dotnet ef migrations add <Name> -o Migrations/RightAudit --context RightAuditDbContext
```

## Tests

`OoBDev.Data.Migrations.Tests`: the snapshot matches the model (Unit), the set applies to an empty database (Sqlite in memory, Simulate; Postgres in the `oobd-test-postgres` container, Integration).
