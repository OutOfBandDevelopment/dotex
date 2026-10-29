# Vector search latency gate (2026-10-29)

Integration test `SqlVectorStore_FindNeighbors_StaysUnderTheLatencyGate` (`OoBDev.Data.Vectors.DB.Tests`) stores 1000 vectors of 384 dimensions in a throwaway database on the Docker SQL Server and runs a nearest-neighbour query. It logs a warning over 500 ms and fails over 750 ms (testing rule), and checks that the query vector itself ranks first. Passed on the local stack.

Left in the backlog: a gate for database-backed (non-vector) endpoints.
