CREATE PROCEDURE [embedding].[oobdev://embedding/sentence-transformer/response/send-batch]
    @items [embedding].[oobdev://embedding/sentence-transformer/response/send-batch/set] READONLY,
	@returnValues BIT = 1
AS
BEGIN
	SET NOCOUNT ON;
	
    DECLARE @results TABLE (
        [ID] BIGINT,
        [ConversationHandle] UNIQUEIDENTIFIER
    );

	DECLARE 
		 @id BIGINT
		,@value NVARCHAR(MAX)
		,@tableName SYSNAME
		,@conversationHandle UNIQUEIDENTIFIER;

	DECLARE [ItemCursor] CURSOR FOR		
		SELECT 
			 [Set].[id]
			,[Set].[value]
			,[Set].[tableName] 
		FROM @items AS [Set];

	OPEN [ItemCursor];
	FETCH NEXT FROM [ItemCursor] 
		INTO @id, @value, @tableName;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC [embedding].[oobdev://embedding/sentence-transformer/response/send]
			 @id = @id
			,@value = @value
			,@tableName = @tableName
			,@conversationHandle = @conversationHandle;

		INSERT INTO @results 
			VALUES (@id, @conversationHandle);

		FETCH NEXT FROM [ItemCursor] 
			INTO @id, @value, @tableName;
	END;

	CLOSE [ItemCursor];
	DEALLOCATE [ItemCursor];

	IF (@returnValues = 1)
	BEGIN
		SELECT
			 [Results].[ID]
			,[Results].[ConversationHandle]
		FROM @results AS [Results];
	END
END			
