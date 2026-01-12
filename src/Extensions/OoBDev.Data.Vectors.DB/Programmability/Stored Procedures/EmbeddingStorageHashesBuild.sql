CREATE PROCEDURE [embedding].[oobdev://embedding/storage/hashes/build]
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO [embedding].[Hashes](
		 [HashSetID]
		,[VectorID]
		,[Hash]
	)
	SELECT
		 [HashPlaneSets].[HashSetID]
		,[Vectors].[VectorID]
		,SUM(CASE
			WHEN CAST([HashPlanes].[Value] AS [embedding].[VectorF]).DotProduct(CAST([Vectors].[Value] AS [embedding].[VectorF])) < 0 THEN 0
			ELSE POWER(2, [HashPlaneSets].[Position]-1)
			END) AS [Hash]
	FROM [embedding].[HashPlaneSets]
	INNER JOIN [embedding].[HashPlanes]
		ON [HashPlanes].[HashPlaneID] = [HashPlaneSets].[HashPlaneID]
	INNER JOIN [embedding].[Vectors]
		ON CAST([Vectors].[Value] AS [embedding].[VectorF]).Length() = CAST([HashPlanes].[Value] AS [embedding].[VectorF]).Length()
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
		,[HashPlaneSets].[HashSetID];

END
