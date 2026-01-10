/*
This needs called from within a transaction.  

In pure SQL it can be a BEGIN TRANS in .Net use TransactionScope or Database.BeginTransaction()
*/

-- Step 1) Get data with hint that it may be updated to and UPDATE ROWLOCK
SELECT TOP(1) 
	[e].[EventGeneratorId], 
	[e].[AssemblyQualifiedName], 
	[e].[Disabled], 
	[e].[LastComplete], 
	[e].[NextRun], 
	[e].[OriginalSchedule], 
	[e].[Status]
FROM [_Scheduler].[EventGenerators] AS [e] WITH (ReadPast, UpdLock) 
WHERE ([e].[Disabled] = CAST(0 AS bit)) AND ([e].[NextRun] <= CAST(GETDATE() AS datetimeoffset))

-- Step 2) Update row selected above as Running to set row lock.
UPDATE [_Scheduler].[EventGenerators]
SET [Status] = 'Running'
WHERE 
	[EventGeneratorId] = @workId;

-- Step 3) Do your long running work
PRINT 'Do something here';

-- Step 4) Update row selected above as Completed to set row lock. 
UPDATE [_Scheduler].[EventGenerators]
SET [Status] = 'Running'
WHERE 
	[EventGeneratorId] = @workId;

-- Step 5) Complete the transaction 
/*
in SQL use COMMIT in .Net use transactionScope.Complete() or Database.CommitTransaction()
*/