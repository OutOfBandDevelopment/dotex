# OoBDev.Microsoft.SqlServer.Server

SQL Server database abstraction with Service Broker integration.

## Description

This package provides comprehensive SQL Server integration including database mapping, stored procedure execution, and Service Broker message queue support. It implements the OoBDev database abstraction patterns for SQL Server.

## Key Features

- Attribute-based database query mapping
- Stored procedure parameter binding
- SQL Service Broker queue message provider
- Connection string management
- JSON and XML parameter serialization
- Dynamic result mapping with expression trees
- Command timeout configuration

## Installation

```xml
<PackageReference Include="OoBDev.Microsoft.SqlServer.Server" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.Microsoft.SqlServer.Server;
using Microsoft.Extensions.DependencyInjection;

services.AddSqlServerSupport(configuration);

// Define query with attributes
[ConnectionStringName("MyDatabase")]
[StoredProcedure("dbo.GetUsers")]
public class GetUsersQuery
{
    [QueryParameter("@userId")]
    public int UserId { get; set; }
}

// Execute query
var results = await databaseQuery.ExecuteAsync(new GetUsersQuery { UserId = 123 });
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
