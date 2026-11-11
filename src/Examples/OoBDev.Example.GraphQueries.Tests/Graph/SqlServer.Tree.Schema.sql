-- The parent/child tree of Tree.Schema.sql for SQL Server.
CREATE TABLE dbo.Node (Id INT NOT NULL PRIMARY KEY, ParentId INT NULL, Name NVARCHAR(50) NOT NULL);
GO
INSERT INTO dbo.Node (Id, ParentId, Name) VALUES (1, NULL, N'CEO'), (2, 1, N'VP'), (3, 2, N'Dev'), (4, 2, N'Ops'), (5, 3, N'Intern'), (6, 7, N'A'), (7, 8, N'B'), (8, 6, N'C');
GO
