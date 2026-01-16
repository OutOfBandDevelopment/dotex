
CREATE PROCEDURE [_Scheduler].[RegisterSchedulers]
	@xml	XML
AS
BEGIN
	SET NOCOUNT ON;
	
	DROP TABLE IF EXISTS [#EventGenerators];

		SELECT 
			 x.x.value('../@AssemblyName', 'NVARCHAR(127)')	AS [AssemblyName]
			,x.x.value('../@Namespace', 'NVARCHAR(127)')	AS [Namespace]
			,x.x.value('../@TypeName', 'NVARCHAR(127)')		AS [TypeName]
			,x.x.value('../@NextStart', 'DATETIMEOFFSET')	AS [NextRun]
			,x.x.value('.', 'NVARCHAR(200)')				AS [Schedule]
		INTO [#EventGenerators]
		FROM @xml.nodes('EventGenerators/EventGenerator/Schedule') x(x)
		;

	DROP TABLE IF EXISTS [#InsertedEventGenerators];
	CREATE TABLE [#InsertedEventGenerators] (
		 [AssemblyName] NVARCHAR(127)
		,[Namespace]    NVARCHAR(127)
		,[TypeName]	    NVARCHAR(127)
		);  

	BEGIN TRAN;

	WITH [$EventGenerators] AS (
		SELECT 
			 [AssemblyName]
			,[Namespace]
			,[TypeName]
			,MIN([NextRun]) AS [NextRun]
			,STRING_AGG([Schedule], '|') AS [OriginalSchedule]
		FROM [#EventGenerators]
		GROUP BY
			 [AssemblyName]
			,[Namespace]
			,[TypeName]
	)
	MERGE 
		INTO [_Scheduler].[EventGenerators] as target
		USING (SELECT 
				 [AssemblyName]
				,[Namespace]
				,[TypeName]
				,[NextRun]
				,[OriginalSchedule]
				FROM [$EventGenerators]) as source
			ON      target.[AssemblyName] = source.[AssemblyName]
				AND target.[Namespace]	  = source.[Namespace]	 
				AND target.[TypeName]	  = source.[TypeName]	 
		WHEN NOT MATCHED BY TARGET THEN 
			INSERT (
				 [AssemblyName]
				,[Namespace]
				,[TypeName]
				,[NextRun]
				,[OriginalSchedule]
			) VALUES (
				 source.[AssemblyName]
				,source.[Namespace]
				,source.[TypeName]
				,source.[NextRun]
				,source.[OriginalSchedule]
			)
			OUTPUT 
				 inserted.[AssemblyName]
				,inserted.[Namespace]
				,inserted.[TypeName]
			INTO 
				[#InsertedEventGenerators]
			;

	WITH [$EventGeneratorSchedules] AS (		
		SELECT 
			 [EventGenerators].[EventGeneratorId]
			,[#EventGenerators].[Schedule]
		FROM [#InsertedEventGenerators]
		INNER JOIN [_Scheduler].[EventGenerators]
			ON      [EventGenerators].[AssemblyName]	= [#InsertedEventGenerators].[AssemblyName]
				AND [EventGenerators].[Namespace]		= [#InsertedEventGenerators].[Namespace]	 
				AND [EventGenerators].[TypeName]		= [#InsertedEventGenerators].[TypeName]	 
		INNER JOIN [#EventGenerators]
			ON      [#EventGenerators].[AssemblyName]	= [#InsertedEventGenerators].[AssemblyName]
				AND [#EventGenerators].[Namespace]		= [#InsertedEventGenerators].[Namespace]	 
				AND [#EventGenerators].[TypeName]		= [#InsertedEventGenerators].[TypeName]	 
	)
	MERGE 
		INTO [_Scheduler].[EventSchedules] as target
		USING (SELECT 
				 [EventGeneratorId]
				,[Schedule]
				FROM [$EventGeneratorSchedules]) as source
			ON      target.[EventGeneratorId]	= source.[EventGeneratorId]
				AND target.[Schedule]			= source.[Schedule]	 
		WHEN NOT MATCHED BY TARGET THEN 
			INSERT (
				 [EventGeneratorId]
				,[Schedule]
			) VALUES (
				 source.[EventGeneratorId]
				,source.[Schedule]
			)
			;

	COMMIT 
END