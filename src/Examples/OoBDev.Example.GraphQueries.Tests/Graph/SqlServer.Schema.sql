-- Social graph sample for SQL Server 2019+ (node and edge tables). Friendship is stored in both directions.
CREATE TABLE dbo.Person (
    PersonId INT NOT NULL PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL
) AS NODE;
GO
CREATE TABLE dbo.Knows AS EDGE;
GO
INSERT INTO dbo.Person (PersonId, Name) VALUES (1, N'Ann'), (2, N'Bob'), (3, N'Cat'), (4, N'Dan'), (5, N'Eve'), (6, N'Fay');
GO
INSERT INTO dbo.Knows ($from_id, $to_id)
SELECT a.$node_id, b.$node_id
FROM (VALUES (1, 2), (2, 3), (3, 4), (1, 5), (5, 4)) AS pair (FromId, ToId)
JOIN dbo.Person a ON a.PersonId = pair.FromId
JOIN dbo.Person b ON b.PersonId = pair.ToId
UNION ALL
SELECT b.$node_id, a.$node_id
FROM (VALUES (1, 2), (2, 3), (3, 4), (1, 5), (5, 4)) AS pair (FromId, ToId)
JOIN dbo.Person a ON a.PersonId = pair.FromId
JOIN dbo.Person b ON b.PersonId = pair.ToId;
GO
