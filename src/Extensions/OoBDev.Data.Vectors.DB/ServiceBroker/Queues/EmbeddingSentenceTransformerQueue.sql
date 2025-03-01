CREATE QUEUE [embedding].[oobdev://embedding/sentence-transformer/queue] WITH 
    STATUS = ON, 
    RETENTION = OFF;
    -- Note: this queue uses external activation