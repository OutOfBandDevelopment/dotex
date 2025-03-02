CREATE QUEUE [embedding].[oobdev://embedding/storage/queue] 
    WITH 
        STATUS = ON, 
        RETENTION = OFF,
        ACTIVATION (
            STATUS = OFF, 
            PROCEDURE_NAME = [embedding].[oobdev://embedding/storage/reader] , 
            MAX_QUEUE_READERS = 1, 
            EXECUTE AS N'dbo'  
        ), POISON_MESSAGE_HANDLING (
            STATUS = ON
        );