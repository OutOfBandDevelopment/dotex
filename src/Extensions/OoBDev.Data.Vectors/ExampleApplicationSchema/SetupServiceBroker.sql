USE [test1]
GO
ALTER DATABASE [test1] SET ENABLE_BROKER WITH ROLLBACK IMMEDIATE;
GO
CREATE MESSAGE TYPE [oobdev://embedding/sentence-transformer/request]
CREATE MESSAGE TYPE [oobdev://embedding/sentence-transformer/response]
CREATE CONTRACT [oobdev://embedding/sentence-transformer] (
    [oobdev://embedding/sentence-transformer/request] SENT BY INITIATOR,
    [oobdev://embedding/sentence-transformer/response] SENT BY TARGET
)
CREATE QUEUE [oobdev://embedding/sentence-transformer/queue] WITH
	STATUS = ON,
	RETENTION = OFF
    -- External Activation
CREATE QUEUE [oobdev://embedding/storage/queue] WITH
	STATUS = ON,
	RETENTION = OFF
    -- Will be internal to save back
CREATE SERVICE [oobdev://embedding/storage] 
	ON QUEUE [oobdev://embedding/storage/queue]
(
	[oobdev://embedding/sentence-transformer]
)
CREATE SERVICE [oobdev://embedding/sentence-transformer] 
	ON QUEUE [oobdev://embedding/sentence-transformer/queue]
(
	[oobdev://embedding/sentence-transformer]
)
GO

-- https://davewentzel.com/content/service-broker-demystified-closed-conversations/

-- check staus
SELECT * FROM sys.conversation_endpoints
 --[dbo].[StartSession]
 --[dbo].[Sentence-transformer_Reader]
 --[dbo].[Storage_Reader]
 --[dbo].[Sentence-transformer_Reader]
GO

CREATE PROCEDURE [dbo].[StartSession]
AS
BEGIN
	SET NOCOUNT ON;

	-- Send Initial Request 

	DECLARE @request XML = N'<request id="123" value="hello world!" />';
	DECLARE @handle UNIQUEIDENTIFIER;

	BEGIN DIALOG @handle
		FROM SERVICE [oobdev://embedding/storage]
		TO SERVICE 'oobdev://embedding/sentence-transformer' 
		ON CONTRACT [oobdev://embedding/sentence-transformer]
		WITH ENCRYPTION = OFF
		;

	SELECT @handle [@handle];

	SEND ON CONVERSATION @handle
		MESSAGE TYPE [oobdev://embedding/sentence-transformer/request]
		(@request);

END
GO

CREATE PROCEDURE [dbo].[Sentence-transformer_Reader]
AS
BEGIN
	DECLARE  @conversationGroup		UNIQUEIDENTIFIER
			,@conversationHandle	UNIQUEIDENTIFIER
			,@mesageType			NVARCHAR(255)
			,@messageBody			XML;

	DECLARE  @read INT			= 1
			,@timeout INT		= 6000;

	BEGIN TRY
		BEGIN TRANSACTION;

		WAITFOR (
			RECEIVE TOP (@read)
				 @conversationGroup		= conversation_group_id
				,@conversationHandle	= conversation_handle
				,@mesageType			= message_type_name
				,@messageBody			= CAST(message_body AS XML)
				FROM [oobdev://embedding/sentence-transformer/queue]
		), TIMEOUT @timeout;

		IF (@@ROWCOUNT > 0)
		BEGIN
			SELECT 
				 @conversationGroup		AS [conversationGroup]
				,@conversationHandle	AS [conversationHandle]
				,@mesageType			AS [messageType]
				,@messageBody			AS [messageBody];

			IF (@mesageType = 'oobdev://embedding/sentence-transformer/request')
			BEGIN
				DECLARE @response XML = N'<response id="123" value="hello world!" />';

				SEND ON CONVERSATION @conversationHandle
					MESSAGE TYPE [oobdev://embedding/sentence-transformer/response]
					(@response);
			END 
			ELSE IF (@messageType = 'http://schemas.microsoft.com/SQL/ServiceBroker/EndDialog')
			BEGIN
				END CONVERSATION @conversationHandle;
			END
		END

		COMMIT;
	END TRY
	BEGIN CATCH
	END CATCH
END
GO

CREATE PROCEDURE [dbo].[Storage_Reader]
AS
BEGIN
	DECLARE  @conversationGroup		UNIQUEIDENTIFIER
			,@conversationHandle	UNIQUEIDENTIFIER
			,@mesageType			NVARCHAR(255)
			,@messageBody			XML;

	DECLARE  @read INT			= 1
			,@timeout INT		= 6000;

	BEGIN TRY
		BEGIN TRANSACTION;

		WAITFOR (
			RECEIVE TOP (@read)
				 @conversationGroup		= conversation_group_id
				,@conversationHandle	= conversation_handle
				,@mesageType			= message_type_name
				,@messageBody			= CAST(message_body AS XML)
				FROM [oobdev://embedding/storage/queue]
		), TIMEOUT @timeout;

		IF (@@ROWCOUNT > 0)
		BEGIN
			SELECT 
				 @conversationGroup		AS [conversationGroup]
				,@conversationHandle	AS [conversationHandle]
				,@mesageType			AS [messageType]
				,@messageBody			AS [messageBody];

			END CONVERSATION @conversationHandle;
		END

		COMMIT;
	END TRY
	BEGIN CATCH
	END CATCH
END
GO

-- read queue 

SELECT TOP (1000) *, casted_message_body = 
CASE message_type_name WHEN 'X' 
  THEN CAST(message_body AS NVARCHAR(MAX)) 
  ELSE message_body 
END 
FROM [test1].[dbo].[oobdev://embedding/storage/queue] WITH(NOLOCK)

-- cleanup 

SELECT *
FROM sys.conversation_endpoints

WHILE (SELECT COUNT(*) FROM sys.conversation_endpoints WHERE state IN ('CD', 'DI')) > 0
BEGIN
	DECLARE @handler UNIQUEIDENTIFIER = (SELECT TOP (1) conversation_handle FROM sys.conversation_endpoints WHERE state IN ('CD', 'DI'));
	END CONVERSATION @handler WITH CLEANUP
END 


