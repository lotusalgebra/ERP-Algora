-- CRM Integrations Database Tables
-- Run this script to create the required tables for CRM sync functionality

SET QUOTED_IDENTIFIER ON;
GO

-- CRM Integration Mappings
-- Maps ERP entities to their corresponding CRM entities
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrmIntegrationMappings')
BEGIN
    CREATE TABLE CrmIntegrationMappings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        CrmType NVARCHAR(50) NOT NULL,              -- 'Salesforce' or 'Dynamics365'
        EntityType NVARCHAR(100) NOT NULL,          -- 'Contact', 'Lead', 'Opportunity', 'Account'
        ErpEntityId UNIQUEIDENTIFIER NOT NULL,      -- ID of the entity in ERP
        CrmEntityId NVARCHAR(100) NOT NULL,         -- ID of the entity in CRM
        LastSyncedAt DATETIME2 NOT NULL,
        SyncStatus NVARCHAR(50) NOT NULL DEFAULT 'Synced',  -- 'Synced', 'Pending', 'Error'
        LastSyncError NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedAt DATETIME2 NULL
    );

    -- Indexes for efficient lookups
    CREATE INDEX IX_CrmIntegrationMappings_TenantId ON CrmIntegrationMappings(TenantId);
    CREATE INDEX IX_CrmIntegrationMappings_ErpEntityId ON CrmIntegrationMappings(TenantId, ErpEntityId, CrmType);
    CREATE INDEX IX_CrmIntegrationMappings_CrmEntityId ON CrmIntegrationMappings(TenantId, CrmEntityId, CrmType);
    CREATE INDEX IX_CrmIntegrationMappings_EntityType ON CrmIntegrationMappings(TenantId, EntityType, CrmType);
END;
GO

-- CRM Sync Logs
-- Tracks sync history for auditing and debugging
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrmSyncLogs')
BEGIN
    CREATE TABLE CrmSyncLogs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        CrmType NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(100) NULL,              -- NULL means full sync
        Direction NVARCHAR(20) NOT NULL,            -- 'ToErp', 'ToCrm', 'Bidirectional'
        RecordsProcessed INT NOT NULL DEFAULT 0,
        RecordsSucceeded INT NOT NULL DEFAULT 0,
        RecordsFailed INT NOT NULL DEFAULT 0,
        StartedAt DATETIME2 NOT NULL,
        CompletedAt DATETIME2 NULL,
        ErrorMessage NVARCHAR(MAX) NULL,
        ErrorDetails NVARCHAR(MAX) NULL,            -- JSON array of individual errors
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    -- Indexes for log queries
    CREATE INDEX IX_CrmSyncLogs_TenantId ON CrmSyncLogs(TenantId);
    CREATE INDEX IX_CrmSyncLogs_CrmType ON CrmSyncLogs(TenantId, CrmType);
    CREATE INDEX IX_CrmSyncLogs_StartedAt ON CrmSyncLogs(TenantId, StartedAt DESC);
END;
GO

-- Tenant CRM Credentials
-- Stores encrypted CRM credentials per tenant
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TenantCrmCredentials')
BEGIN
    CREATE TABLE TenantCrmCredentials (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        CrmType NVARCHAR(50) NOT NULL,              -- 'Salesforce' or 'Dynamics365'
        EncryptedCredentials NVARCHAR(MAX) NOT NULL,-- JSON encrypted with tenant key
        IsEnabled BIT NOT NULL DEFAULT 1,
        LastValidatedAt DATETIME2 NULL,
        ValidationStatus NVARCHAR(50) NULL,         -- 'Valid', 'Invalid', 'Expired'
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedAt DATETIME2 NULL,
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL
    );

    -- Unique constraint: one credential set per tenant per CRM type
    CREATE UNIQUE INDEX IX_TenantCrmCredentials_Unique
        ON TenantCrmCredentials(TenantId, CrmType);
END;
GO

-- CRM Field Mappings (optional advanced feature)
-- Allows custom field mapping between ERP and CRM
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrmFieldMappings')
BEGIN
    CREATE TABLE CrmFieldMappings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        CrmType NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(100) NOT NULL,
        ErpFieldName NVARCHAR(200) NOT NULL,        -- ERP entity property name
        CrmFieldName NVARCHAR(200) NOT NULL,        -- CRM field API name
        TransformType NVARCHAR(50) NULL,            -- 'Direct', 'Lookup', 'Custom'
        TransformConfig NVARCHAR(MAX) NULL,         -- JSON config for transforms
        IsEnabled BIT NOT NULL DEFAULT 1,
        SyncDirection NVARCHAR(20) NOT NULL DEFAULT 'Bidirectional',
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedAt DATETIME2 NULL
    );

    CREATE INDEX IX_CrmFieldMappings_TenantId
        ON CrmFieldMappings(TenantId, CrmType, EntityType);
END;
GO

-- CRM Sync Queue (for async processing)
-- Queues entities for background sync
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CrmSyncQueue')
BEGIN
    CREATE TABLE CrmSyncQueue (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        CrmType NVARCHAR(50) NOT NULL,
        EntityType NVARCHAR(100) NOT NULL,
        EntityId UNIQUEIDENTIFIER NOT NULL,
        Action NVARCHAR(20) NOT NULL,               -- 'Create', 'Update', 'Delete'
        Priority INT NOT NULL DEFAULT 0,            -- Higher = more urgent
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending', -- 'Pending', 'Processing', 'Completed', 'Failed'
        RetryCount INT NOT NULL DEFAULT 0,
        MaxRetries INT NOT NULL DEFAULT 3,
        LastError NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ProcessedAt DATETIME2 NULL
    );

    -- Index for queue processing
    CREATE INDEX IX_CrmSyncQueue_Pending
        ON CrmSyncQueue(TenantId, Status, Priority DESC, CreatedAt)
        WHERE Status = 'Pending';
END;
GO

PRINT 'CRM Integration tables created successfully';
