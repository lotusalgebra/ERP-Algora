-- Algora ERP Master Database Schema
-- This script creates the master database tables for tenant management

SET QUOTED_IDENTIFIER ON
GO

-- Create Tenants table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tenants' AND xtype='U')
BEGIN
    CREATE TABLE Tenants (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(200) NOT NULL,
        Subdomain NVARCHAR(100) NOT NULL,
        ConnectionString NVARCHAR(500) NOT NULL,
        DatabaseName NVARCHAR(100) NOT NULL,
        LogoUrl NVARCHAR(500) NULL,
        PrimaryColor NVARCHAR(20) NULL,
        ContactEmail NVARCHAR(255) NOT NULL,
        ContactPhone NVARCHAR(50) NULL,
        [Address] NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        [State] NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,
        TaxId NVARCHAR(50) NULL,
        CurrencyCode NVARCHAR(3) NOT NULL DEFAULT 'USD',
        TimeZone NVARCHAR(100) NOT NULL DEFAULT 'UTC',
        [Status] INT NOT NULL DEFAULT 1, -- 0=Pending, 1=Active, 2=Suspended, 3=Cancelled
        [Plan] INT NOT NULL DEFAULT 1, -- 0=Free, 1=Basic, 2=Professional, 3=Enterprise
        TrialEndsAt DATETIME2 NULL,
        SubscriptionEndsAt DATETIME2 NULL,
        MaxUsers INT NOT NULL DEFAULT 5,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedAt DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT UQ_Tenants_Subdomain UNIQUE (Subdomain)
    );

    CREATE INDEX IX_Tenants_Subdomain ON Tenants(Subdomain);
    CREATE INDEX IX_Tenants_IsActive ON Tenants(IsActive);
    CREATE INDEX IX_Tenants_Status ON Tenants([Status]);

    PRINT 'Created Tenants table';
END
GO

-- Create TenantUsers table (for super admins and multi-tenant users)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenantUsers' AND xtype='U')
BEGIN
    CREATE TABLE TenantUsers (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        IsSuperAdmin BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2 NULL,
        CONSTRAINT FK_TenantUsers_Tenants FOREIGN KEY (TenantId) REFERENCES Tenants(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_TenantUsers_TenantId_Email UNIQUE (TenantId, Email)
    );

    CREATE INDEX IX_TenantUsers_TenantId ON TenantUsers(TenantId);
    CREATE INDEX IX_TenantUsers_Email ON TenantUsers(Email);

    PRINT 'Created TenantUsers table';
END
GO

-- Seed default tenant (for development/testing)
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Subdomain = 'demo')
BEGIN
    DECLARE @TenantId UNIQUEIDENTIFIER = NEWID();

    INSERT INTO Tenants (Id, Name, Subdomain, ConnectionString, DatabaseName, ContactEmail, CurrencyCode, TimeZone, [Status], [Plan], MaxUsers)
    VALUES (
        @TenantId,
        'Demo Company',
        'demo',
        'Server=(localdb)\mssqllocaldb;Database=AlgoraErp_Demo;Trusted_Connection=True;MultipleActiveResultSets=true',
        'AlgoraErp_Demo',
        'admin@demo.algoraerp.com',
        'USD',
        'America/New_York',
        1, -- Active
        2, -- Professional
        50
    );

    -- Create default super admin for demo tenant
    INSERT INTO TenantUsers (TenantId, Email, FirstName, LastName, PasswordHash, IsSuperAdmin, IsActive)
    VALUES (
        @TenantId,
        'admin@demo.algoraerp.com',
        'Admin',
        'User',
        -- Password: Admin123! (hashed with BCrypt - placeholder, should be properly hashed)
        '$2a$11$rZQpLgJF7pY6LvS7C3K8Ne8xqZL4xHJz1y2P4kK2FhMnQ5nL8mHKy',
        1,
        1
    );

    PRINT 'Seeded demo tenant and admin user';
END
GO

PRINT 'Master database schema created successfully';
GO
