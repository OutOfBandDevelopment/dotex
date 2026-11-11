# OoBDev.Neo4j

Graph retrieval adapter: `IGraphStore` (`OoBDev.Search.Graph`, in `OoBDev.Search.Abstractions`) over Neo4j with Cypher through `Neo4j.Driver`.

## Use

```csharp
services.TryAddNeo4jServices(configuration, "Neo4j");
```

```json
{ "Neo4j": { "Uri": "bolt://localhost:7687", "User": "neo4j", "Password": "...", "Database": null, "MaxPathLength": 15 } }
```

| Operation | Cypher |
|-----------|--------|
| `UpsertNodeAsync` | `MERGE (n:Label {id: $id}) SET n = $properties` |
| `ConnectAsync` | `MATCH` both nodes, `MERGE (a)-[:TYPE]->(b)` (idempotent) |
| `NeighborsAsync` | variable-length match `*1..N`, either direction, nearest first |
| `ShortestPathAsync` | `shortestPath` bounded by `MaxPathLength`; empty list when unconnected |
| `DeleteNodeAsync` | `DETACH DELETE` |

Labels and edge types cannot be parameters in Cypher, so they are validated (`[A-Za-z_][A-Za-z0-9_]*`) before they are put in the query text; values always travel as parameters.

## Tests

Unit tests run anywhere. The `Integration` test needs the Neo4j container (`containers/testing`, `NEO4J_URI`, `NEO4J_USER`, `NEO4J_PASSWORD`); it uses unique ids per run and deletes its nodes.
