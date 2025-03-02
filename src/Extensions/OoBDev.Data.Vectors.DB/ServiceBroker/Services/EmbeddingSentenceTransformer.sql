CREATE SERVICE [oobdev://embedding/sentence-transformer]
    ON QUEUE [embedding].[oobdev://embedding/sentence-transformer/queue]
(
	[oobdev://embedding/sentence-transformer]
)