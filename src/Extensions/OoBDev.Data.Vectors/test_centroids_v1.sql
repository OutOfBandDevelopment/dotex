
-- Build Index

-- Seed missing centroids for given length
INSERT INTO [dbo].[Centroids]([Centroid])
SELECT 
	[vector].[CentroidF]([Embeddings].[Vector])
FROM [dbo].[Embeddings]
LEFT JOIN [dbo].[Centroids]
	ON  [Centroids].[Length] = [Embeddings].[Length]
	AND [Centroids].[ParentId] IS NULL
WHERE 
		[Embeddings].[CentroidId] IS NULL
	AND [Centroids].[id] IS NULL
GROUP BY
	[Embeddings].[Length];

-- Fill missing indexes

WITH [$Matched] AS (
	SELECT 
		 [Embeddings].[ID]											AS [EmbeddingId]
		,[Centroids].[id]											AS [CentroidId]
		,[Centroids].[TimeStamp]									AS [CentroidTimestamp]
		,ROW_NUMBER() OVER (
			PARTITION BY [Embeddings].[ID]
			ORDER BY [Embeddings].[Vector].[Euclidean]([Centroids].[Centroid])
		)															AS [ByCentroidDistance]
		,[Embeddings].[Vector].[Angle]([Centroids].[Centroid])		AS [Angle_Radians]
		,[Embeddings].[Vector].[Euclidean]([Centroids].[Centroid])	AS [Distance]
	FROM [dbo].[Embeddings]
	INNER JOIN [dbo].[Centroids]
		ON [Centroids].[Length] = [Embeddings].[Length]
	WHERE 
		[Embeddings].[CentroidId] IS NULL -- Not Linked to Tree
)
	UPDATE [dbo].[Embeddings]
	SET  [CentroidId]			= [$Matched].[CentroidId]
		,[CentroidTimestamp]	= [$Matched].[CentroidTimestamp]
	FROM [dbo].[Embeddings]
	INNER JOIN [$Matched]
		ON [Embeddings].[ID] = [$Matched].[EmbeddingId]
	WHERE
		[$Matched].[ByCentroidDistance] = 1;

-- Split large branches

DECLARE @maxChildren INT = 1000;

DROP TABLE IF EXISTS [#splits];
CREATE TABLE [#splits] (
	 [ParentID]		INT
	,[CentroidId]	INT
	,[Timestamp]	DATETIME
);
WITH [$Children] AS (
	SELECT 
		[Embeddings].[ID]												AS [EmbeddingId]
		,[Centroids].[id]												AS [CentroidId]
		,ROW_NUMBER() OVER (
			PARTITION BY [Centroids].[ID]
			ORDER BY [Embeddings].[Vector].[Euclidean]([Centroids].[Centroid])
		)																AS [ByCentroidDistance]
		,ROW_NUMBER() OVER (
			PARTITION BY [Centroids].[ID]
			ORDER BY [Embeddings].[Vector].[Euclidean]([Centroids].[Centroid]) DESC
		)																AS [ByCentroidDistanceDesc]
	FROM [dbo].[Embeddings]
	INNER JOIN (
		SELECT 
			 [Centroids].[id]
		FROM [dbo].[Centroids]
		INNER JOIN [dbo].[Embeddings] 
			ON [Embeddings].[CentroidId] = [Centroids].[id]
			AND [Centroids].[Length] = [Embeddings].[Length]
		GROUP BY
			[Centroids].[id]
		HAVING COUNT(*) > @maxChildren
	) AS [$Centroids]
		ON [$Centroids].[id] = [Embeddings].[CentroidId]
	INNER JOIN [dbo].[Centroids]
		ON [Centroids].[id] = [$Centroids].[id]
), [$NewCentroids] AS (
	SELECT 
		[Centroids].[id]												AS [ParentCentroidId]
		,[Centroids].[Centroid]											AS [ParentCentroid]
		,[Centroids].[Length]
		,[Centroids].[Centroid].[Midpoint]([Embeddings].[Vector])		AS [Centroid]
	FROM (
		SELECT DISTINCT
			 [$Children].[EmbeddingId]
			,[$Children].[CentroidId]
		FROM [$Children]
		WHERE 
				[$Children].[ByCentroidDistance] = 1 
			OR  [$Children].[ByCentroidDistanceDesc] = 1
	) AS [$Children]
	INNER JOIN [dbo].[Embeddings]
		ON	[Embeddings].[ID] = [$Children].[EmbeddingId]
	INNER JOIN [dbo].[Centroids]
		ON	[Centroids].[id] = [$Children].[CentroidId]
), [$roots] AS (
	SELECT 
		 [Centroids].[id]												AS [RootCentroidId]
		,[Centroids].[Centroid]											AS [RootCentroid]
		,[Centroids].[Length]
		,ROW_NUMBER() OVER (ORDER BY [Timestamp])						AS [Order]
	FROM [dbo].[Centroids]
	WHERE
		[Centroids].[ParentId] IS NULL
)
	INSERT INTO [dbo].[Centroids] (
		 [Centroid]
		,[ParentId]
		,[ParentAngle]
		,[ParentDistance]
		,[RootAngle]
		,[RootDistance]
	)
	OUTPUT 
		 inserted.[ParentID]
		,inserted.[id]
		,inserted.[Timestamp]
		INTO [#splits]
	SELECT
		[$NewCentroids].[Centroid]
		,[$NewCentroids].[ParentCentroidId]
		,[$NewCentroids].[Centroid].[Angle]([$NewCentroids].[ParentCentroid])		AS [ParentAngle]
		,[$NewCentroids].[Centroid].[Euclidean]([$NewCentroids].[ParentCentroid])	AS [ParentDistance]
		,[$NewCentroids].[Centroid].[Angle]([$roots].[RootCentroid])				AS [RootAngle]
		,[$NewCentroids].[Centroid].[Euclidean]([$roots].[RootCentroid])			AS [RootDistance]
	FROM [$NewCentroids]
	INNER JOIN [$roots]
		ON  [$roots].[Length] = [$NewCentroids].[Length]
		AND [$roots].[Order] = 1;

-- clear 
--UPDATE [dbo].[Embeddings]
--SET [CentroidId] = NULL

WITH [$possible] AS (
	SELECT 
		 [Embeddings].[ID]											AS [EmbeddingId]
		,[Embeddings].[CentroidId]									AS [CurrentCentroidId]
		,[Centroids].[id]											AS [CentroidId]
		,[Centroids].[TimeStamp]									AS [CentroidTimestamp]
		,ROW_NUMBER() OVER (
			PARTITION BY [Embeddings].[ID]
			ORDER BY [Embeddings].[Vector].[Euclidean]([Centroids].[Centroid])
		)															AS [ByCentroidDistance]
		,[Embeddings].[Vector].[Angle]([Centroids].[Centroid])		AS [Angle_Radians]
		,[Embeddings].[Vector].[Euclidean]([Centroids].[Centroid])	AS [Distance]
	FROM [dbo].[Embeddings]
	INNER JOIN [dbo].[Centroids]
		ON [Centroids].[Length] = [Embeddings].[Length]
		AND [Centroids].[id] IN (
			SELECT DISTINCT
				[#splits].[ParentID]
			FROM [#splits]
			UNION ALL
			SELECT DISTINCT
				[#splits].[CentroidId]
			FROM [#splits]
		)
)
	SELECT *
	FROM [$possible]
	WHERE 
		[ByCentroidDistance] = 1;
	