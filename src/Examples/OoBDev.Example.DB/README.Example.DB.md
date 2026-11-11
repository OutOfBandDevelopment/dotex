# OoBDev.Example.DB

Example SQL Server database project that shows how a table asks the vector database for embeddings: inserting a name queues an embedding request on the Service Broker queue of `OoBDev.Data.Vectors.DB`.

## What it contains

- `Tables/dbo.Names.sql`: table `[dbo].[Names]` (`NameID`, `NameValue`) and the trigger `Names_UpdateInsert`. After an insert or update the trigger sends the changed rows as a batch with `[embedding].[oobdev://embedding/sentence-transformer/request/send-batch]`, using `[dbo].[Names]` as the table name. The reader in `OoBDev.Data.Vectors.Hosting` answers the request (see its readme for the message flow).
- `Post-Deployment/`: `SetDacVersion.sql` plus a placeholder for your own scripts.
- A project reference to `OoBDev.Data.Vectors.DB`, deployed into the same database (`IncludeCompositeObjects`), and a build step that makes sure the SQL CLR dacpac of `OoBDev.Data.Vectors.Net481` exists first.

## Build and deploy

```bash
dotnet build
dotnet publish /p:TargetPassword=<sa password>
```

**Table 1 — Deployment settings** (properties in the .csproj, overridable on the command line)

| Property | Default |
|----------|---------|
| `TargetServerName` | `127.0.0.1` |
| `TargetPort` | `1433` |
| `TargetDatabaseName` | `ExampleDb` |
| `TargetUser` | `sa` |
| `TargetPassword` | from the `VECTORS_DB_PASSWORD` environment variable; never stored in the project |

The server needs CLR enabled and the assembly trusted; the steps are in the [Data.Vectors.DB readme](../../Extensions/OoBDev.Data.Vectors.DB/README.Data.Vectors.DB.md#sql-server-prerequisites). Publish runs the deployment (`DeployOnPublish`).
