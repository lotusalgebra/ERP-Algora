-- ============================================
-- Algora ERP Admin Portal Database Schema
-- Creates tables for admin authentication,
-- tenant management, and billing
-- ============================================

SET QUOTED_IDENTIFIER ON;
GO

-- ============================================
-- Admin Roles Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminRoles]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminRoles] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Permissions] NVARCHAR(MAX) NULL, -- JSON array of permissions
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_AdminRoles] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_AdminRoles_Name] UNIQUE ([Name])
    );
    PRINT 'Created AdminRoles table';
END
GO

-- ============================================
-- Admin Users Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AdminUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[AdminUsers] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Email] NVARCHAR(256) NOT NULL,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [RoleId] UNIQUEIDENTIFIER NOT NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [EmailConfirmed] BIT NOT NULL DEFAULT 0,
        [PhoneNumber] NVARCHAR(20) NULL,
        [PhoneNumberConfirmed] BIT NOT NULL DEFAULT 0,
        [TwoFactorEnabled] BIT NOT NULL DEFAULT 0,
        [AccessFailedCount] INT NOT NULL DEFAULT 0,
        [LockoutEnd] DATETIME2 NULL,
        [LastLoginAt] DATETIME2 NULL,
        [LastLoginIp] NVARCHAR(45) NULL,
        [PasswordChangedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] UNIQUEIDENTIFIER NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_AdminUsers] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_AdminUsers_Email] UNIQUE ([Email]),
        CONSTRAINT [FK_AdminUsers_Role] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AdminRoles]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_AdminUsers_Email] ON [dbo].[AdminUsers]([Email]);
    CREATE NONCLUSTERED INDEX [IX_AdminUsers_RoleId] ON [dbo].[AdminUsers]([RoleId]);
    PRINT 'Created AdminUsers table';
END
GO

-- ============================================
-- Refresh Tokens Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Token] NVARCHAR(500) NOT NULL,
        [UserId] UNIQUEIDENTIFIER NOT NULL,
        [ExpiresAt] DATETIME2 NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedByIp] NVARCHAR(45) NULL,
        [RevokedAt] DATETIME2 NULL,
        [RevokedByIp] NVARCHAR(45) NULL,
        [ReplacedByToken] NVARCHAR(500) NULL,
        [ReasonRevoked] NVARCHAR(256) NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_RefreshTokens_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AdminUsers]([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens]([Token]);
    CREATE NONCLUSTERED INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens]([UserId]);
    PRINT 'Created RefreshTokens table';
END
GO

-- ============================================
-- Billing Plans Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BillingPlans]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[BillingPlans] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Code] NVARCHAR(50) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [MonthlyPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [AnnualPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [MaxUsers] INT NOT NULL DEFAULT 5,
        [MaxWarehouses] INT NOT NULL DEFAULT 1,
        [MaxProducts] INT NOT NULL DEFAULT 100,
        [MaxMonthlyTransactions] INT NOT NULL DEFAULT 100,
        [StorageLimitMb] INT NOT NULL DEFAULT 1000,
        [Features] NVARCHAR(MAX) NULL, -- JSON array of features
        [IncludedModules] NVARCHAR(MAX) NULL, -- JSON array of modules
        [IsActive] BIT NOT NULL DEFAULT 1,
        [IsFeatured] BIT NOT NULL DEFAULT 0,
        [SortOrder] INT NOT NULL DEFAULT 0,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] UNIQUEIDENTIFIER NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] UNIQUEIDENTIFIER NULL,
        CONSTRAINT [PK_BillingPlans] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_BillingPlans_Code] UNIQUE ([Code])
    );

    CREATE NONCLUSTERED INDEX [IX_BillingPlans_IsActive] ON [dbo].[BillingPlans]([IsActive]);
    PRINT 'Created BillingPlans table';
END
GO

-- ============================================
-- Tenants Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Tenants]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Tenants] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [Name] NVARCHAR(200) NOT NULL,
        [Subdomain] NVARCHAR(50) NOT NULL,
        [DatabaseName] NVARCHAR(100) NULL,
        [ConnectionString] NVARCHAR(500) NULL,
        [LogoUrl] NVARCHAR(500) NULL,
        [ContactEmail] NVARCHAR(256) NOT NULL,
        [ContactPhone] NVARCHAR(20) NULL,
        [Address] NVARCHAR(500) NULL,
        [City] NVARCHAR(100) NULL,
        [State] NVARCHAR(100) NULL,
        [Country] NVARCHAR(100) NULL,
        [PostalCode] NVARCHAR(20) NULL,
        [Status] INT NOT NULL DEFAULT 1,
        [CurrentSubscriptionId] UNIQUEIDENTIFIER NULL,

        -- Soft Delete fields
        [IsDeleted] BIT NOT NULL DEFAULT 0,
        [DeletedAt] DATETIME2 NULL,
        [DeletedBy] UNIQUEIDENTIFIER NULL,
        [DeletionReason] NVARCHAR(500) NULL,

        -- Suspension fields
        [IsSuspended] BIT NOT NULL DEFAULT 0,
        [SuspendedAt] DATETIME2 NULL,
        [SuspendedBy] UNIQUEIDENTIFIER NULL,
        [SuspensionReason] NVARCHAR(500) NULL,

        -- Audit fields
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [CreatedBy] UNIQUEIDENTIFIER NULL,
        [UpdatedAt] DATETIME2 NULL,
        [UpdatedBy] UNIQUEIDENTIFIER NULL,

        CONSTRAINT [PK_Tenants] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [UQ_Tenants_Subdomain] UNIQUE ([Subdomain])
    );

    CREATE NONCLUSTERED INDEX [IX_Tenants_Status] ON [dbo].[Tenants]([Status]);
    CREATE NONCLUSTERED INDEX [IX_Tenants_IsDeleted] ON [dbo].[Tenants]([IsDeleted]) WHERE [IsDeleted] = 0;
    CREATE NONCLUSTERED INDEX [IX_Tenants_IsSuspended] ON [dbo].[Tenants]([IsSuspended]);
    PRINT 'Created Tenants table';
END
GO

-- ============================================
-- Tenant Users Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantUsers] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [Email] NVARCHAR(256) NOT NULL,
        [FirstName] NVARCHAR(100) NOT NULL,
        [LastName] NVARCHAR(100) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL DEFAULT 'User',
        [IsOwner] BIT NOT NULL DEFAULT 0,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [LastLoginAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT [PK_TenantUsers] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_TenantUsers_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]) ON DELETE CASCADE,
        CONSTRAINT [UQ_TenantUsers_Email_Tenant] UNIQUE ([TenantId], [Email])
    );

    CREATE NONCLUSTERED INDEX [IX_TenantUsers_TenantId] ON [dbo].[TenantUsers]([TenantId]);
    PRINT 'Created TenantUsers table';
END
GO

-- ============================================
-- Tenant Subscriptions Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantSubscriptions]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantSubscriptions] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [PlanId] UNIQUEIDENTIFIER NOT NULL,
        [BillingCycle] INT NOT NULL DEFAULT 0, -- 0=Monthly, 1=Quarterly, 2=SemiAnnual, 3=Annual
        [Status] INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Trialing, 2=Active, 3=PastDue, 4=Cancelled, 5=PendingCancellation
        [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Currency] NVARCHAR(3) NOT NULL DEFAULT 'USD',
        [TrialEndsAt] DATETIME2 NULL,
        [CurrentPeriodStart] DATETIME2 NULL,
        [CurrentPeriodEnd] DATETIME2 NULL,
        [CancelledAt] DATETIME2 NULL,
        [CancellationReason] NVARCHAR(500) NULL,
        [AutoRenew] BIT NOT NULL DEFAULT 1,
        [ExternalId] NVARCHAR(100) NULL, -- Stripe/external billing ID
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_TenantSubscriptions] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_TenantSubscriptions_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_TenantSubscriptions_Plan] FOREIGN KEY ([PlanId]) REFERENCES [dbo].[BillingPlans]([Id])
    );

    CREATE NONCLUSTERED INDEX [IX_TenantSubscriptions_TenantId] ON [dbo].[TenantSubscriptions]([TenantId]);
    CREATE NONCLUSTERED INDEX [IX_TenantSubscriptions_PlanId] ON [dbo].[TenantSubscriptions]([PlanId]);
    CREATE NONCLUSTERED INDEX [IX_TenantSubscriptions_Status] ON [dbo].[TenantSubscriptions]([Status]);
    PRINT 'Created TenantSubscriptions table';
END
GO

-- Add FK from Tenants to TenantSubscriptions
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Tenants_CurrentSubscription]'))
BEGIN
    ALTER TABLE [dbo].[Tenants]
    ADD CONSTRAINT [FK_Tenants_CurrentSubscription] FOREIGN KEY ([CurrentSubscriptionId])
    REFERENCES [dbo].[TenantSubscriptions]([Id]);
    PRINT 'Added CurrentSubscription FK to Tenants';
END
GO

-- ============================================
-- Tenant Billing Invoices Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantBillingInvoices]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantBillingInvoices] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [TenantId] UNIQUEIDENTIFIER NOT NULL,
        [SubscriptionId] UNIQUEIDENTIFIER NOT NULL,
        [InvoiceNumber] NVARCHAR(50) NOT NULL,
        [InvoiceDate] DATETIME2 NOT NULL,
        [DueDate] DATETIME2 NOT NULL,
        [Status] INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Sent, 2=Paid, 3=PartiallyPaid, 4=Overdue, 5=Cancelled, 6=Refunded
        [Subtotal] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Tax] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Discount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Total] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [AmountPaid] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Currency] NVARCHAR(3) NOT NULL DEFAULT 'USD',
        [PaidAt] DATETIME2 NULL,
        [PaymentMethod] NVARCHAR(50) NULL,
        [ExternalId] NVARCHAR(100) NULL,
        [Notes] NVARCHAR(1000) NULL,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NULL,
        CONSTRAINT [PK_TenantBillingInvoices] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_TenantBillingInvoices_Tenant] FOREIGN KEY ([TenantId]) REFERENCES [dbo].[Tenants]([Id]),
        CONSTRAINT [FK_TenantBillingInvoices_Subscription] FOREIGN KEY ([SubscriptionId]) REFERENCES [dbo].[TenantSubscriptions]([Id]),
        CONSTRAINT [UQ_TenantBillingInvoices_Number] UNIQUE ([InvoiceNumber])
    );

    CREATE NONCLUSTERED INDEX [IX_TenantBillingInvoices_TenantId] ON [dbo].[TenantBillingInvoices]([TenantId]);
    CREATE NONCLUSTERED INDEX [IX_TenantBillingInvoices_Status] ON [dbo].[TenantBillingInvoices]([Status]);
    PRINT 'Created TenantBillingInvoices table';
END
GO

-- ============================================
-- Tenant Billing Invoice Lines Table
-- ============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TenantBillingInvoiceLines]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TenantBillingInvoiceLines] (
        [Id] UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [InvoiceId] UNIQUEIDENTIFIER NOT NULL,
        [Description] NVARCHAR(500) NOT NULL,
        [Quantity] DECIMAL(18,4) NOT NULL DEFAULT 1,
        [UnitPrice] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [Amount] DECIMAL(18,2) NOT NULL DEFAULT 0,
        [PeriodStart] DATETIME2 NULL,
        [PeriodEnd] DATETIME2 NULL,
        CONSTRAINT [PK_TenantBillingInvoiceLines] PRIMARY KEY CLUSTERED ([Id]),
        CONSTRAINT [FK_TenantBillingInvoiceLines_Invoice] FOREIGN KEY ([InvoiceId]) REFERENCES [dbo].[TenantBillingInvoices]([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_TenantBillingInvoiceLines_InvoiceId] ON [dbo].[TenantBillingInvoiceLines]([InvoiceId]);
    PRINT 'Created TenantBillingInvoiceLines table';
END
GO

-- ============================================
-- Seed Default Data
-- ============================================

-- Seed default roles
IF NOT EXISTS (SELECT * FROM [dbo].[AdminRoles] WHERE [Name] = 'SuperAdmin')
BEGIN
    INSERT INTO [dbo].[AdminRoles] ([Id], [Name], [Description], [Permissions])
    VALUES
        (NEWID(), 'SuperAdmin', 'Full system access', '["*"]'),
        (NEWID(), 'Admin', 'Administrative access', '["tenants.view", "tenants.create", "tenants.edit", "plans.view", "users.view"]'),
        (NEWID(), 'Support', 'Support team access', '["tenants.view", "plans.view"]');
    PRINT 'Seeded default admin roles';
END
GO

-- Seed default admin user (password: Admin@123)
IF NOT EXISTS (SELECT * FROM [dbo].[AdminUsers] WHERE [Email] = 'admin@algora.com')
BEGIN
    DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER;
    SELECT @SuperAdminRoleId = [Id] FROM [dbo].[AdminRoles] WHERE [Name] = 'SuperAdmin';

    INSERT INTO [dbo].[AdminUsers] ([Id], [Email], [PasswordHash], [FirstName], [LastName], [RoleId], [IsActive], [EmailConfirmed])
    VALUES (NEWID(), 'admin@algora.com', '$2a$11$rBbBQMa1kLU/nB4YmY1Jt.1gA7rVwNwsIHYCGKqJ6SFKlzz3oFnlC', 'System', 'Administrator', @SuperAdminRoleId, 1, 1);
    PRINT 'Seeded default admin user: admin@algora.com / Admin@123';
END
GO

-- Seed default billing plans
IF NOT EXISTS (SELECT * FROM [dbo].[BillingPlans] WHERE [Code] = 'FREE')
BEGIN
    INSERT INTO [dbo].[BillingPlans] ([Id], [Code], [Name], [Description], [MonthlyPrice], [AnnualPrice], [MaxUsers], [MaxWarehouses], [MaxProducts], [MaxMonthlyTransactions], [StorageLimitMb], [Features], [IncludedModules], [IsActive], [IsFeatured], [SortOrder])
    VALUES
        (NEWID(), 'FREE', 'Free', 'Get started with basic features', 0, 0, 2, 1, 100, 50, 500, '["Basic Inventory", "Simple Invoicing", "Email Support"]', '["inventory", "sales", "customers"]', 1, 0, 1),
        (NEWID(), 'STARTER', 'Starter', 'For small businesses getting started', 49, 490, 5, 2, 500, 200, 2000, '["Full Inventory", "Invoicing & Payments", "Basic Reports", "Email Support"]', '["inventory", "sales", "customers", "finance", "reports"]', 1, 0, 2),
        (NEWID(), 'PROFESSIONAL', 'Professional', 'For growing businesses', 149, 1490, 15, 5, 2000, 1000, 10000, '["Full Inventory", "Multi-Warehouse", "Advanced Reports", "Payroll", "Priority Support"]', '["inventory", "sales", "customers", "finance", "hr", "payroll", "reports", "procurement"]', 1, 1, 3),
        (NEWID(), 'ENTERPRISE', 'Enterprise', 'For large organizations', 399, 3990, -1, -1, -1, -1, -1, '["Unlimited Everything", "Manufacturing", "Projects", "Custom Reports", "API Access", "Dedicated Support", "SLA"]', '["*"]', 1, 0, 4);
    PRINT 'Seeded default billing plans';
END
GO

PRINT '============================================';
PRINT 'Algora ERP Admin database schema created successfully!';
PRINT '============================================';
GO
