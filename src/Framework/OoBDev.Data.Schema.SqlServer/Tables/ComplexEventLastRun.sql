CREATE TABLE [ComplexEventLastRun] (
    [Key] nvarchar(200) NOT NULL,
    [LastRunTicks] bigint NOT NULL,
    CONSTRAINT [PK_ComplexEventLastRun] PRIMARY KEY ([Key])
);
GO
