# Vectors database redeploy is idempotent (2026-10-29)

Deploying the same `OoBDev.Data.Vectors.DB` dacpac over a populated database used to drop and recreate the four CLR user-defined types (Matrix, MatrixF, Vector, VectorF), the aggregates and the functions, then rebuild `embedding.HashPlanes` and `embedding.Vectors`. The rebuild failed with Msg 4936 (the persisted computed column `Length` "cannot be persisted because the column is non-deterministic"), because the types it depends on had just been dropped.

Two causes, both fixed:

- **UDT elements.** `DacPacBuilder.UserDefinedTypes` wrote `Format`, `MaxByteSize`, `IsByteOrdered` and a `Methods` relationship into each `SqlUserDefinedType`. The model DacFx builds from the deployed database carries only the class name, assembly and schema, so every type compared as changed. These values already live in the assembly's `SqlUserDefinedType` attribute and SQL Server reads them from there on `CREATE TYPE ... EXTERNAL NAME`, so the builder no longer emits them (`Methods(...)` stays available as a public method, unused by the type element).
- **Computed columns.** The persisted `Length` and `Magnitude` columns of both tables are written as `CONVERT([embedding].[VectorF],[Value]).Length()`, the form the database reports back, so they no longer compare as a different expression.

Test: `SqlServerDeploymentTests.Redeploy_SameDacpac_ChangesNothing` (Integration, Docker SQL Server) generates the deploy script for a second deployment and asserts it neither drops a type nor rebuilds a table, then deploys with `BlockOnPossibleDataLoss`. It failed with Msg 4936 before the change. All 8 tests of `OoBDev.Data.Vectors.DB.Tests` pass. The dacpac was rebuilt in Debug and Release.
