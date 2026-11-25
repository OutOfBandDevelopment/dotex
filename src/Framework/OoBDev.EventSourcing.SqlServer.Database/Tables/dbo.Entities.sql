CREATE TABLE [dbo].[Entities] (
     [Type] NVARCHAR(100) NOT NULL
    ,[Key] NVARCHAR(200) NOT NULL
    ,[Version] BIGINT NOT NULL
    ,[Value] NVARCHAR(MAX) NOT NULL
    ,CONSTRAINT [PK_Entities] PRIMARY KEY CLUSTERED ([Type] ASC, [Key] ASC)
    ,CONSTRAINT [CK_Entities_Value_Json] CHECK (ISJSON([Value]) = 1)
);
