# Search latency gate (2026-10-29)

- `SearchLatencyGateTests` (Simulate, `OoBDev.System.Tests`): builds a free-text search and a filter with the real `ExpressionTreeBuilder` and applies both to 50,000 in-memory rows; logs over 500 ms, fails over 750 ms. Measured about 20 ms.
- Scope: the expression-building and evaluation path of the search syntax. Database-backed and vector search latency stay with the Integration suite and are not gated yet.
