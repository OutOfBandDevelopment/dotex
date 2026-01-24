# OoBDev.Data.Common.Abstractions

Core abstractions for database operations and query mapping.

## Description

This package provides fundamental abstractions for database operations including database mappers, query interfaces, and connection management. It defines the contracts used across the OoBDev data access framework.

## Key Features

- IDatabaseMapper interface for database mapping
- IDatabaseQuery interface for query execution
- Connection string management abstractions
- Stored procedure parameter mapping
- Result set mapping interfaces
- Type-safe database operations

## Installation

```xml
<PackageReference Include="OoBDev.Data.Common.Abstractions" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.Data.Common;

// Implement custom database mapper
public class MyDatabaseMapper : IDatabaseMapper
{
    public DbConnection GetConnection<TDbOptions>()
    {
        // Your implementation
    }

    // Implement other interface members
}
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
