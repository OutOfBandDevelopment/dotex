# OoBDev.Data.Vectors

SQL CLR vector and matrix types with distance calculation functions for SQL Server.

## Description

This package provides high-performance SQL CLR user-defined types (UDT) and aggregates for vector and matrix operations in SQL Server. It enables semantic search, similarity calculations, and vector mathematics directly within the database.

## Key Features

- SqlVector and SqlVectorF types (double and float precision)
- SqlMatrix and SqlMatrixF types for matrix operations
- Distance metrics: Cosine, Euclidean, Manhattan, Dot Product
- Vector aggregations: Centroid, Min, Max
- Matrix aggregations for batch operations
- Byte-ordered serialization for indexing
- SQL CLR integration with full T-SQL support

## Installation

```xml
<PackageReference Include="OoBDev.Data.Vectors" Version="*" />
```

## Basic Usage

```sql
-- Create vector from JSON array
DECLARE @v1 embedding.Vector = '[1.0, 2.0, 3.0]'
DECLARE @v2 embedding.Vector = '[4.0, 5.0, 6.0]'

-- Calculate cosine similarity
SELECT @v1.Similarity(@v2)

-- Calculate distance
SELECT @v1.Distance(@v2, 'cosine')

-- Find nearest neighbors
SELECT TOP 10 Id, Embedding.Distance(@query, 'cosine') as Distance
FROM Documents
ORDER BY Distance
```

## NULL results (read this before ordering by angle)

The two-vector functions never throw on bad data, because one bad row must not fail a whole batch. `Angle`/`AngleF` return `NULL` when the angle is undefined: a `NULL` input, vectors of different lengths, or a zero-magnitude vector. `Distance`/`DistanceF`, `Midpoint`/`MidpointF` and `UniformV`/`UniformVF` return `NULL` for `NULL` input or vectors of different lengths, and `Distance`/`DistanceF` also for an unsupported metric name.

`NULL` means "no answer", not "good match". SQL Server sorts `NULL` first in an ascending `ORDER BY`, so an unfiltered nearest-match query would list these rows on top. Always filter or sort them away:

```sql
SELECT TOP (10) Id, a.Angle(@query) AS Angle
FROM dbo.Items AS a
WHERE a.Angle(@query) IS NOT NULL
ORDER BY Angle;
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
