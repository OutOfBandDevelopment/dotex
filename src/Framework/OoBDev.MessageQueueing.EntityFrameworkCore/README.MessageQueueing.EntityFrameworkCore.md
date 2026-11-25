# OoBDev.MessageQueueing.EntityFrameworkCore

Relational `IInboxStore` and `IOutboxStore` (design in `docs/design/FeatureLandscape/messaging-and-documents.md`). The consumer chooses the database provider (PostgreSQL, SQL Server, SQLite) in the configure callback; this package references only Entity Framework Core Relational.

- `MessagingDbContext`: tables `Inbox` (key `(Consumer, MessageId)`) and `Outbox` (identity `Position`, unique `Id`).
- `EfInboxStore`: `TryBeginAsync` is an insert; the primary key makes the first caller the winner across processes, a duplicate returns `false`. `ReleaseAsync` deletes the row so a redelivery runs again.
- `EfOutboxStore`: a message is stored as its registered type name and System.Text.Json text, read back in insert order, and deleted when sent. Only types added to `OutboxMessageTypes` can be stored or rebuilt, so an edited row cannot make the dispatcher load an arbitrary type; an unregistered name throws.
- `TryAddEntityFrameworkMessaging(configure, messageTypes)`: registers the context factory, the type list and both stores. Call it before `TryAddMessageQueueHosting`, whose in-memory outbox is a `TryAdd`.

```csharp
services.TryAddEntityFrameworkMessaging(
    (sp, options) => options.UseNpgsql(connectionString),
    types => types.Add<OrderPlaced>());
```

The schema is created by the consumer (`EnsureCreated` or a migration); no migrations ship yet. Delivery is at least once, so receivers stay idempotent (the inbox).

## Write in the caller's transaction

Call `modelBuilder.AddOutbox()` in the business context's `OnModelCreating` (same `Outbox` table and database as `MessagingDbContext`), then stage the message in the same unit of work:

```csharp
db.Orders.Add(order);
db.StageOutbox(types, new OrderPlaced(order.Id));   // types is the registered OutboxMessageTypes
await db.SaveChangesAsync();                        // order and message commit or roll back together
```

The dispatcher's `EfOutboxStore` sends the row afterwards. Both contexts must point at one database, and the table is created by the business context's migration or `EnsureCreated`.
