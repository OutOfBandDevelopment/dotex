# OoBDev.Generations.Extensions

Dependency injection extensions for OoBDev.Generations test data generation framework.

## Overview

This package provides extension methods for integrating OoBDev.Generations with Microsoft.Extensions.DependencyInjection, making it easy to use procedural test data generation in ASP.NET Core applications, test projects, and other DI-enabled .NET applications.

## Installation

```bash
dotnet add package OoBDev.Generations.Extensions
```

## Usage

### Basic Registration

Register the procedural generation services in your DI container:

```csharp
using OoBDev.Generations.Extensions;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddProcedualGenerationServices();
    }
}
```

### Inject and Use

Once registered, you can inject the generation provider:

```csharp
public class MyService
{
    private readonly IProcedualGenerationProvider _generator;

    public MyService(IProcedualGenerationProvider generator)
    {
        _generator = generator;
    }

    public User CreateTestUser()
    {
        return _generator.Generate<User>();
    }
}
```

### Test Projects

In test projects using MSTest, xUnit, or NUnit:

```csharp
public class UserServiceTests
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IProcedualGenerationProvider _generator;

    public UserServiceTests()
    {
        var services = new ServiceCollection();
        services.AddProcedualGenerationServices();
        _serviceProvider = services.BuildServiceProvider();
        _generator = _serviceProvider.GetRequiredService<IProcedualGenerationProvider>();
    }

    [TestMethod]
    public void CreateUser_ShouldReturnValidUser()
    {
        // Arrange
        var user = _generator.Generate<User>();

        // Act & Assert
        Assert.IsNotNull(user);
        Assert.IsNotNull(user.Email);
    }
}
```

## Registration Details

The `AddProcedualGenerationServices` method registers:

- **IProcedualGenerationProviderBuilder** as Singleton - Builder for creating providers
- **IProcedualGenerationProvider** as Transient - Main generation provider

### Lifetime Scopes

- **Singleton (IProcedualGenerationProviderBuilder)**: Shared across the application, reused for creating providers
- **Transient (IProcedualGenerationProvider)**: New instance per injection, ensuring independent generation contexts

## Advanced Configuration

### Custom Seed

Create a custom provider builder with a specific seed:

```csharp
services.AddSingleton<IProcedualGenerationProviderBuilder>(sp =>
    new ProcedualGenerationProviderBuilder()
        .WithSeed(42)
        .WithServiceProvider(sp)
);
```

### Integration with Existing Services

The generation provider can access other services from the DI container:

```csharp
services.AddProcedualGenerationServices();
services.AddSingleton<ICustomService, CustomService>();

// The provider can now resolve ICustomService
var provider = serviceProvider.GetRequiredService<IProcedualGenerationProvider>();
```

## Extension Methods

### AddProcedualGenerationServices

```csharp
public static IServiceCollection AddProcedualGenerationServices(
    this IServiceCollection services)
```

Registers the procedural generation services using TryAdd* methods, so multiple calls won't duplicate registrations.

## See Also

- **OoBDev.Generations.Abstractions** - Core interfaces and attributes
- **OoBDev.Generations** - Core implementation
- **OoBDev.Generations.Tests** - Test examples and patterns

## License

Copyright (c) Out-of-Band Development, LLC. All rights reserved.
