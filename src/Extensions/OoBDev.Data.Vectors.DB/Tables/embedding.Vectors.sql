CREATE TABLE [embedding].[Vectors] (
    [VectorID]   BIGINT                IDENTITY (1, 1) NOT NULL,
    [OriginalID] BIGINT                NOT NULL,
    [Value]      [embedding].[VectorF] NOT NULL,
    [Length]     AS                    (CAST([Value] AS [embedding].[VectorF]).[Length]()) PERSISTED,
    [Magnitude]  AS                    (CAST([Value] AS [embedding].[VectorF]).[Magnitude]()) PERSISTED,
    [SourceID]   INT                   NOT NULL,
    CONSTRAINT [PK_Vectors] PRIMARY KEY CLUSTERED ([VectorID] ASC),
    CONSTRAINT [FK_Vectors_Sources]
        FOREIGN KEY ([SourceID]) REFERENCES [embedding].[Sources] ([SourceID])
);