/*
These singleton tables will block while any of the records are locked for processing. 
*/

-- Only list events that are not processing/locked 
SELECT
	*,'READPAST'
FROM [Bucket].[_Scheduler].[EventGenerators] 
	WITH (READPAST)
;

-- List all events including those that are locked
  SELECT
	*,'READUNCOMMITTED'
FROM [Bucket].[_Scheduler].[EventGenerators] 
	WITH (READUNCOMMITTED)
;

--Get Pending Count
SELECT COUNT(*) AS [Count]
FROM [_Scheduler].[EventGenerators] AS [e] WITH (ReadPast)
WHERE [e].[NextRun] <= GETDATE() AND [e].[Disabled] = 0