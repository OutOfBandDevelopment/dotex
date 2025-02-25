CREATE TABLE [dbo].[Embeddings](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Vector] [vector].[VectorF] NULL,
	[Length]  AS ([vector].[LengthF]([Vector])) PERSISTED,
	[Magnitude]  AS ([vector].[MagnitudeF]([Vector])) PERSISTED,
	[CentroidId] [int] NULL,
	[CentroidTimeStamp] [datetime] NULL,
 CONSTRAINT [PK_Embeddings] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
