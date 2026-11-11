-- Who is reachable from Ann in one or two steps? Works unchanged on PostgreSQL and SQLite.
WITH RECURSIVE walk (id, depth) AS (
    SELECT k.to_id, 1 FROM knows k JOIN person p ON p.id = k.from_id WHERE p.name = 'Ann'
    UNION ALL
    SELECT k.to_id, w.depth + 1 FROM walk w JOIN knows k ON k.from_id = w.id WHERE w.depth < 2
)
SELECT DISTINCT p.name
FROM walk w JOIN person p ON p.id = w.id
WHERE p.name <> 'Ann'
ORDER BY p.name;
