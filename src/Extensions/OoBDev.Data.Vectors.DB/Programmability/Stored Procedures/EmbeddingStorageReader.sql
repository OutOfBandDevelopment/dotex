CREATE PROCEDURE [embedding].[oobdev://embedding/storage/reader]
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE  @read INT			= 100
			,@timeout INT		= 6000;

	DECLARE @received TABLE (
		 [conversationGroup]	UNIQUEIDENTIFIER
		,[conversationHandle]	UNIQUEIDENTIFIER
		,[messageType]			NVARCHAR(255)
		,[messageBody]			XML
	);

	BEGIN TRY
		BEGIN TRANSACTION;
			   		 
		WAITFOR (
			RECEIVE TOP (@read)
				 conversation_group_id
				,conversation_handle
				,message_type_name
				,CAST(message_body AS XML)
				FROM [oobdev://embedding/storage/queue]
				INTO @received
		), TIMEOUT @timeout;
		
		DROP TABLE IF EXISTS [#resposes];
			SELECT *
			INTO [#resposes]
			FROM @received AS [$received]
			WHERE 
				[$received].[messageType] = 'oobdev://embedding/sentence-transformer/response';
				
		DECLARE @response [embedding].[oobdev://embedding/sentence-transformer/response/send-batch/set];
				
		INSERT INTO @response([action],[conversationGroupId],[conversationHandle],[messageType],[payload])
			SELECT
				 'other'
				,[conversationGroup]
				,[conversationHandle]
				,[messageType]
				,[messageBody]
			FROM @received AS [$received]
			WHERE 
				[$received].[messageType] != 'oobdev://embedding/sentence-transformer/response';

		WITH XMLNAMESPACES(DEFAULT 'oobdev://embedding/sentence-transformer/response')
		INSERT INTO [embedding].[Sources] ([Name])
		SELECT 
			[$Sources].[SourceName]
		FROM (
			SELECT DISTINCT
				x.value('@tableName', 'NVARCHAR(128)') AS [SourceName]
			FROM [#resposes] AS [$Received]
			CROSS APPLY [$Received].[messageBody].nodes('response') x(x)
		) AS [$Sources]
		WHERE 
			NOT EXISTS (
				SELECT *
				FROM [embedding].[Sources]
				WHERE 
					[Sources].[Name] = [$Sources].[SourceName]
			);

		WITH XMLNAMESPACES(DEFAULT 'oobdev://embedding/sentence-transformer/response')
		MERGE INTO [embedding].[Vectors] AS target
		USING (
			SELECT 
				 [$Received].[Embedding]
				,[$Received].[id]
				,[Sources].[SourceID]
				,[$Received].[SourceName]
				,[$Received].[conversationGroup]
				,[$Received].[conversationHandle]
				,[$Received].[messageType]
			FROM (
				SELECT 
					 x.value('@id', 'BIGINT') AS [id]
					,x.value('@value', 'NVARCHAR(MAX)') AS [value]
					,x.value('@tableName', 'NVARCHAR(128)') AS [SourceName]
					,CAST(x.value('@embedding', 'NVARCHAR(MAX)') AS [embedding].[VectorF]) AS [Embedding]
					,[$Received].[conversationGroup]
					,[$Received].[conversationHandle]
					,[$Received].[messageType]
				FROM [#resposes] AS [$Received]
				CROSS APPLY [$Received].[messageBody].nodes('response') x(x)
			) AS [$Received]
			INNER JOIN [embedding].[Sources]
				ON [Sources].[Name] = [$Received].[SourceName]
		) AS source ON source.[id] = target.[OriginalID]
					AND source.[SourceID] = target.[SourceID]
		WHEN MATCHED
			THEN UPDATE SET 
                [Value] = source.[Embedding]
		WHEN NOT MATCHED BY TARGET 
			THEN INSERT ([Value],[OriginalID],[SourceID]) 
				VALUES (source.[Embedding],source.[id],source.[SourceID])
		OUTPUT 
			 inserted.[OriginalID]
			,inserted.[SourceID]
			,source.[SourceName]
			,inserted.[VectorID]
			,$action
			,source.[conversationGroup]
			,source.[conversationHandle]
			,source.[messageType]
			INTO @response(
					 [id]                   
					,[sourceId]             
					,[sourceName]           
					,[vectorId]             
					,[action]               
					,[conversationGroupId]  
					,[conversationHandle]  
					,[messageType]
				)
		;

		EXEC [embedding].[oobdev://embedding/sentence-transformer/response/send-batch]
			@items = @response,
			@returnValues = 0;

		COMMIT;
	END TRY
	BEGIN CATCH
		SELECT 
			ERROR_NUMBER() AS ErrorNumber,
			ERROR_MESSAGE() AS ErrorMessage;
		ROLLBACK;
	END CATCH
END
