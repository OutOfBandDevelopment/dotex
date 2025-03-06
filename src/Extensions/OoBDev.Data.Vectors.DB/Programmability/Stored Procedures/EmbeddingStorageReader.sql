CREATE PROCEDURE [embedding].[oobdev://embedding/storage/reader] 
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE  @read INT			= 1
			,@timeout INT		= 6000;

	DECLARE @received TABLE (
		 [conversationGroup]	UNIQUEIDENTIFIER
		,[conversationHandle]	UNIQUEIDENTIFIER
		,[mesageType]			NVARCHAR(255)
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
				[$received].[mesageType] = 'oobdev://embedding/sentence-transformer/response'
		DROP TABLE IF EXISTS [#others];
			SELECT *
			INTO [#others]
			FROM @received AS [$received]
			WHERE 
				[$received].[mesageType] != 'oobdev://embedding/sentence-transformer/response'
			   
		IF EXISTS (SELECT * FROM [#resposes])
		BEGIN
		
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
			INSERT INTO [embedding].[Vectors] (
				 [Value]
				,[OriginalID]
				,[SourceID]
			)
			SELECT 
				 [$Received].[Embedding]
				,[$Received].[id]
				,[Sources].[SourceID]
			FROM (
				SELECT 
					x.value('@id', 'BIGINT') AS [id]
					,x.value('@value', 'NVARCHAR(MAX)') AS [value]
					,x.value('@tableName', 'NVARCHAR(128)') AS [SourceName]
					,CAST(x.value('@embedding', 'NVARCHAR(MAX)') AS [embedding].[Vector]) AS [Embedding]
				FROM [#resposes] AS [$Received]
				CROSS APPLY [$Received].[messageBody].nodes('response') x(x)
			) AS [$Received]
			INNER JOIN [embedding].[Sources]
				ON [Sources].[Name] = [$Received].[SourceName];
			
			--TODO: add response here
			-- - [ ] Create a /send
			-- - [ ] Create a /send-batch
            -- - [ ] make this a merge script instead
            -- - [ ] change to VectorF

		END

		--TODO: add end response

		COMMIT;
	END TRY
	BEGIN CATCH
		SELECT 
			ERROR_NUMBER() AS ErrorNumber,
			ERROR_MESSAGE() AS ErrorMessage;
		ROLLBACK;
	END CATCH
END
