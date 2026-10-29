CREATE TABLE [embedding].[HashPlanes] (
    [HashPlaneID] INT                  IDENTITY (1, 1) NOT NULL,
    [Value]       [embedding].[VectorF] NOT NULL,
    [Length]      AS                   (CONVERT([embedding].[VectorF],[Value]).Length()) PERSISTED,
    [Magnitude]   AS                   (CONVERT([embedding].[VectorF],[Value]).Magnitude()) PERSISTED,
    CONSTRAINT [PK_HashPlanes] PRIMARY KEY CLUSTERED ([HashPlaneID] ASC)
);
