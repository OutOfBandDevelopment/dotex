-- Who is reachable from Ann in one or two steps? Repeat the pattern per step and take the distinct names.
SELECT DISTINCT p2.Name
FROM dbo.Person p1, dbo.Knows k1, dbo.Person p2
WHERE MATCH(p1-(k1)->p2) AND p1.Name = N'Ann'
UNION
SELECT DISTINCT p3.Name
FROM dbo.Person p1, dbo.Knows k1, dbo.Person p2, dbo.Knows k2, dbo.Person p3
WHERE MATCH(p1-(k1)->p2-(k2)->p3) AND p1.Name = N'Ann' AND p3.Name <> N'Ann'
ORDER BY Name;
