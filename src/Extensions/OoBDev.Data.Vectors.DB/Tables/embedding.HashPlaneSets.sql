CREATE TABLE [embedding].[HashPlaneSets] (
    [HashPlaneSetID] INT IDENTITY (1, 1) NOT NULL,
    [HashPlaneID]    INT NOT NULL,
    CONSTRAINT [PK_HashPlaneSets] PRIMARY KEY CLUSTERED ([HashPlaneSetID] ASC),
    CONSTRAINT [FK_HashPlaneSets_HashPlane] 
        FOREIGN KEY ([HashPlaneID]) REFERENCES [embedding].[HashPlanes] ([HashPlaneID])
);
