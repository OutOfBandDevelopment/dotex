CREATE TABLE [_Scheduler].[EventSchedules] (
    [EventScheduleId]  INT            IDENTITY (1, 1) NOT NULL,
    [EventGeneratorId] INT            NOT NULL,
    [Schedule]         NVARCHAR (200) NOT NULL,
    CONSTRAINT [PK_EventSchedules] PRIMARY KEY CLUSTERED ([EventScheduleId] ASC),
    CONSTRAINT [FK_EventSchedules_EventGenerators] FOREIGN KEY ([EventGeneratorId]) REFERENCES [_Scheduler].[EventGenerators] ([EventGeneratorId])
);

