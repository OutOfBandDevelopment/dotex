# Example Aspire host: the API uses the SQL vector store

**Date:** 2026-10-28
**Status:** Built; not run end to end

`OoBDev.Example.AppHost` now passes the `VectorDb` connection string to the API as `SqlVectorStoreOptions__ConnectionString`, the setting `OoBDev.Example.WebApi` checks before it registers the SQL vector store (`TryAddSqlVectorStore`); until now it fell back to `InMemoryVectorStore`. The dacpac path uses the host's own build configuration (`#if DEBUG`) instead of a fixed `Debug` folder.

Verified: the AppHost builds with 0 errors. Not run: starting the host (SQL Server 2019 image, Ollama image build) was not done in this session, so the vector store switch is unconfirmed at runtime.
