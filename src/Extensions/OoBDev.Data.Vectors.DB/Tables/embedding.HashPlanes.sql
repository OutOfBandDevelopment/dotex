CREATE TABLE [embedding].[HashPlanes] (
    [HashPlaneID] INT                  IDENTITY (1, 1) NOT NULL,
    [Value]       [embedding].[VectorF] NOT NULL,
    [Length]      AS                   ([Value].[Length]()) PERSISTED,
    [Magnitude]   AS                   ([Value].[Magnitude]()) PERSISTED,
    CONSTRAINT [PK_HashPlanes] PRIMARY KEY CLUSTERED ([HashPlaneID] ASC)
);
