-- All ancestors of Intern, nearest first. SQL Server (no RECURSIVE keyword; the recursive part cannot hold an aggregate, so the depth limit is a constant).
WITH above (Id, depth) AS (
    SELECT n.ParentId, 1 FROM dbo.Node n WHERE n.Name = N'Intern' AND n.ParentId IS NOT NULL
    UNION ALL
    SELECT n.ParentId, a.depth + 1 FROM above a JOIN dbo.Node n ON n.Id = a.Id WHERE n.ParentId IS NOT NULL AND a.depth < 20
)
SELECT n.Name FROM above a JOIN dbo.Node n ON n.Id = a.Id ORDER BY a.depth;
