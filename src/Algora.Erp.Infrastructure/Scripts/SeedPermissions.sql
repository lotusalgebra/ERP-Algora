-- Seed permissions for all modules
-- Run this script on the ERP tenant database

-- Dashboard permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES (NEWID(), 'Dashboard.View', 'View Dashboard', 'Dashboard', 'Access to view the dashboard');

-- Finance module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Finance.View', 'View Finance', 'Finance', 'View invoices, accounts, payments, journal entries'),
(NEWID(), 'Finance.Create', 'Create Finance', 'Finance', 'Create invoices, accounts, payments, journal entries'),
(NEWID(), 'Finance.Edit', 'Edit Finance', 'Finance', 'Edit invoices, accounts, payments, journal entries'),
(NEWID(), 'Finance.Delete', 'Delete Finance', 'Finance', 'Delete invoices, accounts, payments, journal entries');

-- HR module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'HR.View', 'View HR', 'HR', 'View employees, departments, attendance, leave'),
(NEWID(), 'HR.Create', 'Create HR', 'HR', 'Create employees, departments, positions'),
(NEWID(), 'HR.Edit', 'Edit HR', 'HR', 'Edit employees, departments, attendance, leave'),
(NEWID(), 'HR.Delete', 'Delete HR', 'HR', 'Delete employees, departments, positions');

-- Payroll module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Payroll.View', 'View Payroll', 'Payroll', 'View salary components, payroll runs, payslips'),
(NEWID(), 'Payroll.Create', 'Create Payroll', 'Payroll', 'Create salary components, process payroll runs'),
(NEWID(), 'Payroll.Edit', 'Edit Payroll', 'Payroll', 'Edit salary components, payroll configurations'),
(NEWID(), 'Payroll.Delete', 'Delete Payroll', 'Payroll', 'Delete salary components, payroll runs');

-- Inventory module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Inventory.View', 'View Inventory', 'Inventory', 'View products, stock levels, warehouses'),
(NEWID(), 'Inventory.Create', 'Create Inventory', 'Inventory', 'Create products, warehouses, stock movements'),
(NEWID(), 'Inventory.Edit', 'Edit Inventory', 'Inventory', 'Edit products, stock levels, warehouses'),
(NEWID(), 'Inventory.Delete', 'Delete Inventory', 'Inventory', 'Delete products, warehouses');

-- Sales module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Sales.View', 'View Sales', 'Sales', 'View customers, orders, leads, quotations'),
(NEWID(), 'Sales.Create', 'Create Sales', 'Sales', 'Create customers, orders, leads, quotations'),
(NEWID(), 'Sales.Edit', 'Edit Sales', 'Sales', 'Edit customers, orders, leads, quotations'),
(NEWID(), 'Sales.Delete', 'Delete Sales', 'Sales', 'Delete customers, orders, leads, quotations');

-- Procurement module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Procurement.View', 'View Procurement', 'Procurement', 'View suppliers, purchase orders, goods receipts'),
(NEWID(), 'Procurement.Create', 'Create Procurement', 'Procurement', 'Create suppliers, purchase orders'),
(NEWID(), 'Procurement.Edit', 'Edit Procurement', 'Procurement', 'Edit suppliers, purchase orders, goods receipts'),
(NEWID(), 'Procurement.Delete', 'Delete Procurement', 'Procurement', 'Delete suppliers, purchase orders');

-- Manufacturing module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Manufacturing.View', 'View Manufacturing', 'Manufacturing', 'View BOMs, work orders'),
(NEWID(), 'Manufacturing.Create', 'Create Manufacturing', 'Manufacturing', 'Create BOMs, work orders'),
(NEWID(), 'Manufacturing.Edit', 'Edit Manufacturing', 'Manufacturing', 'Edit BOMs, work orders'),
(NEWID(), 'Manufacturing.Delete', 'Delete Manufacturing', 'Manufacturing', 'Delete BOMs, work orders');

-- Quality module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Quality.View', 'View Quality', 'Quality', 'View inspections, rejections'),
(NEWID(), 'Quality.Create', 'Create Quality', 'Quality', 'Create inspections, rejection notes'),
(NEWID(), 'Quality.Edit', 'Edit Quality', 'Quality', 'Edit inspections, rejection notes'),
(NEWID(), 'Quality.Delete', 'Delete Quality', 'Quality', 'Delete inspections, rejection notes');

-- Projects module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Projects.View', 'View Projects', 'Projects', 'View projects, tasks, time tracking'),
(NEWID(), 'Projects.Create', 'Create Projects', 'Projects', 'Create projects, tasks'),
(NEWID(), 'Projects.Edit', 'Edit Projects', 'Projects', 'Edit projects, tasks, time entries'),
(NEWID(), 'Projects.Delete', 'Delete Projects', 'Projects', 'Delete projects, tasks');

-- Dispatch module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Dispatch.View', 'View Dispatch', 'Dispatch', 'View delivery challans'),
(NEWID(), 'Dispatch.Create', 'Create Dispatch', 'Dispatch', 'Create delivery challans'),
(NEWID(), 'Dispatch.Edit', 'Edit Dispatch', 'Dispatch', 'Edit delivery challans'),
(NEWID(), 'Dispatch.Delete', 'Delete Dispatch', 'Dispatch', 'Delete delivery challans');

-- Ecommerce module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Ecommerce.View', 'View Ecommerce', 'Ecommerce', 'View ecommerce products, orders, customers, categories'),
(NEWID(), 'Ecommerce.Create', 'Create Ecommerce', 'Ecommerce', 'Create ecommerce products, categories, coupons'),
(NEWID(), 'Ecommerce.Edit', 'Edit Ecommerce', 'Ecommerce', 'Edit ecommerce products, orders, settings'),
(NEWID(), 'Ecommerce.Delete', 'Delete Ecommerce', 'Ecommerce', 'Delete ecommerce products, categories, coupons');

-- Admin module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Admin.View', 'View Admin', 'Admin', 'View users, roles, permissions'),
(NEWID(), 'Admin.Manage', 'Manage Admin', 'Admin', 'Manage users, roles, permissions, integrations');

-- Reports module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Reports.View', 'View Reports', 'Reports', 'View reports and cancellations'),
(NEWID(), 'Reports.Export', 'Export Reports', 'Reports', 'Export reports to various formats');

-- Settings module permissions
INSERT INTO Permissions (Id, Code, Name, Module, Description)
VALUES
(NEWID(), 'Settings.View', 'View Settings', 'Settings', 'View currencies, GST slabs, locations'),
(NEWID(), 'Settings.Edit', 'Edit Settings', 'Settings', 'Edit currencies, GST slabs, locations');

-- Create Admin role with all permissions (if not exists)
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
SELECT @AdminRoleId = Id FROM Roles WHERE Name = 'Admin';

IF @AdminRoleId IS NULL
BEGIN
    SET @AdminRoleId = NEWID();
    INSERT INTO Roles (Id, Name, Description)
    VALUES (@AdminRoleId, 'Admin', 'Full access to all modules');
END

-- Assign all permissions to Admin role
INSERT INTO RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), @AdminRoleId, p.Id
FROM Permissions p
WHERE NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = @AdminRoleId AND rp.PermissionId = p.Id
);

-- Create User role with basic permissions (if not exists)
DECLARE @UserRoleId UNIQUEIDENTIFIER;
SELECT @UserRoleId = Id FROM Roles WHERE Name = 'User';

IF @UserRoleId IS NULL
BEGIN
    SET @UserRoleId = NEWID();
    INSERT INTO Roles (Id, Name, Description)
    VALUES (@UserRoleId, 'User', 'Basic user with view-only access');
END

-- Assign view permissions to User role
INSERT INTO RolePermissions (Id, RoleId, PermissionId)
SELECT NEWID(), @UserRoleId, p.Id
FROM Permissions p
WHERE p.Code LIKE '%.View' AND NOT EXISTS (
    SELECT 1 FROM RolePermissions rp
    WHERE rp.RoleId = @UserRoleId AND rp.PermissionId = p.Id
);

PRINT 'Permissions seeded successfully';
