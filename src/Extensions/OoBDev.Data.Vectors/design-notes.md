

```plantuml
@startuml
@enduml
```

## hamming distance table

```sql
USE [ExampleDb]
GO

/****** Object:  Table [dbo].[HammingDistances]    Script Date: 6/18/2025 8:25:25 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[HammingDistances](
	[HammingId] [int] IDENTITY(1,1) NOT NULL,
	[Distance] [int] NOT NULL,
	[Mask] [int] NOT NULL,
 CONSTRAINT [PK_HammingDistances] PRIMARY KEY CLUSTERED 
(
	[HammingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
```



## hash search

```sql


--	VectorID	Hash	HashSetID
--	1			43682	6
DECLARE @needle INT = 35490;
DECLARE @hashDistance INT = 5;
DECLARE @vector [embedding].[VectorF] = (
	SELECT 
		[Vectors].[Value]
	FROM [embedding].[Vectors]
	WHERE 
		[Vectors].[VectorID] = 1
)
DECLARE @target NVARCHAR(200) = (
	SELECT 
		[Names].[NameValue]
	FROM [embedding].[Vectors]
	INNER JOIN [dbo].[Names]
		ON [Names].[NameID] = [Vectors].[OriginalID]
	INNER JOIN [embedding].[Sources]
		ON [Sources].[SourceID] =  [Vectors].[SourceID]
	WHERE 
		[Vectors].[VectorID] = 1
		AND [Sources].[Name] = '[dbo].[Names]'
)

--SELECT 
--	@target,
--	@needle,
--	@vector


SELECT 
	 [HammingHashes].[Hash]					AS [HammingHash]
	,[HammingHashes].[Distance]				AS [HammingDistance]

	,[Vectors].[Value].[Cosine](@vector)	AS [CosineDistance]
	,[Names].[NameValue]
FROM (
	SELECT 
		 [HammingDistances].[Mask] ^ 35490	AS [Hash]
		,MIN([HammingDistances].[Distance]) AS [Distance]
	FROM [HammingDistances]
	WHERE 
		[HammingDistances].[Distance] <= @hashDistance
	GROUP BY
		[HammingDistances].[Mask] ^ 35490
) AS [HammingHashes]
INNER JOIN [embedding].[Hashes]
	ON [Hashes].[Hash] = [HammingHashes].[Hash]
		AND [Hashes].[HashSetID] = 6 

INNER JOIN [embedding].[Vectors]
	ON [Vectors].[VectorID] = [Hashes].[VectorID]
INNER JOIN [dbo].[Names]
	ON [Names].[NameID] = [Vectors].[OriginalID]
INNER JOIN [embedding].[Sources]
	ON [Sources].[SourceID] =  [Vectors].[SourceID]

ORDER BY	
	[CosineDistance]


```