CREATE TABLE [Suppressions] (
    [Address] nvarchar(320) NOT NULL,
    [Reason] nvarchar(200) NOT NULL,
    [AddedAtTicks] bigint NOT NULL,
    CONSTRAINT [PK_Suppressions] PRIMARY KEY ([Address])
);
GO
