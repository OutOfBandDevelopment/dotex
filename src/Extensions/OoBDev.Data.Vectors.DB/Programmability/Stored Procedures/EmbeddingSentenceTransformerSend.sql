
CREATE PROCEDURE [embedding].[oobdev://embedding/sentence-transformer/send]
    @id BIGINT = NULL,
    @value NVARCHAR(MAX),
    @tableName SYSNAME,
    @conversationHandle UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @message XML;

    WITH XMLNAMESPACES ('oobdev://embedding/sentence-transformer/request' AS st)
	SELECT @message = (
		SELECT 
			@id AS [st:request/@id],
			@value AS [st:request/@value],
			@tableName AS [st:request/@tableName]
		FOR XML PATH('st:request'), TYPE
	);

	BEGIN DIALOG @conversationHandle
		FROM SERVICE [oobdev://embedding/storage]
		TO SERVICE 'oobdev://embedding/sentence-transformer' 
		ON CONTRACT [oobdev://embedding/sentence-transformer]
		WITH ENCRYPTION = OFF
		;

	SEND ON CONVERSATION @conversationHandle
		MESSAGE TYPE [oobdev://embedding/sentence-transformer/request]
		(@message);
END

