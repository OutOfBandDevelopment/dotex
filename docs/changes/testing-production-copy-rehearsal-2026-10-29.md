# Production-copy rehearsal, first slice (2026-10-29)

Integration test `RestoredCopy_WithNewBrokerId_KeepsTheDataAndIsQueryable` (`OoBDev.Data.Vectors.DB.Tests`) seeds the deployed vectors database, backs it up inside the Docker SQL Server, restores it under a unique throwaway name and checks the rows and the CLR vector type still work. It cleans the copy up and uses the existing test-instance guard. Passed.

Findings that shape the rest of the rehearsal:
- A restore beside the original needs `WITH NEW_BROKER`; otherwise SQL Server refuses to enable the second Service Broker (Msg 9772).
- Redeploying the same dacpac over an existing, populated vectors database fails: the generated script drops every CLR function and aggregate and rebuilds `embedding.HashPlanes`, whose persisted computed `Length` column is then rejected as non-deterministic (Msg 4936). The upgrade path is therefore not idempotent; this is in the backlog.
