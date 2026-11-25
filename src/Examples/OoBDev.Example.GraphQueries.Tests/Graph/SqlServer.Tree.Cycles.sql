-- Nodes that are their own ancestor. Walk up from every dbo.Node and report the ones that arrive back at the start. SQL Server (no RECURSIVE keyword; the recursive part cannot hold an aggregate, so the depth limit is a constant).
WITH up (start_id, Id, depth) AS (
    SELECT n.Id, n.ParentId, 1 FROM dbo.Node n WHERE n.ParentId IS NOT NULL
    UNION ALL
    SELECT u.start_id, n.ParentId, u.depth + 1 FROM up u JOIN dbo.Node n ON n.Id = u.Id
    WHERE n.ParentId IS NOT NULL AND u.Id <> u.start_id AND u.depth < 20
)
SELECT DISTINCT n.Name FROM up u JOIN dbo.Node n ON n.Id = u.start_id WHERE u.Id = u.start_id ORDER BY n.Name;
