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

Build produces `OoBDev.Data.Vectors.DB.dacpac`. The SQL CLR vector types come from `OoBDev.Data.Vectors.Net481` (a dacpac with the assembly and the `[embedding].[Vector]`, `VectorF`, `Matrix` and `MatrixF` types) and are merged into this package's deployment (`IncludeCompositeObjects`).

```bash
dotnet build
dotnet publish /p:TargetPassword=<sa password>
```

## SQL Server prerequisites

The server must allow CLR before the types can be created (a Linux SQL Server container supports `SAFE` CLR):

```sql
EXEC sp_configure 'show advanced options', 1; RECONFIGURE;
EXEC sp_configure 'clr enabled', 1; RECONFIGURE;
```

The assembly must also be trusted. For a development server turn off `clr strict security`; on a shared server trust the assembly hash instead:

```sql
EXEC sp_configure 'clr strict security', 0; RECONFIGURE;   -- development only
-- production: EXEC sys.sp_add_trusted_assembly @hash = <SHA-512 of the assembly>;
```

Deploying with `sqlpackage` directly needs the composite flag: `sqlpackage /Action:Publish /SourceFile:OoBDev.Data.Vectors.DB.dacpac /Properties:IncludeCompositeObjects=True ...`. Keep the sqlpackage tool in a short path on Windows (long paths fail with an SNI error).

The `OoBDev.Data.Vectors.Tests` project has Integration tests (`SqlServerDeploymentTests`) that do all of this against the Docker SQL Server and check distances, angles and NULL results.

## Configuration

Deployment settings are properties in the .csproj and can be overridden on the command line:

**Table 1 — Deployment settings**

| Property | Default | Notes |
|----------|---------|-------|
| `TargetServerName` | `127.0.0.1` | |
| `TargetPort` | `1433` | |
| `TargetDatabaseName` | `VectorDb` | |
| `TargetUser` | `sa` | SQL authentication |
| `TargetPassword` | from `VECTORS_DB_PASSWORD` | Never stored in the project; empty prompts |

Functions return NULL for undefined results; see `OoBDev.Data.Vectors` readme, "NULL results".

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
