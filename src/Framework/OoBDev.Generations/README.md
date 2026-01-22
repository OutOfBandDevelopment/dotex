# OoBDev.Generations

Core implementation of deterministic procedural test data generation.

## Overview

OoBDev.Generations provides the complete implementation for generating deterministic, reproducible test data. Built on a seed-based random number generator, it ensures that the same seed always produces identical test data, making tests repeatable and debuggable.

## Features

- Deterministic test data generation based on seed values
- Procedural generation with support for complex object graphs
- Attribute-based configuration for fine-grained control
- Built-in generators for common data types
- Extensible architecture for custom generators
- Support for interfaces and abstract types via proxy generation
- Configurable array and collection generation

## Quick Start

### Basic Usage

```csharp
using OoBDev.Generations;

// Create a provider with a specific seed
var provider = new ProcedualGenerationProviderBuilder()
    .WithSeed(42)
    .Build();

// Generate test data
var user = provider.Generate<User>();
```

### With Dependency Injection

```csharp
services.AddGenerations(options =>
{
    options.Seed = 42; // Optional: use a specific seed
});

// Inject and use
public class MyTest
{
    private readonly IProcedualGenerationProvider _generator;

    public MyTest(IProcedualGenerationProvider generator)
    {
        _generator = generator;
    }

    [Fact]
    public void TestMethod()
    {
        var user = _generator.Generate<User>();
        // ... test logic
    }
}
```

## Built-In Generators

The framework includes generators for all common types:

### Primitive Types
- **GenerateIntegerAttribute** - Generates int values
- **GenerateLongAttribute** - Generates long values
- **GenerateDoubleAttribute** - Generates double values
- **GenerateBooleanAttribute** - Generates boolean values
- **GenerateStringAttribute** - Generates string values
- **GenerateGuidAttribute** - Generates GUID values
- **GenerateDateTimeAttribute** - Generates DateTime values

### Complex Types
- **GenerateObjectAttribute** - Generates complex objects
- **GenerateArrayAttribute** - Generates arrays
- **GenerateCollectionAttribute** - Generates collections (List, IEnumerable, etc.)
- **GenerateQueryableAttribute** - Generates IQueryable sequences
- **GenerateNullableAttribute** - Generates nullable types
- **GenerateEnumerationAttribute** - Generates enum values

### Special Types
- **GenerateInterfaceAttribute** - Generates interface implementations via DispatchProxy
- **GenerateAbstractAttribute** - Generates abstract type implementations

## Custom Object Generation

### Simple Objects

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

var product = provider.Generate<Product>();
// Id: random int
// Name: random string
// Price: random decimal
```

### Attribute-Based Configuration

```csharp
public class Order
{
    [Number(Factor = 1000000)]
    public int OrderId { get; set; }

    [EmailAddress]
    public string CustomerEmail { get; set; }

    [Array(MinLength = 1, MaxLength = 10)]
    public Product[] Items { get; set; }

    [DateTime(MinYear = 2020, MaxYear = 2024)]
    public DateTime OrderDate { get; set; }
}

var order = provider.Generate<Order>();
```

## Seed-Based Determinism

The same seed always produces the same data:

```csharp
var provider1 = new ProcedualGenerationProviderBuilder()
    .WithSeed(42)
    .Build();

var provider2 = new ProcedualGenerationProviderBuilder()
    .WithSeed(42)
    .Build();

var user1 = provider1.Generate<User>();
var user2 = provider2.Generate<User>();

// user1 and user2 will have identical data
Assert.Equal(user1.Name, user2.Name);
Assert.Equal(user1.Email, user2.Email);
```

## Advanced Features

### Interface Generation

Generate implementations of interfaces using DispatchProxy:

```csharp
public interface IRepository
{
    Task<User> GetUserAsync(int id);
    Task SaveAsync(User user);
}

var repository = provider.Generate<IRepository>();
// Returns a working proxy with generated return values
```

### Abstract Type Generation

Generate instances of abstract classes:

```csharp
public abstract class BaseEntity
{
    public abstract int Id { get; set; }
    public abstract string Name { get; set; }
}

var entity = provider.Generate<BaseEntity>();
// Returns a derived type with generated values
```

### Collection Generation

```csharp
// Generate lists
var users = provider.Generate<List<User>>();

// Generate arrays with specific size
[Array(MinLength = 5, MaxLength = 5)]
public User[] FixedSizeArray { get; set; }

// Generate IQueryable sequences
var queryable = provider.Generate<IQueryable<User>>();
```

## Context and Customization

### Custom Generators

Implement `IGenerateObject` for custom generation logic:

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class CustomValueAttribute : Attribute, IGenerateObject
{
    public int Priority => 0;

    public bool CanGenerateValue(IProcedualGenerationContext context)
        => context.TargetType == typeof(string);

    public object? GenerateValue(IProcedualGenerationContext context)
    {
        // Custom generation logic
        return $"Custom_{context.Random.Next(1000)}";
    }
}
```

### Context Builder

Configure generation context:

```csharp
var context = new ProcedualGenerationContextBuilder()
    .WithSeed(42)
    .WithServiceProvider(serviceProvider)
    .Build();
```

## Architecture

### Key Components

1. **ProcedualGenerationProvider** - Main provider implementation
2. **ProcedualGenerationContext** - Context for generation operations
3. **ProcedualGenerationSeedGenerator** - Deterministic seed generation
4. **ProceduralGenerationDispatchProxyFactory** - Interface proxy generation
5. **ProceduralGenerationTypeBuilderFactory** - Dynamic type generation
6. **ProcedualGenerationRegister** - Generator registration and discovery

### Generation Flow

1. Context is created with target type and attributes
2. Provider checks for applicable generation rules (IGenerateObject)
3. Rules are sorted by priority
4. First matching rule generates the value
5. For complex objects, recursively generate properties

## Performance Considerations

The current implementation prioritizes correctness and determinism over raw performance. Future optimizations may include:

- Cached reflection metadata
- Object pooling for frequently generated types
- Compiled expression trees for property access
- Optimized collection generation

## See Also

- **OoBDev.Generations.Abstractions** - Core interfaces and attributes
- **OoBDev.Generations.Extensions** - Dependency injection extensions
- **OoBDev.Generations.Tests** - Test examples and patterns

## License

Copyright (c) Out-of-Band Development, LLC. All rights reserved.
