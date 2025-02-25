CREATE TABLE [dbo].[Centroids](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Centroid] [vector].[VectorF] NOT NULL,
	[Length]  AS ([vector].[LengthF]([Centroid])) PERSISTED,
	[Magnitude]  AS ([vector].[MagnitudeF]([Centroid])) PERSISTED,
	[ParentId] [int] NULL,
	[ParentAngle] [real] NULL,
	[ParentDistance] [real] NULL,
	[RootAngle] [real] NULL,
	[RootDistance] [real] NULL,
	[TimeStamp]  AS (getdate()),
 CONSTRAINT [PK_Centroids] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO