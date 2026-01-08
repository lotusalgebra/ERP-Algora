-- Create DatabaseBackups table for Admin database
-- This table tracks database backups for each tenant

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DatabaseBackups' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[DatabaseBackups] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [DatabaseName] NVARCHAR(100) NOT NULL,
        [FileName] NVARCHAR(500) NOT NULL,
        [FilePath] NVARCHAR(1000) NOT NULL,
        [Type] INT NOT NULL DEFAULT 0,  -- 0: Full, 1: Differential, 2: TransactionLog
        [Status] INT NOT NULL DEFAULT 0, -- 0: Pending, 1: InProgress, 2: Completed, 3: Failed, 4: Deleted
        [FileSizeBytes] BIGINT NULL,
        [StartedAt] DATETIME2 NULL,
        [CompletedAt] DATETIME2 NULL,
        [ErrorMessage] NVARCHAR(4000) NULL,
        [ExpiresAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] UNIQUEIDENTIFIER NULL,
        [Notes] NVARCHAR(1000) NULL,
        CONSTRAINT [PK_DatabaseBackups] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_DatabaseBackups_Tenants] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants] ([Id]) ON DELETE CASCADE
    );

    -- Create indexes
    CREATE NONCLUSTERED INDEX [IX_DatabaseBackups_TenantId_CreatedAt]
        ON [dbo].[DatabaseBackups] ([TenantId] ASC, [CreatedAt] DESC);

    CREATE NONCLUSTERED INDEX [IX_DatabaseBackups_Status]
        ON [dbo].[DatabaseBackups] ([Status] ASC);

    CREATE NONCLUSTERED INDEX [IX_DatabaseBackups_ExpiresAt]
        ON [dbo].[DatabaseBackups] ([ExpiresAt] ASC)
        WHERE [ExpiresAt] IS NOT NULL;

    PRINT 'DatabaseBackups table created successfully.';
END
ELSE
BEGIN
    PRINT 'DatabaseBackups table already exists.';
END
GO
