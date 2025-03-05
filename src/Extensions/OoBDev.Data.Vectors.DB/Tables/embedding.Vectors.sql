CREATE TABLE [embedding].[Vectors] (
    [VectorID]  BIGINT               IDENTITY (1, 1) NOT NULL,
    [Value]     [embedding].[Vector] NOT NULL,
    [Length]    AS                   ([Value].[Length]()) PERSISTED,
    [Magnitude] AS                   ([Value].[Magnitude]()) PERSISTED,
    [SourceID]  INT                  NOT NULL,
    CONSTRAINT [PK_Vectors] PRIMARY KEY CLUSTERED ([VectorID] ASC),
    CONSTRAINT [FK_Vectors_Source] 
        FOREIGN KEY ([SourceID]) REFERENCES [embedding].[Sources] ([SourceID])
);

