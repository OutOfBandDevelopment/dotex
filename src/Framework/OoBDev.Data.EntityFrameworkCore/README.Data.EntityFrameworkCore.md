# OoBDev.Data.EntityFrameworkCore

The small piece every store shares so several copies of one store can live in one database ([design](../../../docs/design/DesiredStateDatabase/instances.md)).

| Type | Role |
|------|------|
| `StoreInstance.UseStoreInstance(schema, tablePrefix)` | Names the instance on the context options and gives each instance its own cached model. |
| `StoreInstance.Of(context)` | The instance in `OnModelCreating`; `StoreInstanceOptions.Default` when none was set. |
| `StoreInstanceOptions.ToTable(entity, name)` | Places an entity: `TablePrefix + name` in `Schema`. |

```csharp
// in the store's DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder) =>
    StoreInstance.Of(this).ToTable(modelBuilder.Entity<Row>(), "Rows");

// in the composition root
services.AddDbContext<MyStoreContext>(o => o.UseNpgsql(cs).UseStoreInstance(schema: "billing"));
```

Use a schema on PostgreSQL and SQL Server and a prefix on SQLite. The desired-state tooling scopes each plan to the instance (`DatabaseTarget.ForInstance`). References EF Core Relational only.

Tests: `OoBDev.Data.EntityFrameworkCore.Tests` (Unit).
