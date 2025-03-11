CREATE TABLE [embedding].[HashSets] (
    [HashSetID]      INT           IDENTITY (1, 1) NOT NULL,
    [DistanceMetric] NVARCHAR (20) NOT NULL,
    [Name]           NVARCHAR (50) NULL,
    CONSTRAINT [PK_HashSets] PRIMARY KEY CLUSTERED ([HashSetID] ASC)
);