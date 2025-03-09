CREATE TABLE [embedding].[HashSets] (
    [HashSetID]      INT           NOT NULL,
    [DistanceMetric] NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_HashSets] PRIMARY KEY CLUSTERED ([HashSetID] ASC)
);