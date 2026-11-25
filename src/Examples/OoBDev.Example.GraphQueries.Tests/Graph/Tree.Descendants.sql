-- All descendants of CEO (children, grandchildren, ...). The depth limit keeps a cycle from running forever. PostgreSQL and SQLite.
WITH RECURSIVE below (id, depth) AS (
    SELECT c.id, 1 FROM node c JOIN node p ON c.parent_id = p.id WHERE p.name = 'CEO'
    UNION ALL
    SELECT c.id, b.depth + 1 FROM below b JOIN node c ON c.parent_id = b.id WHERE b.depth < (SELECT COUNT(*) FROM node)
)
SELECT DISTINCT n.name FROM below b JOIN node n ON n.id = b.id ORDER BY n.name;
