
CREATE PROCEDURE [_Scheduler].[ReleaseLock]
	@eventGeneratorId	INT,
	@NextRun			DATETIMEOFFSET NULL,
	@LastErrorMessage	NVARCHAR(500) NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE [_Scheduler].[EventGenerators]
	SET  [Status] = 'Complete'
		,[NextRun] = @NextRun
		,[LastErrorMessage] = @LastErrorMessage
		,[LastComplete] = SYSDATETIMEOFFSET()
	FROM [_Scheduler].[EventGenerators] WITH (ROWLOCK)
	WHERE 
		[EventGenerators].[EventGeneratorId] = @eventGeneratorId
		AND [EventGenerators].[Disabled] = 0
END