
CREATE PROCEDURE [_Scheduler].[LockRecord]
	@eventGeneratorId INT
AS
BEGIN
	SET NOCOUNT ON;
	
	UPDATE [_Scheduler].[EventGenerators]
	SET [Status] = 'Running'
	FROM [_Scheduler].[EventGenerators] WITH (RowLock)
	WHERE 
		[EventGenerators].[EventGeneratorId] = @eventGeneratorId
		AND [EventGenerators].[Disabled] = 0;
END