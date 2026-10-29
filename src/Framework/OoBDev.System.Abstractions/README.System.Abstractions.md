# OoBDev - System.Abstractions 

## Summary

This library is the core collection of interface contracts and models to support the 
OoBDev Common Framework
## Vector math

`OoBDev.System.Math` holds the application-level vector type, beside the SQL Server CLR vectors in `OoBDev.Data.Vectors` (which return a cosine similarity; this type returns a cosine distance, `1 - similarity`).

| Type | Purpose |
|------|---------|
| `Vector` | `double[]` with a cached magnitude; `Create`, `Parse` (JSON), `From(object)`, binary `Read` and `Write`, `Distance(other, metric)` |
| `VectorMath` | `CosineDistance`, `EuclideanDistance`, `DotProduct`, `ManhattanDistance` |
| `VectorDistanceMetrics` | Cosine, Euclidean, DotProduct, Manhattan |
| `VectorComparer` | Orders vectors by the sign of their distance for the chosen metric |

Tests are in `OoBDev.System.Tests/Math`. The comparer treats a distance within 1e-12 of zero as equal, so cosine rounding does not separate equal vectors.
