CREATE TABLE [embedding].[Hashes] (
    [VectorID]  BIGINT     NOT NULL,
    [Hash]      INT        NOT NULL,
    [HashSetID] INT        NOT NULL,
    CONSTRAINT [FK_Hashes_HashSets] 
        FOREIGN KEY ([HashSetID]) REFERENCES [embedding].[HashSets] ([HashSetID]),
    CONSTRAINT [FK_Hashes_Vectors] 
        FOREIGN KEY ([VectorID]) REFERENCES [embedding].[Vectors] ([VectorID])
);
GO

CREATE NONCLUSTERED INDEX [IX_Hashes_HashSet_Hash]
    ON [embedding].[Hashes]([HashSetID] ASC, [Hash] ASC);
GO

CREATE NONCLUSTERED INDEX [IX_Hashes_Vector]
    ON [embedding].[Hashes]([VectorID] ASC);
GO