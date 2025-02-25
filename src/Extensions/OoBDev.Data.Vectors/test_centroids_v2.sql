/*
TRUNCATE TABLE [dbo].[Centroids];
UPDATE [dbo].[Embeddings] SET [CentroidID] = NULL;
*/

DECLARE @batches INT = 10,
		@maxBatch INT = 5000;


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

DROP TABLE IF EXISTS [#temp];
CREATE TABLE [#temp] (
	[ParentID] INT 
);
WITH [$set] AS (
	SELECT  
		NTILE(@batches) OVER (
			PARTITION BY
				 [Embeddings].[Length]
				,[Embeddings].[CentroidId]
			ORDER BY
				DEGREES([Embeddings].[Vector].[Angle]([Centroids].[Centroid]))
		) AS [Set]
		,COUNT([Embeddings].[ID]) OVER (
			PARTITION BY
				 [Embeddings].[Length]
				,[Embeddings].[CentroidId]
		) AS [Size]
		,[Embeddings].[Vector]
		,[Embeddings].[Length]
		,[Embeddings].[CentroidId] AS [ParentID]
	FROM [dbo].[Embeddings]
	INNER JOIN [dbo].[Centroids]
		ON [Centroids].[id] = [Embeddings].[CentroidId]
)
	INSERT INTO [dbo].[Centroids] (
		[Centroid]
		,[ParentID]
		,[ParentAngle]
		,[ParentDistance]
	)
	OUTPUT (inserted.id) INTO [#temp] ([ParentID])
	SELECT 
		 [vector].[CentroidF]([$set].[Vector])
		,[$set].[ParentID]
		,[vector].[CentroidF]([$set].[Vector]).[Angle]([Centroids].[Centroid])
		,[vector].[CentroidF]([$set].[Vector]).[Cosine]([Centroids].[Centroid])
	FROM [$set]
	INNER JOIN [dbo].[Centroids]
		ON [Centroids].[Length] = [$set].[Length]
			AND [Centroids].[id] = [$set].[ParentID]
	GROUP BY 
		[$set].[ParentID]
		,[Centroids].[Centroid]
	HAVING 
		COUNT(*) > @maxBatch;

UPDATE [dbo].[Embeddings]
SET [CentroidID] = NULL
FROM [dbo].[Embeddings]
INNER JOIN [dbo].[Centroids]
	ON [Embeddings].[CentroidID] = [Centroids].[parentid]
INNER JOIN [#temp]
	ON [Centroids].[id] = [#temp].[ParentID];


WITH [$Matched] AS (
	SELECT 
		 [Embeddings].[ID]											AS [EmbeddingId]
		,[Centroids].[id]											AS [CentroidId]
		,[Centroids].[TimeStamp]									AS [CentroidTimestamp]
		,ROW_NUMBER() OVER (
			PARTITION BY [Embeddings].[ID]
			ORDER BY [Embeddings].[Vector].[Cosine]([Centroids].[Centroid])
		)															AS [ByCentroidDistance]
		,[Embeddings].[Vector].[Angle]([Centroids].[Centroid])		AS [Angle_Radians]
		,[Embeddings].[Vector].[Cosine]([Centroids].[Centroid])	AS [Distance]
	FROM [dbo].[Embeddings]
	INNER JOIN [dbo].[Centroids]
		ON [Centroids].[Length] = [Embeddings].[Length]
	WHERE 
		[Embeddings].[CentroidId] IS NULL -- Not Linked to Tree
)
	UPDATE [dbo].[Embeddings]
	SET  [CentroidId]			= [$Matched].[CentroidId]
		,[CentroidTimestamp]	= [$Matched].[CentroidTimestamp]
	--SELECT *
	FROM [dbo].[Embeddings]
	INNER JOIN [$Matched]
		ON [Embeddings].[ID] = [$Matched].[EmbeddingId]
	WHERE
		[$Matched].[ByCentroidDistance] = 1
		AND ([Embeddings].[CentroidId] != [$Matched].[CentroidId] OR [Embeddings].[CentroidId]  IS NULL)
		;

UPDATE [dbo].[Centroids]
SET [RootAngle] = [Centroids].[Centroid].[Angle]([Roots].[Centroid])
	,[RootDistance] = [Centroids].[Centroid].[Cosine]([Roots].[Centroid])	
FROM [dbo].[Centroids]
INNER JOIN [test1].[dbo].[Centroids] AS [Roots]
	ON [Roots].[ParentId] IS NULL
		AND [Roots].[Length] = [Centroids].[Length]
WHERE 
	[Centroids].[ParentId] IS NOT NULL
	AND [Centroids].[RootAngle] IS NULL

--UPDATE [dbo].[Centroids] SET [RootAngle] = NULL
-- TRUNCATE TABLE [dbo].[Centroids]


/*    
SELECT 
	[Embeddings].[CentroidId]
	,COUNT(*)
FROM [Embeddings]
GROUP BY
	[Embeddings].[CentroidId]
*/