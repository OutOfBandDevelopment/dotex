CREATE TABLE [embedding].[Sources] (
    [SourceID] INT            IDENTITY (1, 1) NOT NULL,
    [Name]     NVARCHAR (128) NOT NULL,
    CONSTRAINT [PK_Sources] PRIMARY KEY CLUSTERED ([SourceID] ASC)
);
