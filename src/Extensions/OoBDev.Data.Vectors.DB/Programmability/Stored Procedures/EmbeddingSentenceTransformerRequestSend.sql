
CREATE PROCEDURE [embedding].[oobdev://embedding/sentence-transformer/request/send]
    @id BIGINT = NULL,
    @value NVARCHAR(MAX),
    @tableName SYSNAME,
    @conversationHandle UNIQUEIDENTIFIER OUTPUT
AS
BEGIN
	SET NOCOUNT ON;
    SET @conversationHandle = NULL;
    IF @value IS NULL OR @tableName IS NULL
    BEGIN
        RAISERROR ('Invalid request payload', 16, 1);
        RETURN;
    END
    DECLARE @message XML;

    WITH XMLNAMESPACES ('oobdev://embedding/sentence-transformer/request' AS st)
	SELECT @message = (
		SELECT 
			@id AS [@id],
			@value AS [@value],
			@tableName AS [@tableName]
		FOR XML PATH('st:request')
	);

    DECLARE @startedTran BIT = 0;
    BEGIN TRY
        IF @@TRANCOUNT = 0
        BEGIN
            BEGIN TRAN;
            SET @startedTran = 1;
        END

	    BEGIN DIALOG @conversationHandle
		    FROM SERVICE [oobdev://embedding/storage]
		    TO SERVICE 'oobdev://embedding/sentence-transformer' 
		    ON CONTRACT [oobdev://embedding/sentence-transformer]
		    WITH ENCRYPTION = OFF
		    ;

	    SEND ON CONVERSATION @conversationHandle
		    MESSAGE TYPE [oobdev://embedding/sentence-transformer/request]
		    (@message);
            
        IF @startedTran = 1
        BEGIN
            COMMIT TRAN;
        END
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 AND @startedTran = 1
        BEGIN
            ROLLBACK TRAN;
        END
        SET @conversationHandle = NULL;
        THROW;
    END CATCH
END

