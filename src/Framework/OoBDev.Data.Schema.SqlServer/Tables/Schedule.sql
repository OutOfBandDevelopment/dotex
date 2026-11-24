CREATE TABLE [ComplexEventLastRun] (
    [Key] nvarchar(200) NOT NULL,
    [LastRunTicks] bigint NOT NULL,
    CONSTRAINT [PK_ComplexEventLastRun] PRIMARY KEY ([Key])
);
GO

CREATE TABLE [ComplexEventSchedule] (
    [Id] uniqueidentifier NOT NULL,
    [Key] nvarchar(200) NOT NULL,
    [DueAtTicks] bigint NOT NULL,
    [EventType] nvarchar(200) NOT NULL,
    [Payload] varbinary(max) NOT NULL,
    [Attempts] int NOT NULL,
    [Failed] bit NOT NULL,
    [LeasedUntilTicks] bigint NOT NULL,
    CONSTRAINT [PK_ComplexEventSchedule] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_ComplexEventSchedule_Failed_DueAtTicks] ON [ComplexEventSchedule] ([Failed], [DueAtTicks]);
GO

CREATE UNIQUE INDEX [IX_ComplexEventSchedule_Key] ON [ComplexEventSchedule] ([Key]);
GO
