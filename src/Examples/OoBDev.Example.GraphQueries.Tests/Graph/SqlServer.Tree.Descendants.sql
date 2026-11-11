-- All descendants of CEO (children, grandchildren, ...). The depth limit keeps a cycle from running forever. SQL Server (no RECURSIVE keyword; the recursive part cannot hold an aggregate, so the depth limit is a constant).
WITH below (Id, depth) AS (
    SELECT c.Id, 1 FROM dbo.Node c JOIN dbo.Node p ON c.ParentId = p.Id WHERE p.Name = N'CEO'
    UNION ALL
    SELECT c.Id, b.depth + 1 FROM below b JOIN dbo.Node c ON c.ParentId = b.Id WHERE b.depth < 20
)
SELECT DISTINCT n.Name FROM below b JOIN dbo.Node n ON n.Id = b.Id ORDER BY n.Name;
