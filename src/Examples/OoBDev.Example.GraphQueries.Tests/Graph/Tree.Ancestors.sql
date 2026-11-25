-- All ancestors of Intern, nearest first. PostgreSQL and SQLite.
WITH RECURSIVE above (id, depth) AS (
    SELECT n.parent_id, 1 FROM node n WHERE n.name = 'Intern' AND n.parent_id IS NOT NULL
    UNION ALL
    SELECT n.parent_id, a.depth + 1 FROM above a JOIN node n ON n.id = a.id WHERE n.parent_id IS NOT NULL AND a.depth < (SELECT COUNT(*) FROM node)
)
SELECT n.name FROM above a JOIN node n ON n.id = a.id ORDER BY a.depth;
