CREATE PROCEDURE [_Scheduler].[GetRecord]
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP(1) 
		 [EventGenerators].[EventGeneratorId]
		,[EventGenerators].[AssemblyQualifiedName]
		,[EventGenerators].[OriginalSchedule]
		,(SELECT DISTINCT
				[Schedule] AS [text()]
			FROM [_Scheduler].[EventSchedules]
			WHERE 
				[EventSchedules].[EventGeneratorId] = [EventGenerators].[EventGeneratorId]
			FOR XML PATH('S'), ROOT('X')
			) AS [SchedulesXml]
	FROM [_Scheduler].[EventGenerators] WITH (ReadPast, UpdLock, RowLock) 
	WHERE (
		[EventGenerators].[Disabled] = 0
	) AND (
		[EventGenerators].[NextRun] <= SYSDATETIMEOFFSET()
		);
END