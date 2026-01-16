CREATE SCHEMA [_Scheduler]
    AUTHORIZATION [dbo];
GO

CREATE TABLE [_Scheduler].[EventGenerators] (
    [EventGeneratorId]      INT                IDENTITY (1, 1) NOT NULL,
    [AssemblyQualifiedName] NVARCHAR (256)     NOT NULL,
    [OriginalSchedule]      NVARCHAR (200)     NOT NULL,
    [NextRun]               DATETIMEOFFSET (7) NULL,
    [LastComplete]          DATETIMEOFFSET (7) NULL,
    [Status]                NVARCHAR (50)      NULL,
    CONSTRAINT [PK_EventGenerators] PRIMARY KEY CLUSTERED ([EventGeneratorId] ASC)
);

CREATE TABLE [_Scheduler].[EventSchedules] (
    [EventScheduleId]  INT            IDENTITY (1, 1) NOT NULL,
    [EventGeneratorId] INT            NOT NULL,
    [Schedule]         NVARCHAR (200) NOT NULL,
    [Disabled]         BIT            NOT NULL,
    CONSTRAINT [PK_EventSchedules] PRIMARY KEY CLUSTERED ([EventScheduleId] ASC),
    CONSTRAINT [FK_EventSchedules_EventGenerators] FOREIGN KEY ([EventGeneratorId]) REFERENCES [_Scheduler].[EventGenerators] ([EventGeneratorId])
);

