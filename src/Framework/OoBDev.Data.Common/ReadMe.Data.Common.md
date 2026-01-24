# OoBDev.Data.Common

Core implementations for database query execution and operations.

## Description

This package provides concrete implementations of database operations including query execution, connection management, and result mapping. It implements the abstractions defined in OoBDev.Data.Common.Abstractions.

## Key Features

- DatabaseQuery implementation for query execution
- Connection pooling support
- Transaction management
- Result set enumeration
- Async query execution
- Integration with dependency injection

## Installation

```xml
<PackageReference Include="OoBDev.Data.Common" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.Data.Common;
using Microsoft.Extensions.DependencyInjection;

services.AddDatabaseQuery<MyDatabaseOptions>();

// Execute query
var results = await databaseQuery.ExecuteAsync(myQuery);
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
