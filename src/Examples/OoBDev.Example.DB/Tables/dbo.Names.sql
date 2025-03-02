CREATE TABLE [dbo].[Names] (
     [NameID] [bigint] IDENTITY(1,1) NOT NULL
    ,[NameValue] [nvarchar](200) NOT NULL
    ,CONSTRAINT [PK_Names] PRIMARY KEY CLUSTERED 
    (
	    [NameID] ASC
        ) WITH (
            PAD_INDEX = OFF, 
            STATISTICS_NORECOMPUTE = OFF, 
            IGNORE_DUP_KEY = OFF, 
            ALLOW_ROW_LOCKS = ON, 
            ALLOW_PAGE_LOCKS = ON, 
            OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
        ) ON [PRIMARY]
    ) ON [PRIMARY]

GO
CREATE TRIGGER [dbo].[Names_UpdateInsert]
   ON  [dbo].[Names] 
   AFTER INSERT, UPDATE
AS 
BEGIN
	SET NOCOUNT ON;

    DECLARE @items [embedding].[oobdev://embedding/sentence-transformer/send-batch/set];
	
    INSERT INTO @items ([id], [value], [tableName])
    SELECT 
		NameID
		,NameValue
		,'[dbo].[Names]'
    FROM INSERTED;
	
    EXEC [embedding].[oobdev://embedding/sentence-transformer/send-batch] @items = @items, @returnValues = 0;

END
GO