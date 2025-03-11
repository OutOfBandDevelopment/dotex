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
		,[HashPlaneSets].[HashSetID];

END
