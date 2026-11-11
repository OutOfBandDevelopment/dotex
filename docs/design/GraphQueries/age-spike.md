# Apache AGE spike result

**Status:** Done · **Date:** 2026-11-11 · **Related:** [GraphQueries](README.md)

Question: can PostgreSQL answer the graph questions of the [plan](README.md#plan) with openCypher through the Apache AGE extension, so that an `IGraphStore` provider on PostgreSQL is feasible?

## Method

Ran the `apache/age` image (PostgreSQL 18.6, 695 MB) ad hoc with `docker run` (not added to the test stack), enabled the extension (`CREATE EXTENSION age; LOAD 'age';`, `search_path = ag_catalog`), created a graph and ran openCypher through `cypher('graph', $$ ... $$) AS (col agtype)`. Container removed afterwards.

## Findings

- Create graph, create nodes and edges, and variable-length traversal work: `MATCH (a:Person {name:'Ann'})-[:KNOWS*1..2]->(x) RETURN x.name` returned Bob and Cat. Undirected traversal (`-[:KNOWS*1..3]-`) with `RETURN DISTINCT` also works.
- `shortestPath(...)` is **not supported**: `syntax error at or near "shortestPath"`. A shortest path needs a variable-length match ordered by path length, or a recursive CTE over the AGE tables.
- Every query is wrapped in `SELECT * FROM cypher(...) AS (...)`, results come back as `agtype` (JSON-like text, quoted strings), parameters are passed through a prepared statement, and labels cannot be parameters. A provider has to parse `agtype`.
- The extension needs a custom image or a managed service that offers it (it is not in the stock `postgres` image the test stack uses), and it lags the newest PostgreSQL major version in general.

## Decision

Do not build an AGE-backed `IGraphStore` now. Keep Neo4j (Cypher, built) and SQL Server graph tables as the supported graph engines, and the recursive CTEs of `OoBDev.Example.GraphQueries.Tests` for plain PostgreSQL. Revisit if a deployment needs graph queries on PostgreSQL only, then add a provider behind `IGraphStore` that maps `NeighborsAsync` to a variable-length match and `ShortestPathAsync` to a recursive CTE.
