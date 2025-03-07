
DECLARE @v1 embedding.VectorF = '1,2,3,4,5';
DECLARE @v2 embedding.VectorF = '2,3,4,5,6';

DECLARE @r REAL = 2;

SELECT CAST(@v1.Scale(@r) AS NVARCHAR(MAX));


WITH [$fakeValues] AS (
	SELECT 
		[embedding].[UniformF](5, 3,10, CHECKSUM(*)) AS [vector]
	FROM sys.columns
), [$spread] AS (
	SELECT 
		 CAST([embedding].[MinimumF]([$fakeValues].[Vector])	AS NVARCHAR(MAX)) AS [Minimim]
		,CAST([embedding].[UniformVF](
			  [embedding].[MinimumF]([$fakeValues].[Vector])
			 ,[embedding].[MaximumF]([$fakeValues].[Vector])
			,CHECKSUM(NEWID())
		)														AS NVARCHAR(MAX)) AS [Random]
		,CAST([embedding].[MaximumF]([$fakeValues].[Vector])	AS NVARCHAR(MAX)) AS [Maximum]
	FROM [$fakeValues]
), [$vectors] AS (
	SELECT 
		 [description]
		,[vector]
	FROM [$spread]
	UNPIVOT (
		[vector]
		FOR [description] IN (
			 [Minimim]
			,[Random]
			,[Maximum]
		)
	) [u]
)
	SELECT 
		CAST(
			[embedding].[VectorToMatrixF]([$vectors].[vector]) --.[Row](1)
			AS NVARCHAR(MAX))
	FROM [$vectors]

	--[embedding].[VectorToMatrixF]
--SELECT 
--	-- CAST(@v1 AS NVARCHAR(MAX)),
--	--CAST(@v2 AS NVARCHAR(MAX)),
--	CAST(
--		embedding.UniformVF(@v1, @v2, CHECKSUM(GETDATE()))
--		AS NVARCHAR(MAX))
