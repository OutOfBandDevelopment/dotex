# OoBDev.Example.GraphQueries.Tests

Teaching scripts and tests for graph questions on relational databases. One small social graph (Ann-Bob-Cat-Dan and Ann-Eve-Dan, Fay alone) answers two questions on three engines.

| Question | SQL Server 2019+ (`Graph/SqlServer.*.sql`) | PostgreSQL and SQLite (`Graph/Recursive.*.sql`) |
|----------|--------------------------------------------|-------------------------------------------------|
| Who is reachable within two steps of Ann? | `AS NODE` / `AS EDGE` tables, `MATCH(p1-(k1)->p2-(k2)->p3)` | `WITH RECURSIVE` walk with a depth limit |
| Shortest path Ann to Dan | `MATCH(SHORTEST_PATH(p1(-(k)->p2)+))` with `STRING_AGG ... WITHIN GROUP (GRAPH PATH)` | `WITH RECURSIVE` walk with a path string as cycle guard, shortest first, `LIMIT 1` |

The recursive scripts run unchanged on PostgreSQL and SQLite. Friendship is stored in both directions in all three.

The tests (tier `Integration`) create a throwaway database (SQL Server), schema (PostgreSQL) or in-memory database (SQLite), run the scripts through the same helper and assert the answers: friends `Bob, Cat, Dan, Eve`, shortest path `Eve>Dan` (SQL Server) and `1,5,4` (recursive). They use `SQLSERVER_CONNECTION_STRING` and `POSTGRES_CONNECTION_STRING` from `src/.runsettings` and need the `sql-server` and `postgres` services of `containers/testing`.

The scripts live here, not in `OoBDev.Example.DB`, because that project is a dacpac whose build compiles every `.sql` file as T-SQL. The design is in [GraphQueries](../../../docs/design/GraphQueries/README.md).

## Trees and cycles

A second data set is a parent/child tree (CEO, VP, Dev, Ops, Intern) plus a cycle A, B, C. `Tree.Descendants`, `Tree.Ancestors` and `Tree.Cycles` (PostgreSQL and SQLite) and `SqlServer.Tree.*` answer: all descendants of CEO (`Dev, Intern, Ops, VP`), the ancestors of Intern (`Dev, VP, CEO`) and the nodes that are their own ancestor (`A, B, C`). A depth limit stops the walk in a cycle; SQL Server has no `RECURSIVE` keyword and does not allow an aggregate in the recursive part, so its limit is a constant.
