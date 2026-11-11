-- Nodes that are their own ancestor. Walk up from every node and report the ones that arrive back at the start. PostgreSQL and SQLite.
WITH RECURSIVE up (start_id, id, depth) AS (
    SELECT n.id, n.parent_id, 1 FROM node n WHERE n.parent_id IS NOT NULL
    UNION ALL
    SELECT u.start_id, n.parent_id, u.depth + 1 FROM up u JOIN node n ON n.id = u.id
    WHERE n.parent_id IS NOT NULL AND u.id <> u.start_id AND u.depth < (SELECT COUNT(*) FROM node)
)
SELECT DISTINCT n.name FROM up u JOIN node n ON n.id = u.start_id WHERE u.id = u.start_id ORDER BY n.name;
