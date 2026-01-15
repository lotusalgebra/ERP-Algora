-- =============================================
-- Seed Admin Data for Production
-- Run this after database creation if auto-seeding
-- is disabled in production
-- =============================================

USE [AlgoraErpAdmin]
GO

-- Seed Roles (if not exist)
IF NOT EXISTS (SELECT 1 FROM AdminRoles WHERE Name = 'SuperAdmin')
BEGIN
    INSERT INTO AdminRoles (Id, Name, Description, Permissions, CreatedAt)
    VALUES
        (NEWID(), 'SuperAdmin', 'Full system access', '["*"]', GETUTCDATE()),
        (NEWID(), 'Admin', 'Administrative access', '["tenants.view", "tenants.create", "tenants.edit", "plans.view", "users.view"]', GETUTCDATE()),
        (NEWID(), 'Support', 'Support team access', '["tenants.view", "plans.view"]', GETUTCDATE());

    PRINT 'Seeded admin roles';
END
GO

-- Seed Admin User (if not exist)
-- NOTE: Change password immediately after deployment!
DECLARE @SuperAdminRoleId UNIQUEIDENTIFIER;
SELECT @SuperAdminRoleId = Id FROM AdminRoles WHERE Name = 'SuperAdmin';

IF NOT EXISTS (SELECT 1 FROM AdminUsers WHERE Email = 'admin@algora.com')
BEGIN
    INSERT INTO AdminUsers (Id, Email, PasswordHash, FirstName, LastName, RoleId, IsActive, EmailConfirmed, CreatedAt)
    VALUES (
        NEWID(),
        'admin@algora.com',
        -- Default password: Admin@123 (CHANGE IMMEDIATELY!)
        '$2a$11$rK7Cj8SXWXS9q1Z8vKxZz.XYZEXAMPLEHASHCHANGETHIS',
        'System',
        'Administrator',
        @SuperAdminRoleId,
        1,
        1,
        GETUTCDATE()
    );

    PRINT 'Seeded admin user: admin@algora.com';
    PRINT 'WARNING: Change the default password immediately!';
END
GO

-- Seed Plan Modules (if not exist)
IF NOT EXISTS (SELECT 1 FROM PlanModules)
BEGIN
    INSERT INTO PlanModules (Id, Code, Name, Description, BasePrice, Category, IsCore, DisplayOrder, CreatedAt)
    VALUES
        (NEWID(), 'inventory', 'Inventory Management', 'Product and stock management', 500, 'Core', 1, 1, GETUTCDATE()),
        (NEWID(), 'sales', 'Sales & Invoicing', 'Sales orders and invoicing', 500, 'Core', 1, 2, GETUTCDATE()),
        (NEWID(), 'customers', 'Customer Management', 'Customer database and CRM', 0, 'Core', 1, 3, GETUTCDATE()),
        (NEWID(), 'finance', 'Finance & Accounting', 'General ledger and financial reports', 1000, 'Finance', 0, 4, GETUTCDATE()),
        (NEWID(), 'banking', 'Banking Integration', 'Bank feeds and reconciliation', 500, 'Finance', 0, 5, GETUTCDATE()),
        (NEWID(), 'tax', 'Tax Management', 'Tax calculations and reporting', 750, 'Finance', 0, 6, GETUTCDATE()),
        (NEWID(), 'procurement', 'Procurement', 'Purchase orders and supplier management', 750, 'Operations', 0, 7, GETUTCDATE()),
        (NEWID(), 'manufacturing', 'Manufacturing', 'Production planning and BOMs', 1500, 'Operations', 0, 8, GETUTCDATE()),
        (NEWID(), 'warehouse', 'Warehouse Management', 'Multi-warehouse and locations', 500, 'Operations', 0, 9, GETUTCDATE()),
        (NEWID(), 'hr', 'Human Resources', 'Employee management', 1000, 'HR', 0, 10, GETUTCDATE()),
        (NEWID(), 'payroll', 'Payroll', 'Salary and payroll processing', 1500, 'HR', 0, 11, GETUTCDATE()),
        (NEWID(), 'crm', 'CRM', 'Advanced customer relationship management', 1000, 'Advanced', 0, 12, GETUTCDATE()),
        (NEWID(), 'projects', 'Project Management', 'Projects and task tracking', 750, 'Advanced', 0, 13, GETUTCDATE()),
        (NEWID(), 'ecommerce', 'E-Commerce', 'Online store integration', 1500, 'Advanced', 0, 14, GETUTCDATE()),
        (NEWID(), 'reports', 'Advanced Reports', 'Custom reports and analytics', 500, 'Advanced', 0, 15, GETUTCDATE()),
        (NEWID(), 'api', 'API Access', 'REST API integration', 1000, 'Integration', 0, 16, GETUTCDATE()),
        (NEWID(), 'webhooks', 'Webhooks', 'Event webhooks', 500, 'Integration', 0, 17, GETUTCDATE());

    PRINT 'Seeded plan modules';
END
GO

PRINT '';
PRINT '=============================================';
PRINT 'Admin data seeding complete!';
PRINT '';
PRINT 'IMPORTANT: Run 02-change-admin-password.sql';
PRINT 'to change the default admin password!';
PRINT '=============================================';
GO
