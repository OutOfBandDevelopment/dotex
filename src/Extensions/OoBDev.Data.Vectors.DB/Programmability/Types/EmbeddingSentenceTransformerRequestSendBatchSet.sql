CREATE TYPE [embedding].[oobdev://embedding/sentence-transformer/request/send-batch/set] AS TABLE 
(
    [id]                    BIGINT NOT NULL,
    [value]                 NVARCHAR(MAX) NOT NULL
    PRIMARY KEY ([id])
)
