
INSERT INTO [embedding].[HashPlanes]([Value])
SELECT TOP (10000) 
	[embedding].[UniformVF](
		[embedding].[MinimumF]([Vectors].[Value])
		,[embedding].[MaximumF]([Vectors].[Value])
		,[cols].[check]
		)
FROM [embedding].[Vectors]
CROSS JOIN (	
	SELECT TOP 64
		CHECKSUM(*)
	FROM sys.columns
) AS [cols]([check])
GROUP BY
	[cols].[check]
