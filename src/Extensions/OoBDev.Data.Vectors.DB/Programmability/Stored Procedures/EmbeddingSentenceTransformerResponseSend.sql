CREATE PROCEDURE [embedding].[oobdev://embedding/sentence-transformer/response/send]
     @id                    BIGINT
    ,@sourceId              BIGINT
    ,@sourceName            NVARCHAR(128)
    ,@vectorId              BIGINT
    ,@action                NVARCHAR(20)
    ,@conversationGroupId   UNIQUEIDENTIFIER
    ,@conversationHandle    UNIQUEIDENTIFIER
    ,@messageType           NVARCHAR(128)
    ,@payload               XML
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @message XML;

    WITH XMLNAMESPACES ('oobdev://embedding/sentence-transformer/response' AS st)
	SELECT @message = (
		SELECT 
			@id				AS [@id],
			@sourceName		AS [@tableName],
			@sourceId		AS [@sourceId],
			@vectorId		AS [@vectorId],
			@action			AS [@action]
		FOR XML PATH('st:response')
	);

	PRINT @messageType;


	IF (@messageType = 'oobdev://embedding/sentence-transformer/response')
	BEGIN
		END CONVERSATION @conversationHandle;
		--TODO: [ ] Add support to optionally route an event back to the application 
		
		--SEND ON CONVERSATION @conversationHandle
		--	MESSAGE TYPE [oobdev://embedding/sentence-transformer/response]
		--	(@message);
	END 
END
