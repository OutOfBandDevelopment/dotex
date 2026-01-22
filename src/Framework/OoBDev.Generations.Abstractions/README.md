# OoBDev.Generations.Abstractions

Contracts and interfaces for deterministic procedural test data generation.

## Overview

OoBDev.Generations.Abstractions provides the core interfaces and attributes for building deterministic, procedural test data generators. The framework supports seed-based reproducibility, ensuring that the same seed always produces the same test data.

## Key Interfaces

### IProcedualGenerationProvider

The main entry point for test data generation:

```csharp
public interface IProcedualGenerationProvider
{
    IProcedualGenerationContext CreateContext(Type type, IEnumerable<Attribute>? attributes = default, IProcedualGenerationContext? context = default, int? index = default);
    object? Generate(IProcedualGenerationContext context, bool required = true);
    IServiceProvider? ServiceProvider { get; }
}
```

### IProcedualGenerationContext

Provides context for generation operations including type information, random number generator, and attributes:

```csharp
public interface IProcedualGenerationContext
{
    Type TargetType { get; }
    Random Random { get; }
    IEnumerable<Attribute> Attributes { get; }
    IProcedualGenerationProvider Provider { get; }
}
```

### IGenerateObject

Interface for custom value generators:

```csharp
public interface IGenerateObject : IHavePriority
{
    object? GenerateValue(IProcedualGenerationContext context);
    bool CanGenerateValue(IProcedualGenerationContext context);
}
```

## Generation Rules

The framework includes several built-in attribute-based rules:

- **EmailAddressAttribute** - Generates random email addresses
- **FirstSpaceLastNameAttribute** - Generates "FirstName LastName" format
- **LastCommaFirstNameAttribute** - Generates "LastName, FirstName" format
- **AddressAttribute** - Generates street addresses
- **PhoneAttribute** - Generates phone numbers
- **DateTimeAttribute** - Generates date/time values
- **NumberAttribute** - Generates numeric values with configurable precision
- **BooleanAttribute** - Generates boolean values
- **WordsAttribute** - Generates random word sequences
- **ArrayAttribute** - Generates arrays of values

## Extension Methods

```csharp
// Generate a value of type T
T value = provider.Generate<T>();

// Generate from a service provider
T value = serviceProvider.Generate<T>();

// Get a generation rule from context
var rule = context.GetRule<EmailAddressAttribute>();

// Choose randomly from a collection
var item = context.ChooseFrom(items);
```

## Usage Example

```csharp
public class User
{
    [EmailAddress]
    public string Email { get; set; }

    [FirstSpaceLastName]
    public string FullName { get; set; }

    [Number(Factor = 100)]
    public int Age { get; set; }

    [Phone]
    public string Phone { get; set; }
}

// Generate a user with deterministic data
var user = provider.Generate<User>();
```

## Deterministic Generation

All generation is deterministic based on the seed:

```csharp
// Same seed = same data
var context1 = builder.WithSeed(42).Build();
var context2 = builder.WithSeed(42).Build();

var user1 = context1.Generate<User>(); // Same data
var user2 = context2.Generate<User>(); // Same data
```

## Priority System

Rules can specify priority for resolution order:

```csharp
public interface IHavePriority
{
    int Priority { get; }
}
```

Lower priority values are processed first. Use this to control which rule applies when multiple rules match.

## See Also

- **OoBDev.Generations** - Core implementation
- **OoBDev.Generations.Extensions** - Dependency injection extensions
- **OoBDev.Generations.Tests** - Test examples and patterns

## License

Copyright (c) Out-of-Band Development, LLC. All rights reserved.
