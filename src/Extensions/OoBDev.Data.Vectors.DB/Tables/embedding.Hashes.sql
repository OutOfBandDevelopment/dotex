CREATE TABLE [embedding].[Hashes] (
    [VectorID]       BIGINT NOT NULL,
    [HashPlaneSetID] INT    NOT NULL,
    [Hash]           INT    NOT NULL,
    CONSTRAINT [FK_Hashes_Vectors] 
        FOREIGN KEY ([VectorID]) REFERENCES [embedding].[Vectors] ([VectorID]),
    CONSTRAINT [FK_Hashes_HashPlaneSets] 
        FOREIGN KEY ([HashPlaneSetID]) REFERENCES [embedding].[HashPlaneSets] ([HashPlaneSetID])
);