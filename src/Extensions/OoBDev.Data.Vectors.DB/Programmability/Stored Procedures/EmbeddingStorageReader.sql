CREATE PROCEDURE [embedding].[oobdev://embedding/storage/reader] 
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE  @conversationGroup		UNIQUEIDENTIFIER
			,@conversationHandle	UNIQUEIDENTIFIER
			,@mesageType			NVARCHAR(255)
			,@messageBody			XML;

	DECLARE  @read INT			= 1
			,@timeout INT		= 6000;

	BEGIN TRY
		BEGIN TRANSACTION;
		DECLARE @processed INT;

		WAITFOR (
			RECEIVE TOP (@read)
				 @conversationGroup		= conversation_group_id
				,@conversationHandle	= conversation_handle
				,@mesageType			= message_type_name
				,@messageBody			= CAST(message_body AS XML)
				FROM [oobdev://embedding/storage/queue]
		), TIMEOUT @timeout;
		
		SET @processed = 1;
		PRINT '[Storage_Reader]: ' + ISNULL(@mesageType,'(null)') + ' ' + CAST(@processed AS NVARCHAR(20))
		IF (@processed > 0)
		BEGIN
			PRINT '[Storage_Reader]: TODO: write embedding'
			SELECT 
				 @conversationGroup		AS [conversationGroup]
				,@conversationHandle	AS [conversationHandle]
				,@mesageType			AS [messageType]
				,@messageBody			AS [messageBody];

			PRINT '[Storage_Reader]: end request'
			END CONVERSATION @conversationHandle;
		END

		COMMIT;
	END TRY
	BEGIN CATCH
		SELECT 
			ERROR_NUMBER() AS ErrorNumber,
			ERROR_MESSAGE() AS ErrorMessage;
		ROLLBACK;
	END CATCH
END
GO


