CREATE TYPE [embedding].[oobdev://embedding/sentence-transformer/send-batch/set] AS TABLE 
(
    [id] BIGINT,
    [value] NVARCHAR(MAX),
    [tableName] SYSNAME,
    PRIMARY KEY ([id])
)
