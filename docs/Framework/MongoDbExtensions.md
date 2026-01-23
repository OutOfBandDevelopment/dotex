# OoBDev - MongoDbExtensions

See [back](MajorFunctionality.md)

## Summary

The MongoDbExtensions from OoBDev are intended to simplify configuration and instantiation of Mongo Database collections for use with .NET 10.0+.

This provides a common means to build, configure and use collections by conventions to reduce complexity for developers.

Define a common means to support mongo collections from within .Net applications. Also provides
serialization and query extensions. `OoBDev.MongoDB.Extensions`

* Add functionality to make compatible with Entity Framework

## Usage

### Declare and Register Collections 

Using the `OoBDev.MongoDB.Extensions` just requires creating and interface with getter only properties per collection you which to register.  

#### Example

```csharp

//collection definition
public class UserCollection
{
    [Key] //you may either use the BsonId and Bson
    public string? UserId { get; set; }
    public string? EmailAddress { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool Active { get; set; }
    public List<UserModuleCollection>? UserModules { get; set; }
    public DateTimeOffset? CreatedOn { get; set; }
}

// database definition
public interface ICoreMongoDatabase
{
    //this attribute may be used to explicitly declare the name for the related collection in MongoDB
    [CollectionName("users")] 
    IMongoCollection<UserCollection> Users { get; }
    
    // if the collection name is not provided the configure Json Property Naming Policy will be used.  
    // Default is camel case.  In this example it would be "persons"
    IMongoCollection<PersonCollection> Persons { get; }
}

//registration
//In your IOC registration method use the `TryAddMongoDatabase<>` extension method from the `OoBDev.MongoDb.Extensions` namespace.
//this will create a proxy class in your IoC container to access your mongodb

using OoBDev.MongoDB.Extensions;

namespace Example.Core.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCorePersistenceServices(this IServiceCollection services)
    {
        services.TryAddMongoDatabase<ICoreMongoDatabase>();
        return services;
    }
}
```

## Integration Testing

MongoDB integration tests should follow the patterns established in the [Testing Guidelines](../architecture/testing/testing-guidelines.md).

### Test Configuration

Use `TestContext.GetRequiredProperty<T>()` for required configuration values:

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task MongoDB_InsertDocument_Succeeds()
{
    // Required values - must be configured in .runsettings
    var connectionString = TestContext.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");

    // Create unique database per test for isolation
    var databaseName = $"IntegrationTest_{Guid.NewGuid():N}";
    _databaseName = databaseName; // Store for cleanup

    // ... test implementation
}

[TestCleanup]
public async Task TestCleanup()
{
    if (_mongoClient != null && _databaseName != null)
    {
        await _mongoClient.DropDatabaseAsync(_databaseName);
    }
}
```

### Key Testing Patterns

1. **Unique database names**: Use `$"IntegrationTest_{Guid.NewGuid():N}"` to prevent test interference
2. **Cleanup in TestCleanup**: Always drop test databases after tests complete
3. **Access IMongoClient**: Get from `IMongoDatabase.Client` property via the factory:
   ```csharp
   var factory = provider.GetRequiredService<IMongoDatabaseFactory>();
   var mongoDatabase = factory.Create<MongoDatabaseOptions>();
   _mongoClient = mongoDatabase.Client; // For cleanup operations
   ```

### Test Variables

| Variable | Description |
|----------|-------------|
| `MONGODB_CONNECTION_STRING` | Connection string (e.g., `mongodb://localhost:27017`) |
| `MONGODB_DATABASE_NAME` | Default database name for tests |

See [TEST_VARIABLES.md](../../TEST_VARIABLES.md) for complete reference.

---

See [back](MajorFunctionality.md)