CREATE TABLE [RightAudit] (
    [Id] bigint NOT NULL IDENTITY,
    [AtTicks] bigint NOT NULL,
    [Subject] nvarchar(200) NOT NULL,
    [Rights] nvarchar(1000) NOT NULL,
    [Result] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_RightAudit] PRIMARY KEY ([Id])
);
GO

CREATE INDEX [IX_RightAudit_AtTicks] ON [RightAudit] ([AtTicks]);
GO

CREATE INDEX [IX_RightAudit_Subject_AtTicks] ON [RightAudit] ([Subject], [AtTicks]);
GO
