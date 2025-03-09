
CREATE PROCEDURE [embedding].[oobdev://embedding/sentence-transformer/response/send]
    @id BIGINT = NULL,
    @value NVARCHAR(MAX),
    @tableName SYSNAME,
    @conversationHandle UNIQUEIDENTIFIER
AS
BEGIN
	SET NOCOUNT ON;

    DECLARE @message XML;

    WITH XMLNAMESPACES ('oobdev://embedding/sentence-transformer/response' AS st)
	SELECT @message = (
		SELECT 
			@id AS [@id],
			@value AS [@value],
			@tableName AS [@tableName]
		FOR XML PATH('st:response')
	);

	SEND ON CONVERSATION @conversationHandle
		MESSAGE TYPE [oobdev://embedding/sentence-transformer/response]
		(@message);
END

