# OoBDev.History

Time-series history with tiered rollups for small hosts: `IHistoryStore` (append a point, read raw points, read tier buckets, prune by retention), `HistoryTier` and `HistoryOptions` (defaults: raw 1 day, 1 minute for 7 days, 1 hour for 90 days, 1 day for 5 years), and `InMemoryHistoryStore` for tests. Rollups are updated as points arrive, so no rollup job runs. The SQLite provider and the snapshot helper are in [OoBDev.History.Sqlite](../OoBDev.History.Sqlite/README.History.Sqlite.md). Design: [HistoryStore](../../../docs/design/HistoryStore/README.md).

Tests: `OoBDev.History.Tests` (`Unit`): one shared suite runs against both providers.
