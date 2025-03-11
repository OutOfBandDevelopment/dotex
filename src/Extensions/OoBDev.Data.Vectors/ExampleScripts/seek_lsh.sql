DECLARE @find INT = 2;

WITH [$seek] AS (
	SELECT 
		 [Hashes].[VectorID]
		,[AlignedHash].[VectorID] AS [VectorID_Haystack]
		,COUNT(*) AS [CollisionCount]
		,ROW_NUMBER() OVER (
			PARTITION BY 
				[Hashes].[VectorID]
			ORDER BY 
				COUNT(*) DESC
			) AS [SeekOrder]
	FROM [embedding].[Hashes]
	INNER JOIN [embedding].[Hashes] AS [AlignedHash]
		ON  [AlignedHash].[HashSetID]	 = [Hashes].[HashSetID]
		AND [AlignedHash].[Hash]		 = [Hashes].[Hash]
		AND [AlignedHash].[VectorID]	!= [Hashes].[VectorID]
	WHERE 
		 [Hashes].[VectorID] = @find
	GROUP BY
		 [Hashes].[VectorID]
		,[AlignedHash].[VectorID]
)
	SELECT 
		[$Seek].*
		,[Needle].[OriginalID]
		,[Haystack].[OriginalID]						AS [OriginalID_Haystack]
		,[Needle].[Value].[Cosine]([Haystack].[Value])	AS [CosineDistance]
		,[NeedleValue].[NameValue]						AS [NeedleValue]
		,[HaystackValue].[NameValue]					AS [HaystackValue]
	FROM [$Seek]
	INNER JOIN [embedding].[Vectors] AS [Needle]
		ON [Needle].[VectorID] = [$Seek].[VectorID]
	INNER JOIN [embedding].[Vectors] AS [Haystack]
		ON [Haystack].[VectorID] = [$Seek].[VectorID_Haystack]
	INNER JOIN [dbo].[Names] AS [NeedleValue]
		ON [NeedleValue].[NameID] = [Needle].[OriginalID]
	INNER JOIN [dbo].[Names] AS [HaystackValue]
		ON [HaystackValue].[NameID] = [Haystack].[OriginalID]
	ORDER BY
		 [CosineDistance]
		,[$Seek].[SeekOrder]
