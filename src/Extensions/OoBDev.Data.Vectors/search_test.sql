

DECLARE  @target [vector].[VectorF] = (SELECT [vector].[RandomF](384, CHECKSUM(NEWID())))
		,@threshold REAL = 0.25
		,@maxSeek INT = 7;

WITH [$searchOrder] AS (
	SELECT 
		[Centroids].[id] AS [CentroidID]
		,@target.Cosine([Centroids].[Centroid]) AS [Distance]
		,[Centroids].[Length]
		,[Centroids].[RootDistance]
		,ROW_NUMBER() OVER ( 
			ORDER BY
				@target.Cosine([Centroids].[Centroid])
			) AS [SearchOrder]
	FROM [dbo].[Centroids]
	WHERE
		[Centroids].[Length] = @target.Length()
)
	SELECT 
		[Embeddings].*
		,@target.Cosine([Embeddings].[Vector]) AS [Distance]
		,[$searchOrder].*
	FROM [dbo].[Embeddings]
	INNER JOIN [$searchOrder]
		ON [$searchOrder].[CentroidID] = [Embeddings].[CentroidId]
		AND [$searchOrder].[Length] = [Embeddings].[Length]
	WHERE 
		[Embeddings].[Length] = @target.Length()
		AND @target.Cosine([Embeddings].[Vector]) < @threshold
		AND [$searchOrder].[SearchOrder] <= @maxSeek
	ORDER BY
		@target.Cosine([Embeddings].[Vector])
		
		--@target.Cosine([Embeddings].[Vector]) < @threshold
		
