DECLARE @vector VECTOR = '[1,2,3,4]';
DECLARE @vector2 VECTOR = '[4,3,2,1]';
DECLARE @vector3 VECTOR = '[2,1,3,5]';

SELECT 
	@vector.Element(0) AS Element,
	@vector.Distance(@vector2, 'dot') AS Distance,
	@vector.DotProduct(@vector2) AS DotProduct,
	@vector.Cosine(@vector2) AS Cosine,
	@vector.Similarity(@vector2) AS Similarity,
	@vector.Euclidean(@vector2) AS Euclidean,
	@vector.Manhattan(@vector2) AS Manhattan,
	@vector.Angle(@vector2) AS Angle_InRadians,
	(@vector.Angle(@vector2) * 180)/ PI() AS Angle_Degrees,
	'x';
	
SELECT 
	CAST([dbo].[Centroid]([vector]) AS NVARCHAR(MAX))
FROM (VALUES 
	(@vector),
	(@vector2),
	(@vector3)
) AS [vectors](vector)
