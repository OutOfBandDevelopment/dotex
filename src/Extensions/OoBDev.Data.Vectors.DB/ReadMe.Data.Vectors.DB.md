# OoBDev.Data.Vectors.DB

SQL Server database project (DacPac) for vector database operations with Service Broker integration.

## Description

This database project provides a complete vector database implementation for SQL Server including SQL CLR vector types, Service Broker message queuing for asynchronous embedding generation, and vector search capabilities.

## Key Features

- Complete vector database schema
- SQL CLR vector and matrix types deployment
- Service Broker queues for embedding requests
- Message contracts and services
- Vector search stored procedures
- Pre-configured for VectorDb database

## Installation

Build and deploy using:

```bash
dotnet build
dotnet publish
```

## Configuration

Database deployment settings in .csproj:
- Server: 127.0.0.1:1433
- Database: VectorDb
- Authentication: SQL Authentication (sa/L0c@lD3v)

Update the .csproj file for your environment before deployment.

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
