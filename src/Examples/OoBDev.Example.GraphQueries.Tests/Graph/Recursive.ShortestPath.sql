-- Shortest path from Ann to Dan. The path string guards against cycles; breadth first by depth, shortest wins. Works unchanged on PostgreSQL and SQLite.
WITH RECURSIVE walk (id, depth, path) AS (
    SELECT p.id, 0, ',' || CAST(p.id AS TEXT) || ',' FROM person p WHERE p.name = 'Ann'
    UNION ALL
    SELECT k.to_id, w.depth + 1, w.path || CAST(k.to_id AS TEXT) || ','
    FROM walk w JOIN knows k ON k.from_id = w.id
    WHERE w.path NOT LIKE '%,' || CAST(k.to_id AS TEXT) || ',%'
)
SELECT w.path, w.depth
FROM walk w JOIN person p ON p.id = w.id
WHERE p.name = 'Dan'
ORDER BY w.depth, w.path
LIMIT 1;
