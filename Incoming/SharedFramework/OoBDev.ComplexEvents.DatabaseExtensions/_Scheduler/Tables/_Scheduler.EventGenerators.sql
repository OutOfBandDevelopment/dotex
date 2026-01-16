CREATE TABLE [_Scheduler].[EventGenerators] (
    [EventGeneratorId]      INT                IDENTITY (1, 1) NOT NULL,
    [AssemblyName]          NVARCHAR (127)     NOT NULL,    
	[Namespace]             NVARCHAR (127)     NOT NULL,
	[TypeName]              NVARCHAR (127)     NOT NULL,
    [OriginalSchedule]      NVARCHAR (200)     NOT NULL,
    [NextRun]               DATETIMEOFFSET (7) NULL,
    [LastComplete]          DATETIMEOFFSET (7) NULL,
    [Status]                NVARCHAR (50)      NULL,
    [LastErrorMessage]      NVARCHAR (500)     NULL,
    [Disabled]              BIT                CONSTRAINT [DF_EventGenerators_Disabled] DEFAULT ((0)) NOT NULL,
    [AssemblyQualifiedName]  AS (((([Namespace]+'.')+[TypeName])+', ')+[AssemblyName]) PERSISTED NOT NULL,
    CONSTRAINT [PK_EventGenerators] PRIMARY KEY CLUSTERED ([EventGeneratorId] ASC)
);

GO

CREATE NONCLUSTERED INDEX [UX_EventGenerators] ON [_Scheduler].[EventGenerators]
	(
	[AssemblyName],
	[Namespace],
    [TypeName]  
    )
GO