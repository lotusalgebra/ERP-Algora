-- Create IntegrationSettings table for storing CRM integration configuration
-- Run this on each tenant database
-- This replaces the appsettings.json configuration with database-stored settings

SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IntegrationSettings')
BEGIN
    CREATE TABLE IntegrationSettings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        -- Integration identification
        IntegrationType NVARCHAR(50) NOT NULL,    -- 'Salesforce', 'Dynamics365', 'Shopify'

        -- Status
        IsEnabled BIT NOT NULL DEFAULT 0,

        -- Configuration
        SettingsJson NVARCHAR(MAX) NOT NULL DEFAULT '{}',  -- Non-sensitive JSON config
        EncryptedCredentials NVARCHAR(MAX) NULL,           -- Encrypted sensitive credentials
        SyncIntervalMinutes INT NOT NULL DEFAULT 30,

        -- Test status
        LastTestedAt DATETIME2 NULL,
        LastTestSuccess BIT NULL,
        LastTestError NVARCHAR(MAX) NULL,

        -- Sync status
        LastSyncAt DATETIME2 NULL,
        LastSyncSuccess BIT NULL,
        LastSyncRecordsProcessed INT NULL,

        -- Audit fields (from AuditableEntity)
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    -- Unique constraint: one settings record per integration type
    CREATE UNIQUE INDEX IX_IntegrationSettings_IntegrationType
        ON IntegrationSettings(IntegrationType)
        WHERE IsDeleted = 0;

    -- Index for querying enabled integrations
    CREATE INDEX IX_IntegrationSettings_IsEnabled
        ON IntegrationSettings(IsEnabled)
        WHERE IsDeleted = 0;

    PRINT 'Created IntegrationSettings table';
END
ELSE
BEGIN
    PRINT 'IntegrationSettings table already exists';
END
GO

-- Add any missing columns if the table was created previously with fewer columns
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'IntegrationSettings')
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IntegrationSettings') AND name = 'LastSyncRecordsProcessed')
    BEGIN
        ALTER TABLE IntegrationSettings ADD LastSyncRecordsProcessed INT NULL;
        PRINT 'Added LastSyncRecordsProcessed column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IntegrationSettings') AND name = 'LastSyncAt')
    BEGIN
        ALTER TABLE IntegrationSettings ADD LastSyncAt DATETIME2 NULL;
        PRINT 'Added LastSyncAt column';
    END

    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IntegrationSettings') AND name = 'LastSyncSuccess')
    BEGIN
        ALTER TABLE IntegrationSettings ADD LastSyncSuccess BIT NULL;
        PRINT 'Added LastSyncSuccess column';
    END
END
GO

PRINT 'IntegrationSettings table setup complete';
