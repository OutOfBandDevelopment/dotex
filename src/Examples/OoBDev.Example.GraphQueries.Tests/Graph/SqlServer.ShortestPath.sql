-- Shortest path from Ann to Dan (SQL Server 2019 SHORTEST_PATH). The outer filter keeps the row that ends at Dan.
SELECT Path, Levels
FROM (
    SELECT
        STRING_AGG(p2.Name, N'>') WITHIN GROUP (GRAPH PATH) AS Path,
        LAST_VALUE(p2.Name) WITHIN GROUP (GRAPH PATH) AS LastName,
        COUNT(p2.Name) WITHIN GROUP (GRAPH PATH) AS Levels
    FROM dbo.Person p1, dbo.Person FOR PATH AS p2, dbo.Knows FOR PATH AS k
    WHERE MATCH(SHORTEST_PATH(p1(-(k)->p2)+)) AND p1.Name = N'Ann'
) AS paths
WHERE LastName = N'Dan';
