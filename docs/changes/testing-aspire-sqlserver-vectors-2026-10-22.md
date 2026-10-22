# Aspire host: SQL Server 2019 with the vector types

**Date:** 2026-10-22 (owner request)

## Done

- `OoBDev.Example.AppHost` adds a SQL Server container (`2019-latest`, data volume) and a `VectorDb` database (`Aspire.Hosting.SqlServer`).
- `VectorDatabaseDeployment.WithVectorTypes` runs when the database is ready: allows SQL CLR, then publishes the `OoBDev.Data.Vectors.DB` dacpac (with the CLR vector types merged in) through DacFx. The API references the database and waits for it.
- The dacpac project is a build-only project reference; `Microsoft.Data.SqlClient`, `Microsoft.SqlServer.DacFx` and the `System.Security.Cryptography.Pkcs` pin (as in the DB tests) are added to the host.

## Verification

- `dotnet build` of the host: succeeded.
- `SqlServerDeploymentTests` (3 Integration tests) against a temporary `mssql/server:2019-latest` container: 3 passed.
- The Aspire host itself was not started (it builds the Ollama image and starts about eight containers), so the in-host deploy callback has not run end to end.

## Follow-up

BACKLOG: the API still uses `InMemoryVectorStore`; the dacpac path is fixed to the Debug output.
