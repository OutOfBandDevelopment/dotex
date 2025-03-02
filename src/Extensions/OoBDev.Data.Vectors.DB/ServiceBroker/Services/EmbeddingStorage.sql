CREATE SERVICE [oobdev://embedding/storage]
    ON QUEUE [embedding].[oobdev://embedding/storage/queue]
(
	[oobdev://embedding/sentence-transformer]
)