CREATE TYPE [embedding].[oobdev://embedding/sentence-transformer/response/send-batch/set] AS TABLE 
(
    [id]                    BIGINT,
    [sourceId]              BIGINT,
    [sourceName]            NVARCHAR(128),
    [vectorId]              BIGINT,
    [action]                NVARCHAR(20),
    [conversationGroupId]   UNIQUEIDENTIFIER,
    [conversationHandle]    UNIQUEIDENTIFIER,
    [messageType]           NVARCHAR(128),
    [payload]               XML,
    PRIMARY KEY ([id])
)
