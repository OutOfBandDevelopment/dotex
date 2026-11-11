# OoBDev.MongoDB

MongoDB access through typed database interfaces: you declare an interface whose properties are `IMongoCollection<T>`, and the library creates the database object behind it from configuration.

## Use

```csharp
services.TryAddMongoServices(configuration);                    // section MongoDatabaseOptions
services.TryAddMongoDatabase<IOrdersDatabase>();                // uses MongoDatabaseOptions
services.TryAddMongoDatabase<IOrdersDatabase, MySettings>();    // or your own IMongoSettings type
```

```csharp
public interface IOrdersDatabase
{
    IMongoCollection<OrderModel> Orders { get; }

    [Table("order_lines")]                 // optional collection name
    IMongoCollection<OrderLineModel> Lines { get; }
}
```

```json
{ "MongoDatabaseOptions": { "ConnectionString": "mongodb://localhost:27017", "DatabaseName": "orders" } }
```

In `DEBUG` builds the section name of `TryAddMongoServices` is a required argument (rule `coding-standards`). `MongoDatabaseOptions` (`ConnectionString` and `DatabaseName`, both required and validated) implements `IMongoSettings`; a custom settings type must be bound as `IOptions<TSettings>` by the caller.

## Behaviour

**Table 1 — Components (namespace `OoBDev.MongoDB.Extensions`)**

| Type | Role |
|------|------|
| `IMongoDatabaseFactory` / `MongoDatabaseFactory` (singleton) | `Create<TSettings>()` returns an `IMongoDatabase` for the settings; `Create<TDatabase, TSettings>()` returns the typed interface behind a dispatch proxy. A new `MongoClient` is created per call; the transient registration of each typed database calls it on every resolve. |
| `MongoDispatchProxy` | Implements the interface at run time. Each property or method returning `IMongoCollection<T>` resolves the collection named by `[Table]` on the property, else on the entity type, else the property name; collections are cached per member. |
| `BsonObjectIdConvention` | A member named `_id`, `<Type>Id` (the type name without a `Model` or `Collection` suffix) or marked `[Key]` becomes the BSON id, stored as an `ObjectId`. |
| `IMongoDatabaseRegistration` | Internal list of the registered typed database interfaces. |

The first use of `MongoDatabaseFactory` registers, process-wide, a `DateTimeOffset` serializer (BSON date time) and a convention pack with camel-case element names plus the id convention for all types.

`IMongoVectorSearch` and `IAtlasVectorSearch` are interfaces for Atlas vector search; nothing in the repository implements them yet.

## Tests

`OoBDev.MongoDB.Tests` holds six `Integration` tests against the MongoDB container of the test stack (`MONGODB_*` parameters in `.runsettings`).
