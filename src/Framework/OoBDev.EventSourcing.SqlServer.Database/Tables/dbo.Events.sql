CREATE TABLE [dbo].[Events] (
     [Position] BIGINT IDENTITY(1,1) NOT NULL
    ,[StreamId] NVARCHAR(200) NOT NULL
    ,[Sequence] BIGINT NOT NULL
    ,[Type] NVARCHAR(200) NOT NULL
    ,[Timestamp] DATETIMEOFFSET(7) NOT NULL
    ,[Payload] NVARCHAR(MAX) NOT NULL
    ,[Metadata] NVARCHAR(MAX) NULL
    ,CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED ([Position] ASC)
    ,CONSTRAINT [CK_Events_Payload_Json] CHECK (ISJSON([Payload]) = 1)
);
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Events_StreamId_Sequence] ON [dbo].[Events] ([StreamId] ASC, [Sequence] ASC);
