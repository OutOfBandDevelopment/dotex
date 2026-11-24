# OoBDev.History.Sqlite

SQLite provider for [OoBDev.History](../OoBDev.History/README.History.md): `SqliteHistoryStore.OpenAsync(connectionString, options)` creates the tables in WAL mode and serialises calls on one connection (SQLite has a single writer). Appends update the raw table and every tier bucket in one transaction (an upsert).

`SqliteSnapshot.CreateAsync(connection, path)` copies a running SQLite database with `VACUUM INTO` (a consistent copy, writers continue) and `SqliteSnapshot.VerifyAsync(path)` runs `PRAGMA integrity_check` on it. It works on any SQLite database, so the event store and the queue can use it too. Design: [HistoryStore](../../../docs/design/HistoryStore/README.md).
