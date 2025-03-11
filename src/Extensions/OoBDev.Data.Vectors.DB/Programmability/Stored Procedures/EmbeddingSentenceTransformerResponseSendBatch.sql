CREATE PROCEDURE [embedding].[oobdev://embedding/sentence-transformer/response/send-batch]
    @items [embedding].[oobdev://embedding/sentence-transformer/response/send-batch/set] READONLY,
	@returnValues BIT = 0
AS
BEGIN
	SET NOCOUNT ON;
	
    DECLARE @results TABLE (
        [ID] BIGINT,
        [ConversationHandle] UNIQUEIDENTIFIER
    );

	DECLARE 
         @id                    BIGINT
        ,@sourceId              BIGINT
        ,@sourceName            NVARCHAR(128)
        ,@vectorId              BIGINT
        ,@action                NVARCHAR(20)
        ,@conversationGroupId   UNIQUEIDENTIFIER
        ,@conversationHandle    UNIQUEIDENTIFIER
        ,@messageType           NVARCHAR(128)
        ,@payload               XML;

	DECLARE [ItemCursor] CURSOR FOR		
		SELECT 
             [Set].[id]                   
            ,[Set].[sourceId]             
            ,[Set].[sourceName]           
            ,[Set].[vectorId]             
            ,[Set].[action]               
            ,[Set].[conversationGroupId]  
            ,[Set].[conversationHandle]   
            ,[Set].[messageType]          
            ,[Set].[payload]            
		FROM @items AS [Set];      

	OPEN [ItemCursor];
	FETCH NEXT FROM [ItemCursor] INTO
         @id                    
        ,@sourceId              
        ,@sourceName            
        ,@vectorId              
        ,@action                
        ,@conversationGroupId   
        ,@conversationHandle    
        ,@messageType           
        ,@payload
        ;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		EXEC [embedding].[oobdev://embedding/sentence-transformer/response/send]
             @id                  = @id                    
            ,@sourceId            = @sourceId              
            ,@sourceName          = @sourceName            
            ,@vectorId            = @vectorId              
            ,@action              = @action                
            ,@conversationGroupId = @conversationGroupId   
            ,@conversationHandle  = @conversationHandle    
            ,@messageType         = @messageType           
            ,@payload             = @payload
            ;

		INSERT INTO @results 
			VALUES (@id, @conversationHandle);

	    FETCH NEXT FROM [ItemCursor] INTO
             @id                    
            ,@sourceId              
            ,@sourceName            
            ,@vectorId              
            ,@action                
            ,@conversationGroupId   
            ,@conversationHandle    
            ,@messageType           
            ,@payload
            ;
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
