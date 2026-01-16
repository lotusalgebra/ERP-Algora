-- Create Integration Tables
-- Run this script on each tenant database to enable CRM/E-commerce integrations
-- This includes IntegrationSettings and CrmIntegrationMappings tables

SET QUOTED_IDENTIFIER ON;
GO

-- ================================================
-- IntegrationSettings Table
-- Stores CRM/E-commerce integration configuration
-- ================================================
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

-- ================================================
-- CrmIntegrationMappings Table
-- Maps ERP entities to their corresponding CRM entities
-- ================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrmIntegrationMappings')
BEGIN
    CREATE TABLE CrmIntegrationMappings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CrmType NVARCHAR(50) NOT NULL,              -- 'Salesforce', 'Dynamics365', 'Shopify'
        EntityType NVARCHAR(100) NOT NULL,          -- 'Contact', 'Lead', 'Customer', 'Order', 'Product'
        ErpEntityId UNIQUEIDENTIFIER NOT NULL,      -- ID of the entity in ERP
        CrmEntityId NVARCHAR(100) NOT NULL,         -- ID of the entity in CRM
        LastSyncedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        SyncStatus NVARCHAR(50) NOT NULL DEFAULT 'Synced',  -- 'Synced', 'Pending', 'Error'
        LastSyncError NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Indexes for efficient lookups
    CREATE INDEX IX_CrmIntegrationMappings_ErpEntityId
        ON CrmIntegrationMappings(ErpEntityId, CrmType);
    CREATE INDEX IX_CrmIntegrationMappings_CrmEntityId
        ON CrmIntegrationMappings(CrmEntityId, CrmType);
    CREATE INDEX IX_CrmIntegrationMappings_EntityType
        ON CrmIntegrationMappings(EntityType, CrmType);

    PRINT 'Created CrmIntegrationMappings table';
END
ELSE
BEGIN
    PRINT 'CrmIntegrationMappings table already exists';
END
GO

PRINT 'Integration tables setup complete';
GO
