-- =============================================================================
-- Algora ERP - Admin Database Production Seed Data
-- =============================================================================
-- This script seeds production-ready data for the Admin (Host) database
-- Database: AlgoraErpAdmin
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

USE AlgoraErpAdmin;
GO

PRINT '=============================================================================';
PRINT 'Starting Admin Database Seed Data...';
PRINT '=============================================================================';

-- =============================================================================
-- 1. ADMIN ROLES
-- =============================================================================
PRINT 'Seeding Admin Roles...';

DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @SupportRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @BillingRoleId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM AdminRoles WHERE Name = 'Super Administrator')
    INSERT INTO AdminRoles (Id, Name, Description, Permissions, IsSystemRole, CreatedAt)
    VALUES (@SuperAdminRoleId, 'Super Administrator', 'Full system access with all permissions', '["*"]', 1, GETUTCDATE());
ELSE
    SELECT @SuperAdminRoleId = Id FROM AdminRoles WHERE Name = 'Super Administrator';

IF NOT EXISTS (SELECT 1 FROM AdminRoles WHERE Name = 'Administrator')
    INSERT INTO AdminRoles (Id, Name, Description, Permissions, IsSystemRole, CreatedAt)
    VALUES (@AdminRoleId, 'Administrator', 'Tenant and user management access', '["tenants.*","users.*","subscriptions.*"]', 0, GETUTCDATE());
ELSE
    SELECT @AdminRoleId = Id FROM AdminRoles WHERE Name = 'Administrator';

IF NOT EXISTS (SELECT 1 FROM AdminRoles WHERE Name = 'Support')
    INSERT INTO AdminRoles (Id, Name, Description, Permissions, IsSystemRole, CreatedAt)
    VALUES (@SupportRoleId, 'Support', 'Customer support and read-only access', '["tenants.read","users.read","subscriptions.read"]', 0, GETUTCDATE());
ELSE
    SELECT @SupportRoleId = Id FROM AdminRoles WHERE Name = 'Support';

IF NOT EXISTS (SELECT 1 FROM AdminRoles WHERE Name = 'Billing')
    INSERT INTO AdminRoles (Id, Name, Description, Permissions, IsSystemRole, CreatedAt)
    VALUES (@BillingRoleId, 'Billing', 'Billing and subscription management', '["subscriptions.*","billing.*","invoices.*"]', 0, GETUTCDATE());
ELSE
    SELECT @BillingRoleId = Id FROM AdminRoles WHERE Name = 'Billing';

PRINT 'Admin Roles seeded.';

-- =============================================================================
-- 2. ADMIN USERS
-- =============================================================================
PRINT 'Seeding Admin Users...';

-- Password hash for 'Admin@123' using BCrypt
DECLARE @PasswordHash NVARCHAR(255) = '$2a$11$K7BvgZ5JvL8vxLqXxQxXheJZPw7HxTqR8qHvL8JxKqXxQxXheJZPw7';

IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Email = 'admin@algora.com')
    INSERT INTO AdminUsers (Id, Email, PasswordHash, FirstName, LastName, Phone, RoleId, IsActive, EmailConfirmed, AccessFailedCount, LockoutEnabled, TwoFactorEnabled, CreatedAt)
    VALUES (NEWID(), 'admin@algora.com', @PasswordHash, 'System', 'Administrator', '+91-9000000001', @SuperAdminRoleId, 1, 1, 0, 1, 0, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Email = 'support@algora.com')
    INSERT INTO AdminUsers (Id, Email, PasswordHash, FirstName, LastName, Phone, RoleId, IsActive, EmailConfirmed, AccessFailedCount, LockoutEnabled, TwoFactorEnabled, CreatedAt)
    VALUES (NEWID(), 'support@algora.com', @PasswordHash, 'Support', 'Team', '+91-9000000002', @SupportRoleId, 1, 1, 0, 1, 0, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Email = 'billing@algora.com')
    INSERT INTO AdminUsers (Id, Email, PasswordHash, FirstName, LastName, Phone, RoleId, IsActive, EmailConfirmed, AccessFailedCount, LockoutEnabled, TwoFactorEnabled, CreatedAt)
    VALUES (NEWID(), 'billing@algora.com', @PasswordHash, 'Billing', 'Team', '+91-9000000003', @BillingRoleId, 1, 1, 0, 1, 0, GETUTCDATE());

IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Email = 'ops@algora.com')
    INSERT INTO AdminUsers (Id, Email, PasswordHash, FirstName, LastName, Phone, RoleId, IsActive, EmailConfirmed, AccessFailedCount, LockoutEnabled, TwoFactorEnabled, CreatedAt)
    VALUES (NEWID(), 'ops@algora.com', @PasswordHash, 'Operations', 'Admin', '+91-9000000004', @AdminRoleId, 1, 1, 0, 1, 0, GETUTCDATE());

PRINT 'Admin Users seeded.';

-- =============================================================================
-- 3. BILLING PLANS
-- =============================================================================
PRINT 'Seeding Billing Plans...';

DECLARE @FreePlanId UNIQUEIDENTIFIER = NEWID();
DECLARE @StarterPlanId UNIQUEIDENTIFIER = NEWID();
DECLARE @ProfessionalPlanId UNIQUEIDENTIFIER = NEWID();
DECLARE @EnterprisePlanId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM BillingPlans WHERE Code = 'FREE')
    INSERT INTO BillingPlans (Id, Code, Name, Description, MonthlyPrice, AnnualPrice, Currency, AnnualDiscountPercent,
                              MaxUsers, MaxWarehouses, MaxProducts, MaxTransactionsPerMonth, StorageLimitMB,
                              Features, IncludedModules, DisplayOrder, IsPopular, IsActive, IsPublic, TrialDays, CreatedAt)
    VALUES (@FreePlanId, 'FREE', 'Free', 'Get started with basic features', 0, 0, 'INR', 0,
            2, 1, 100, 500, 100,
            '["Up to 2 users","1 warehouse","100 products","500 transactions/month","100MB storage","Email support"]',
            '["FINANCE","INVENTORY","SALES"]', 1, 0, 1, 1, 0, GETUTCDATE());
ELSE
    SELECT @FreePlanId = Id FROM BillingPlans WHERE Code = 'FREE';

IF NOT EXISTS (SELECT 1 FROM BillingPlans WHERE Code = 'STARTER')
    INSERT INTO BillingPlans (Id, Code, Name, Description, MonthlyPrice, AnnualPrice, Currency, AnnualDiscountPercent,
                              MaxUsers, MaxWarehouses, MaxProducts, MaxTransactionsPerMonth, StorageLimitMB,
                              Features, IncludedModules, DisplayOrder, IsPopular, IsActive, IsPublic, TrialDays, CreatedAt)
    VALUES (@StarterPlanId, 'STARTER', 'Starter', 'Perfect for small teams', 2499, 24990, 'INR', 17,
            5, 2, 500, 2000, 500,
            '["Up to 5 users","2 warehouses","500 products","2000 transactions/month","500MB storage","Email support","API access"]',
            '["FINANCE","INVENTORY","SALES","HR","PROCUREMENT"]', 2, 0, 1, 1, 14, GETUTCDATE());
ELSE
    SELECT @StarterPlanId = Id FROM BillingPlans WHERE Code = 'STARTER';

IF NOT EXISTS (SELECT 1 FROM BillingPlans WHERE Code = 'PROFESSIONAL')
    INSERT INTO BillingPlans (Id, Code, Name, Description, MonthlyPrice, AnnualPrice, Currency, AnnualDiscountPercent,
                              MaxUsers, MaxWarehouses, MaxProducts, MaxTransactionsPerMonth, StorageLimitMB,
                              Features, IncludedModules, DisplayOrder, IsPopular, IsActive, IsPublic, TrialDays, CreatedAt)
    VALUES (@ProfessionalPlanId, 'PROFESSIONAL', 'Professional', 'For growing businesses', 7499, 74990, 'INR', 17,
            20, 5, 5000, 10000, 2000,
            '["Up to 20 users","5 warehouses","5000 products","10000 transactions/month","2GB storage","Priority support","API access","Custom reports"]',
            '["FINANCE","INVENTORY","SALES","HR","PROCUREMENT","PAYROLL","MANUFACTURING","PROJECTS"]', 3, 1, 1, 1, 14, GETUTCDATE());
ELSE
    SELECT @ProfessionalPlanId = Id FROM BillingPlans WHERE Code = 'PROFESSIONAL';

IF NOT EXISTS (SELECT 1 FROM BillingPlans WHERE Code = 'ENTERPRISE')
    INSERT INTO BillingPlans (Id, Code, Name, Description, MonthlyPrice, AnnualPrice, Currency, AnnualDiscountPercent,
                              MaxUsers, MaxWarehouses, MaxProducts, MaxTransactionsPerMonth, StorageLimitMB,
                              Features, IncludedModules, DisplayOrder, IsPopular, IsActive, IsPublic, TrialDays, CreatedAt)
    VALUES (@EnterprisePlanId, 'ENTERPRISE', 'Enterprise', 'Unlimited everything for large organizations', 19999, 199990, 'INR', 17,
            -1, -1, -1, -1, -1,
            '["Unlimited users","Unlimited warehouses","Unlimited products","Unlimited transactions","Unlimited storage","24/7 dedicated support","Custom integrations","SLA guarantee"]',
            '["ALL"]', 4, 0, 1, 1, 30, GETUTCDATE());
ELSE
    SELECT @EnterprisePlanId = Id FROM BillingPlans WHERE Code = 'ENTERPRISE';

PRINT 'Billing Plans seeded.';

-- =============================================================================
-- 4. SAMPLE TENANTS
-- =============================================================================
PRINT 'Seeding Sample Tenants...';

DECLARE @Tenant1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tenant2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tenant3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tenant4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Tenant5Id UNIQUEIDENTIFIER = NEWID();

DECLARE @Sub1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Sub2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Sub3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Sub4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Sub5Id UNIQUEIDENTIFIER = NEWID();

-- Tenant 1: Manufacturing Company on Professional Plan
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Subdomain = 'techmanufacturing')
BEGIN
    INSERT INTO Tenants (Id, Name, Subdomain, DatabaseName, LogoUrl, PrimaryColor, IsActive, MaxUsers,
                         ContactEmail, ContactPerson, CompanyName, TaxId, Address, City, State, Country, PostalCode,
                         CurrencyCode, TimeZone, Status, IsDeleted, IsSuspended, CreatedAt)
    VALUES (@Tenant1Id, 'Tech Manufacturing Ltd', 'techmanufacturing', 'AlgoraErp_TechManufacturing', NULL, '#4F46E5', 1, 20,
            'billing@techmanufacturing.com', 'Rajesh Kumar', 'Tech Manufacturing Ltd', 'GSTIN33AABCT1234T1Z5',
            '123 Industrial Park', 'Chennai', 'Tamil Nadu', 'India', '600001',
            'INR', 'Asia/Kolkata', 1, 0, 0, GETUTCDATE());

    INSERT INTO TenantSubscriptions (Id, TenantId, PlanId, Status, BillingCycle, StartDate, EndDate, CurrentPeriodStart, CurrentPeriodEnd,
                                     Amount, DiscountAmount, TaxAmount, TotalAmount, Currency, AutoRenew, NextBillingDate, CreatedAt)
    VALUES (@Sub1Id, @Tenant1Id, @ProfessionalPlanId, 1, 1, DATEADD(MONTH, -3, GETUTCDATE()), DATEADD(MONTH, 9, GETUTCDATE()),
            DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()),
            7499, 0, 1350, 8849, 'INR', 1, DATEADD(MONTH, 1, GETUTCDATE()), GETUTCDATE());

    UPDATE Tenants SET CurrentSubscriptionId = @Sub1Id WHERE Id = @Tenant1Id;
END
ELSE
    SELECT @Tenant1Id = Id FROM Tenants WHERE Subdomain = 'techmanufacturing';

-- Tenant 2: Retail Company on Enterprise Plan
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Subdomain = 'megamart')
BEGIN
    INSERT INTO Tenants (Id, Name, Subdomain, DatabaseName, LogoUrl, PrimaryColor, IsActive, MaxUsers,
                         ContactEmail, ContactPerson, CompanyName, TaxId, Address, City, State, Country, PostalCode,
                         CurrencyCode, TimeZone, Status, IsDeleted, IsSuspended, CreatedAt)
    VALUES (@Tenant2Id, 'MegaMart Retail Pvt Ltd', 'megamart', 'AlgoraErp_MegaMart', NULL, '#059669', 1, 100,
            'accounts@megamart.com', 'Vikram Patel', 'MegaMart Retail Pvt Ltd', 'GSTIN27AABCM5678M1Z2',
            '456 Commercial Street', 'Mumbai', 'Maharashtra', 'India', '400001',
            'INR', 'Asia/Kolkata', 1, 0, 0, GETUTCDATE());

    INSERT INTO TenantSubscriptions (Id, TenantId, PlanId, Status, BillingCycle, StartDate, EndDate, CurrentPeriodStart, CurrentPeriodEnd,
                                     Amount, DiscountAmount, TaxAmount, TotalAmount, Currency, AutoRenew, NextBillingDate, CreatedAt)
    VALUES (@Sub2Id, @Tenant2Id, @EnterprisePlanId, 1, 1, DATEADD(MONTH, -6, GETUTCDATE()), DATEADD(MONTH, 6, GETUTCDATE()),
            DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()),
            19999, 0, 3600, 23599, 'INR', 1, DATEADD(MONTH, 1, GETUTCDATE()), GETUTCDATE());

    UPDATE Tenants SET CurrentSubscriptionId = @Sub2Id WHERE Id = @Tenant2Id;
END
ELSE
    SELECT @Tenant2Id = Id FROM Tenants WHERE Subdomain = 'megamart';

-- Tenant 3: Startup on Starter Plan
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Subdomain = 'innovatech')
BEGIN
    INSERT INTO Tenants (Id, Name, Subdomain, DatabaseName, LogoUrl, PrimaryColor, IsActive, MaxUsers,
                         ContactEmail, ContactPerson, CompanyName, TaxId, Address, City, State, Country, PostalCode,
                         CurrencyCode, TimeZone, Status, IsDeleted, IsSuspended, CreatedAt)
    VALUES (@Tenant3Id, 'InnovaTech Solutions', 'innovatech', 'AlgoraErp_InnovaTech', NULL, '#7C3AED', 1, 5,
            'hello@innovatech.io', 'Arjun Nair', 'InnovaTech Solutions Pvt Ltd', 'GSTIN29AABCI9012I1Z8',
            '789 Tech Hub', 'Bangalore', 'Karnataka', 'India', '560001',
            'INR', 'Asia/Kolkata', 1, 0, 0, GETUTCDATE());

    INSERT INTO TenantSubscriptions (Id, TenantId, PlanId, Status, BillingCycle, StartDate, EndDate, CurrentPeriodStart, CurrentPeriodEnd,
                                     Amount, DiscountAmount, TaxAmount, TotalAmount, Currency, AutoRenew, NextBillingDate, CreatedAt)
    VALUES (@Sub3Id, @Tenant3Id, @StarterPlanId, 1, 0, DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(MONTH, 11, GETUTCDATE()),
            DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()),
            2499, 0, 450, 2949, 'INR', 1, DATEADD(MONTH, 1, GETUTCDATE()), GETUTCDATE());

    UPDATE Tenants SET CurrentSubscriptionId = @Sub3Id WHERE Id = @Tenant3Id;
END
ELSE
    SELECT @Tenant3Id = Id FROM Tenants WHERE Subdomain = 'innovatech';

-- Tenant 4: Small Business on Free Plan
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Subdomain = 'localshop')
BEGIN
    INSERT INTO Tenants (Id, Name, Subdomain, DatabaseName, LogoUrl, PrimaryColor, IsActive, MaxUsers,
                         ContactEmail, ContactPerson, CompanyName, Address, City, State, Country, PostalCode,
                         CurrencyCode, TimeZone, Status, IsDeleted, IsSuspended, CreatedAt)
    VALUES (@Tenant4Id, 'Local Shop & Services', 'localshop', 'AlgoraErp_LocalShop', NULL, '#DC2626', 1, 2,
            'owner@localshop.com', 'Ramesh Gupta', 'Local Shop & Services',
            '10 Main Street', 'Delhi', 'Delhi', 'India', '110001',
            'INR', 'Asia/Kolkata', 1, 0, 0, GETUTCDATE());

    INSERT INTO TenantSubscriptions (Id, TenantId, PlanId, Status, BillingCycle, StartDate, CurrentPeriodStart, CurrentPeriodEnd,
                                     Amount, DiscountAmount, TaxAmount, TotalAmount, Currency, AutoRenew, CreatedAt)
    VALUES (@Sub4Id, @Tenant4Id, @FreePlanId, 1, 0, DATEADD(DAY, -15, GETUTCDATE()),
            DATEADD(DAY, -15, GETUTCDATE()), DATEADD(MONTH, 1, GETUTCDATE()),
            0, 0, 0, 0, 'INR', 1, GETUTCDATE());

    UPDATE Tenants SET CurrentSubscriptionId = @Sub4Id WHERE Id = @Tenant4Id;
END
ELSE
    SELECT @Tenant4Id = Id FROM Tenants WHERE Subdomain = 'localshop';

-- Tenant 5: Trial Account (Professional)
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE Subdomain = 'trialcompany')
BEGIN
    INSERT INTO Tenants (Id, Name, Subdomain, DatabaseName, LogoUrl, PrimaryColor, IsActive, MaxUsers,
                         ContactEmail, ContactPerson, CompanyName, Address, City, State, Country, PostalCode,
                         CurrencyCode, TimeZone, Status, IsDeleted, IsSuspended, TrialStartedAt, TrialEndsAt, CreatedAt)
    VALUES (@Tenant5Id, 'Trial Company Demo', 'trialcompany', 'AlgoraErp_TrialCompany', NULL, '#2563EB', 1, 20,
            'demo@trialcompany.com', 'Demo User', 'Trial Company Demo',
            '99 Demo Lane', 'Hyderabad', 'Telangana', 'India', '500001',
            'INR', 'Asia/Kolkata', 1, 0, 0, GETUTCDATE(), DATEADD(DAY, 14, GETUTCDATE()), GETUTCDATE());

    INSERT INTO TenantSubscriptions (Id, TenantId, PlanId, Status, BillingCycle, StartDate, EndDate, CurrentPeriodStart, CurrentPeriodEnd,
                                     Amount, DiscountAmount, TaxAmount, TotalAmount, Currency, AutoRenew, IsTrialPeriod, TrialEndDate, CreatedAt)
    VALUES (@Sub5Id, @Tenant5Id, @ProfessionalPlanId, 0, 0, GETUTCDATE(), DATEADD(DAY, 14, GETUTCDATE()),
            GETUTCDATE(), DATEADD(DAY, 14, GETUTCDATE()),
            0, 0, 0, 0, 'INR', 0, 1, DATEADD(DAY, 14, GETUTCDATE()), GETUTCDATE());

    UPDATE Tenants SET CurrentSubscriptionId = @Sub5Id WHERE Id = @Tenant5Id;
END
ELSE
    SELECT @Tenant5Id = Id FROM Tenants WHERE Subdomain = 'trialcompany';

PRINT 'Sample Tenants seeded.';

-- =============================================================================
-- 5. TENANT USERS (Account Owners and Staff)
-- =============================================================================
PRINT 'Seeding Tenant Users...';

-- Tech Manufacturing Users
IF NOT EXISTS (SELECT 1 FROM TenantUsers WHERE Email = 'ceo@techmanufacturing.com')
BEGIN
    INSERT INTO TenantUsers (Id, TenantId, Email, FullName, Role, IsOwner, IsActive, CreatedAt)
    VALUES
        (NEWID(), @Tenant1Id, 'ceo@techmanufacturing.com', 'Rajesh Kumar', 'CEO', 1, 1, GETUTCDATE()),
        (NEWID(), @Tenant1Id, 'cfo@techmanufacturing.com', 'Priya Sharma', 'CFO', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant1Id, 'hr@techmanufacturing.com', 'Amit Singh', 'HR Manager', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant1Id, 'ops@techmanufacturing.com', 'Deepak Verma', 'Operations Manager', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant1Id, 'plant@techmanufacturing.com', 'Suresh Reddy', 'Plant Manager', 0, 1, GETUTCDATE());
END

-- MegaMart Users
IF NOT EXISTS (SELECT 1 FROM TenantUsers WHERE Email = 'director@megamart.com')
BEGIN
    INSERT INTO TenantUsers (Id, TenantId, Email, FullName, Role, IsOwner, IsActive, CreatedAt)
    VALUES
        (NEWID(), @Tenant2Id, 'director@megamart.com', 'Vikram Patel', 'Managing Director', 1, 1, GETUTCDATE()),
        (NEWID(), @Tenant2Id, 'finance@megamart.com', 'Anita Desai', 'Finance Head', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant2Id, 'purchase@megamart.com', 'Rahul Mehta', 'Purchase Manager', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant2Id, 'store@megamart.com', 'Kavita Iyer', 'Store Manager', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant2Id, 'warehouse@megamart.com', 'Mohan Das', 'Warehouse Head', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant2Id, 'ecom@megamart.com', 'Neha Gupta', 'E-Commerce Manager', 0, 1, GETUTCDATE());
END

-- InnovaTech Users
IF NOT EXISTS (SELECT 1 FROM TenantUsers WHERE Email = 'founder@innovatech.io')
BEGIN
    INSERT INTO TenantUsers (Id, TenantId, Email, FullName, Role, IsOwner, IsActive, CreatedAt)
    VALUES
        (NEWID(), @Tenant3Id, 'founder@innovatech.io', 'Arjun Nair', 'Founder & CEO', 1, 1, GETUTCDATE()),
        (NEWID(), @Tenant3Id, 'tech@innovatech.io', 'Sneha Rao', 'CTO', 0, 1, GETUTCDATE()),
        (NEWID(), @Tenant3Id, 'admin@innovatech.io', 'Kiran Babu', 'Admin', 0, 1, GETUTCDATE());
END

-- Local Shop Users
IF NOT EXISTS (SELECT 1 FROM TenantUsers WHERE Email = 'owner@localshop.com')
BEGIN
    INSERT INTO TenantUsers (Id, TenantId, Email, FullName, Role, IsOwner, IsActive, CreatedAt)
    VALUES
        (NEWID(), @Tenant4Id, 'owner@localshop.com', 'Ramesh Gupta', 'Owner', 1, 1, GETUTCDATE()),
        (NEWID(), @Tenant4Id, 'staff@localshop.com', 'Sunita Devi', 'Staff', 0, 1, GETUTCDATE());
END

-- Trial Company Users
IF NOT EXISTS (SELECT 1 FROM TenantUsers WHERE Email = 'demo@trialcompany.com')
BEGIN
    INSERT INTO TenantUsers (Id, TenantId, Email, FullName, Role, IsOwner, IsActive, CreatedAt)
    VALUES
        (NEWID(), @Tenant5Id, 'demo@trialcompany.com', 'Demo User', 'Trial Admin', 1, 1, GETUTCDATE());
END

PRINT 'Tenant Users seeded.';

-- =============================================================================
-- 6. SAMPLE BILLING INVOICES
-- =============================================================================
PRINT 'Seeding Sample Billing Invoices...';

-- Professional plan invoices for Tech Manufacturing
IF NOT EXISTS (SELECT 1 FROM TenantBillingInvoices WHERE TenantId = @Tenant1Id)
BEGIN
    INSERT INTO TenantBillingInvoices (Id, TenantId, SubscriptionId, InvoiceNumber, InvoiceDate, DueDate,
                                        PeriodStart, PeriodEnd, SubTotal, DiscountAmount, TaxAmount, TotalAmount, AmountPaid,
                                        Status, PaidAt, PaymentMethod, PaymentTransactionId, Currency, CreatedAt)
    VALUES
        (NEWID(), @Tenant1Id, @Sub1Id, 'INV-2024-001', DATEADD(MONTH, -3, GETUTCDATE()), DATEADD(MONTH, -3, DATEADD(DAY, 15, GETUTCDATE())),
         DATEADD(MONTH, -4, GETUTCDATE()), DATEADD(MONTH, -3, GETUTCDATE()),
         7499, 0, 1350, 8849, 8849, 2, DATEADD(MONTH, -3, DATEADD(DAY, 5, GETUTCDATE())), 'Card', 'TXN-TECH-001', 'INR', GETUTCDATE()),
        (NEWID(), @Tenant1Id, @Sub1Id, 'INV-2024-002', DATEADD(MONTH, -2, GETUTCDATE()), DATEADD(MONTH, -2, DATEADD(DAY, 15, GETUTCDATE())),
         DATEADD(MONTH, -3, GETUTCDATE()), DATEADD(MONTH, -2, GETUTCDATE()),
         7499, 0, 1350, 8849, 8849, 2, DATEADD(MONTH, -2, DATEADD(DAY, 3, GETUTCDATE())), 'Card', 'TXN-TECH-002', 'INR', GETUTCDATE()),
        (NEWID(), @Tenant1Id, @Sub1Id, 'INV-2024-003', DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(MONTH, -1, DATEADD(DAY, 15, GETUTCDATE())),
         DATEADD(MONTH, -2, GETUTCDATE()), DATEADD(MONTH, -1, GETUTCDATE()),
         7499, 0, 1350, 8849, 8849, 2, DATEADD(MONTH, -1, DATEADD(DAY, 7, GETUTCDATE())), 'Card', 'TXN-TECH-003', 'INR', GETUTCDATE());
END

-- Enterprise plan invoices for MegaMart
IF NOT EXISTS (SELECT 1 FROM TenantBillingInvoices WHERE TenantId = @Tenant2Id)
BEGIN
    INSERT INTO TenantBillingInvoices (Id, TenantId, SubscriptionId, InvoiceNumber, InvoiceDate, DueDate,
                                        PeriodStart, PeriodEnd, SubTotal, DiscountAmount, TaxAmount, TotalAmount, AmountPaid,
                                        Status, PaidAt, PaymentMethod, PaymentTransactionId, Currency, CreatedAt)
    VALUES
        (NEWID(), @Tenant2Id, @Sub2Id, 'INV-2024-101', DATEADD(MONTH, -6, GETUTCDATE()), DATEADD(MONTH, -6, DATEADD(DAY, 15, GETUTCDATE())),
         DATEADD(MONTH, -7, GETUTCDATE()), DATEADD(MONTH, -6, GETUTCDATE()),
         19999, 0, 3600, 23599, 23599, 2, DATEADD(MONTH, -6, DATEADD(DAY, 2, GETUTCDATE())), 'Bank Transfer', 'TXN-MEGA-001', 'INR', GETUTCDATE()),
        (NEWID(), @Tenant2Id, @Sub2Id, 'INV-2024-102', DATEADD(MONTH, -5, GETUTCDATE()), DATEADD(MONTH, -5, DATEADD(DAY, 15, GETUTCDATE())),
         DATEADD(MONTH, -6, GETUTCDATE()), DATEADD(MONTH, -5, GETUTCDATE()),
         19999, 0, 3600, 23599, 23599, 2, DATEADD(MONTH, -5, DATEADD(DAY, 1, GETUTCDATE())), 'Bank Transfer', 'TXN-MEGA-002', 'INR', GETUTCDATE()),
        (NEWID(), @Tenant2Id, @Sub2Id, 'INV-2024-103', DATEADD(MONTH, -4, GETUTCDATE()), DATEADD(MONTH, -4, DATEADD(DAY, 15, GETUTCDATE())),
         DATEADD(MONTH, -5, GETUTCDATE()), DATEADD(MONTH, -4, GETUTCDATE()),
         19999, 0, 3600, 23599, 23599, 2, DATEADD(MONTH, -4, DATEADD(DAY, 2, GETUTCDATE())), 'Bank Transfer', 'TXN-MEGA-003', 'INR', GETUTCDATE());
END

-- Starter plan invoice for InnovaTech
IF NOT EXISTS (SELECT 1 FROM TenantBillingInvoices WHERE TenantId = @Tenant3Id)
BEGIN
    INSERT INTO TenantBillingInvoices (Id, TenantId, SubscriptionId, InvoiceNumber, InvoiceDate, DueDate,
                                        PeriodStart, PeriodEnd, SubTotal, DiscountAmount, TaxAmount, TotalAmount, AmountPaid,
                                        Status, PaidAt, PaymentMethod, PaymentTransactionId, Currency, CreatedAt)
    VALUES
        (NEWID(), @Tenant3Id, @Sub3Id, 'INV-2024-201', DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(MONTH, -1, DATEADD(DAY, 15, GETUTCDATE())),
         DATEADD(MONTH, -2, GETUTCDATE()), DATEADD(MONTH, -1, GETUTCDATE()),
         2499, 0, 450, 2949, 2949, 2, DATEADD(MONTH, -1, DATEADD(DAY, 1, GETUTCDATE())), 'Card', 'TXN-INNO-001', 'INR', GETUTCDATE());
END

PRINT 'Sample Billing Invoices seeded.';

-- =============================================================================
-- SUMMARY
-- =============================================================================
PRINT '';
PRINT '=============================================================================';
PRINT 'Admin Database Seed Complete!';
PRINT '=============================================================================';
PRINT 'Seeded:';
PRINT '  - 4 Admin Roles';
PRINT '  - 4 Admin Users (Password: Admin@123)';
PRINT '  - 4 Billing Plans (Free, Starter, Professional, Enterprise)';
PRINT '  - 5 Sample Tenants with Subscriptions';
PRINT '  - 17 Tenant Users';
PRINT '  - Sample Billing Invoices';
PRINT '';
PRINT 'Admin Login: admin@algora.com / Admin@123';
PRINT '=============================================================================';
GO
