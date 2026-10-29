# SQLCLR aggregates: Merge fixed and covered by tests

**Date:** 2026-10-28
**Status:** Complete

- Defect: `Merge` of `Minimum`, `MinimumF`, `Maximum` and `MaximumF` added the partial results (`+=`) instead of taking the element-wise minimum or maximum, so a parallel plan returned wrong values. All six aggregates (with `Centroid` and `CentroidF`) also indexed past the end when merging an empty partial into a filled one, and now ignore an empty partial and throw `NotSupportedException` for different lengths, like `Accumulate`.
- Tests: `AggregateTests` (Unit, `OoBDev.Data.Vectors.Tests`: accumulate, merge of two partials, empty partial, mismatched lengths) and `Aggregates_RunInsideSqlServer` (Integration, `OoBDev.Data.Vectors.DB.Tests`: `MinimumF`, `MaximumF`, `CentroidF` on the deployed dacpac).

Verified: Unit 69 passed in `OoBDev.Data.Vectors.Tests`; the Integration test passed on the Aspire SQL Server. The SQL Server run did not force a parallel plan, so the merge fix is verified by the Unit tests only.

## Matrix read defect and SQL Server coverage

- Defect: `SqlMatrix.Read` (double matrix) used `ReadSingle` while `Write` wrote doubles, so every `embedding.Matrix` read back from SQL Server held wrong values. It now uses `ReadDouble`.
- Tests: `BinarySerializationTests` (Unit: round trip of `Matrix`, `MatrixF`, `Vector`, `VectorF`) and `MatrixAndDoubleVector_RunInsideSqlServer` (Integration: `MatrixF`/`Matrix` `Row`, `Column`, out of range gives NULL, double `Vector` `Magnitude`, `Distance`, `Length`). This closes the SQLCLR coverage item (the old `OoBDev.DacFx.Tests` note).

Verified: Unit 72 passed; `OoBDev.Data.Vectors.DB.Tests` Integration 5 passed on the Aspire SQL Server.
