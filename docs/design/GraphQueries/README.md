# Graph queries: SQL Server examples and PostgreSQL options

**Status:** Draft · **Date:** 2026-11-11 · **Request:** owner, 2026-11-11 · **Related:** [HybridSearch](../HybridSearch/README.md) (Neo4j graph adapter), [DesiredStateDatabase](../DesiredStateDatabase/README.md), `src/Examples/OoBDev.Example.DB`

Examples of graph queries on SQL Server, and what PostgreSQL offers for the same questions. The examples are teaching material and the test bed for the graph retrieval adapters; they add no framework API.

## Contents

1. [Questions to answer](#questions-to-answer)
2. [What each database offers](#what-each-database-offers)
3. [Plan](#plan)

**List of tables**

1. [Table 1 — Graph support by database](#table-1)

## Questions to answer

- Who is connected to whom within N steps (friends of friends, parts of an assembly, document links)?
- What is the shortest path between two nodes?
- What are all ancestors or descendants in a parent/child tree, and where are the cycles?

## What each database offers

**Table 1 — Graph support by database**

| Database | Native graph? | How to query |
|----------|---------------|--------------|
| SQL Server (2017 and later) | Yes: node tables (`AS NODE`) and edge tables (`AS EDGE`) | `MATCH` patterns in `SELECT`; `SHORTEST_PATH` with `+` and `{1,n}` quantifiers from SQL Server 2019; edge constraints |
| PostgreSQL | No graph language in the core server | `WITH RECURSIVE` (with the `SEARCH` and `CYCLE` clauses since PostgreSQL 14) covers parent/child trees and general graphs; the Apache AGE extension adds openCypher on property graphs; `ltree` suits pure hierarchies; pgRouting suits path algorithms. SQL:2023 `GRAPH_TABLE` (SQL/PGQ) is under development upstream; check the current release before relying on it |
| SQLite | No | `WITH RECURSIVE` only |
| Neo4j | Yes | Cypher, through the planned graph adapter |

So PostgreSQL supports more than parent/child searching through recursive CTEs, but real graph pattern queries need an extension (Apache AGE) or a dedicated graph database.

## Plan

1. Add graph tables (node and edge) and a post-deployment sample data set to `OoBDev.Example.DB`, with queries for the three questions above using `MATCH` and `SHORTEST_PATH`.
2. Write the same three queries as recursive CTEs for PostgreSQL and SQLite, as documented examples with Integration tests on the containers (the PostgreSQL service is already on the backlog).
3. Evaluate Apache AGE as a spike after the PostgreSQL service exists.
4. Reuse the data set to test the Neo4j adapter.
