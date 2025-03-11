
--INSERT INTO [embedding].[HashPlanes]([Value])
--SELECT TOP 16
--	[embedding].[UniformF](384, -1, 1, CHECKSUM(NEWID()))
--FROM sys.columns

--INSERT INTO [embedding].[HashPlaneSets]
--           ([HashPlaneID]
--           ,[HashSetID]
--           ,[Position])
-- SELECT 
--	 [HashPlaneID]
--	,1
--	,ROW_NUMBER() OVER (ORDER BY [HashPlaneID])
-- FROM [embedding].[HashPlanes]

--INSERT INTO [embedding].[HashPlanes]([Value])
--SELECT TOP 16
--	[embedding].[RandomF](384, CHECKSUM(NEWID()))
--FROM sys.columns

--INSERT INTO [embedding].[HashPlaneSets]
--           ([HashPlaneID]
--           ,[HashSetID]
--           ,[Position])
-- SELECT 
--	 [HashPlaneID]
--	,2
--	,ROW_NUMBER() OVER (ORDER BY [HashPlaneID])
-- FROM [embedding].[HashPlanes]
-- WHERE [HashPlaneID] > 16

--INSERT INTO [embedding].[HashPlanes]([Value])
--SELECT TOP 16
--	[embedding].[UniformVF](
--		 (SELECT [embedding].[MinimumF]([Vectors].[Value]) FROM [embedding].[Vectors])
--		,(SELECT [embedding].[MaximumF]([Vectors].[Value]) FROM [embedding].[Vectors])
--		, CHECKSUM(NEWID()))
--FROM sys.columns
--INSERT INTO [embedding].[HashPlaneSets]
--           ([HashPlaneID]
--           ,[HashSetID]
--           ,[Position])
-- SELECT 
--	 [HashPlaneID]
--	,4
--	,ROW_NUMBER() OVER (ORDER BY [HashPlaneID])
-- FROM [embedding].[HashPlanes]
-- WHERE [HashPlaneID] > 32


--SELECT 
--	CAST(CAST([embedding].[VectorToMatrixF]([Bounds].[Vector]) AS NVARCHAR(MAX)) AS XML)
--FROM (
--	SELECT [embedding].[MinimumF]([Vectors].[Value]) FROM [embedding].[Vectors]
--	UNION ALL
--	SELECT [embedding].[MaximumF]([Vectors].[Value]) FROM [embedding].[Vectors]
--) [Bounds]([Vector])

INSERT INTO [embedding].[Hashes](
	 [HashSetID]
	,[VectorID]
	,[Hash]
)
SELECT
	 [HashPlaneSets].[HashSetID]
	,[Vectors].[VectorID]
	,SUM(CASE 
		WHEN [HashPlanes].[Value].DotProduct([Vectors].[Value]) < 0 THEN 0 
		ELSE POWER(2, [HashPlaneSets].[Position]-1) 
		END) AS [Hash]
FROM [embedding].[HashPlaneSets]
INNER JOIN [embedding].[HashPlanes]
	ON [HashPlanes].[HashPlaneID] = [HashPlaneSets].[HashPlaneID]
INNER JOIN [embedding].[Vectors]
	ON [Vectors].[Value].Length() = [HashPlanes].[Value].Length()
WHERE 
	NOT EXISTS (
		SELECT *
		FROM [embedding].[Hashes]
		WHERE 
			    [Hashes].[HashSetID]	= [HashPlaneSets].[HashSetID]
			AND [Hashes].[VectorID]		= [Vectors].[VectorID]
	)
GROUP BY
	 [Vectors].[VectorID]
	,[HashPlaneSets].[HashSetID]
    
--SELECT
--	 --[Hashes].[VectorID]
--	 COUNT(*) AS [collisions]
--    ,[Hashes].[Hash]
--    ,[Hashes].[HashSetID]
--FROM [embedding].[Hashes]
--GROUP BY
--     [Hashes].[Hash]
--    ,[Hashes].[HashSetID]
--ORDER BY
--     [Hashes].[HashSetID]
--    ,[Hashes].[Hash]
