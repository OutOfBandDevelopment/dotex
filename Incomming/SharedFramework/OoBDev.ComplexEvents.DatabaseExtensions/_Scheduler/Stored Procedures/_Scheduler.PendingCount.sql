CREATE PROCEDURE [_Scheduler].[PendingCount]
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT COUNT(*) AS [Count]
	FROM [_Scheduler].[EventGenerators] WITH (ReadPast, UpdLock, RowLock) 
	WHERE (
		[EventGenerators].[Disabled] = 0
	) AND (
		[EventGenerators].[NextRun] <= SYSDATETIMEOFFSET()
	);
END