CREATE TABLE [embedding].[HashPlaneSets] (
    [HashPlaneSetID] INT IDENTITY (1, 1) NOT NULL,
    [HashPlaneID]    INT NOT NULL,
    [HashSetID]      INT NOT NULL,
    [Position]       INT NOT NULL,
    CONSTRAINT [PK_HashPlaneSets] PRIMARY KEY CLUSTERED ([HashPlaneSetID] ASC),
    CONSTRAINT [FK_HashPlaneSets_HashPlanes] 
        FOREIGN KEY ([HashPlaneID]) REFERENCES [embedding].[HashPlanes] ([HashPlaneID]),
    CONSTRAINT [FK_HashPlaneSets_HashSets] 
        FOREIGN KEY ([HashSetID]) REFERENCES [embedding].[HashSets] ([HashSetID])
);