-- Seed sample permissions data
SET QUOTED_IDENTIFIER ON;
GO

-- Administration Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'admin.users.view', 'View Users', 'View user list and details', 'Administration'),
(NEWID(), 'admin.users.create', 'Create Users', 'Create new users', 'Administration'),
(NEWID(), 'admin.users.edit', 'Edit Users', 'Edit user details', 'Administration'),
(NEWID(), 'admin.users.delete', 'Delete Users', 'Delete users', 'Administration'),
(NEWID(), 'admin.roles.view', 'View Roles', 'View role list and details', 'Administration'),
(NEWID(), 'admin.roles.manage', 'Manage Roles', 'Create, edit, delete roles', 'Administration'),
(NEWID(), 'admin.settings.view', 'View Settings', 'View system settings', 'Administration'),
(NEWID(), 'admin.settings.edit', 'Edit Settings', 'Modify system settings', 'Administration');
GO

-- Finance Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'finance.accounts.view', 'View Chart of Accounts', 'View account list', 'Finance'),
(NEWID(), 'finance.accounts.manage', 'Manage Accounts', 'Create, edit, delete accounts', 'Finance'),
(NEWID(), 'finance.journals.view', 'View Journal Entries', 'View journal entries', 'Finance'),
(NEWID(), 'finance.journals.create', 'Create Journal Entries', 'Create new journal entries', 'Finance'),
(NEWID(), 'finance.journals.post', 'Post Journal Entries', 'Post journal entries to ledger', 'Finance'),
(NEWID(), 'finance.invoices.view', 'View Invoices', 'View invoice list', 'Finance'),
(NEWID(), 'finance.invoices.manage', 'Manage Invoices', 'Create, edit invoices', 'Finance'),
(NEWID(), 'finance.payments.record', 'Record Payments', 'Record payments against invoices', 'Finance'),
(NEWID(), 'finance.reports.view', 'View Financial Reports', 'View financial reports', 'Finance');
GO

-- HR Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'hr.employees.view', 'View Employees', 'View employee list and details', 'HR'),
(NEWID(), 'hr.employees.manage', 'Manage Employees', 'Create, edit, delete employees', 'HR'),
(NEWID(), 'hr.departments.view', 'View Departments', 'View department list', 'HR'),
(NEWID(), 'hr.departments.manage', 'Manage Departments', 'Create, edit departments', 'HR'),
(NEWID(), 'hr.attendance.view', 'View Attendance', 'View attendance records', 'HR'),
(NEWID(), 'hr.attendance.manage', 'Manage Attendance', 'Edit attendance records', 'HR'),
(NEWID(), 'hr.leave.view', 'View Leave Requests', 'View leave requests', 'HR'),
(NEWID(), 'hr.leave.approve', 'Approve Leave', 'Approve/reject leave requests', 'HR');
GO

-- Inventory Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'inventory.products.view', 'View Products', 'View product list', 'Inventory'),
(NEWID(), 'inventory.products.manage', 'Manage Products', 'Create, edit products', 'Inventory'),
(NEWID(), 'inventory.stock.view', 'View Stock Levels', 'View stock levels', 'Inventory'),
(NEWID(), 'inventory.stock.adjust', 'Adjust Stock', 'Make stock adjustments', 'Inventory'),
(NEWID(), 'inventory.warehouses.view', 'View Warehouses', 'View warehouse list', 'Inventory'),
(NEWID(), 'inventory.warehouses.manage', 'Manage Warehouses', 'Create, edit warehouses', 'Inventory');
GO

-- Sales Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'sales.customers.view', 'View Customers', 'View customer list', 'Sales'),
(NEWID(), 'sales.customers.manage', 'Manage Customers', 'Create, edit customers', 'Sales'),
(NEWID(), 'sales.leads.view', 'View Leads', 'View lead list', 'Sales'),
(NEWID(), 'sales.leads.manage', 'Manage Leads', 'Create, edit, convert leads', 'Sales'),
(NEWID(), 'sales.orders.view', 'View Sales Orders', 'View sales order list', 'Sales'),
(NEWID(), 'sales.orders.create', 'Create Sales Orders', 'Create new sales orders', 'Sales'),
(NEWID(), 'sales.orders.process', 'Process Sales Orders', 'Confirm, ship, deliver orders', 'Sales'),
(NEWID(), 'sales.quotations.manage', 'Manage Quotations', 'Create, edit quotations', 'Sales');
GO

-- Procurement Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'procurement.suppliers.view', 'View Suppliers', 'View supplier list', 'Procurement'),
(NEWID(), 'procurement.suppliers.manage', 'Manage Suppliers', 'Create, edit suppliers', 'Procurement'),
(NEWID(), 'procurement.po.view', 'View Purchase Orders', 'View purchase order list', 'Procurement'),
(NEWID(), 'procurement.po.create', 'Create Purchase Orders', 'Create new purchase orders', 'Procurement'),
(NEWID(), 'procurement.po.approve', 'Approve Purchase Orders', 'Approve purchase orders', 'Procurement'),
(NEWID(), 'procurement.receipts.manage', 'Manage Goods Receipts', 'Record goods receipts', 'Procurement');
GO

-- Manufacturing Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'manufacturing.bom.view', 'View BOM', 'View bill of materials', 'Manufacturing'),
(NEWID(), 'manufacturing.bom.manage', 'Manage BOM', 'Create, edit BOM', 'Manufacturing'),
(NEWID(), 'manufacturing.wo.view', 'View Work Orders', 'View work orders', 'Manufacturing'),
(NEWID(), 'manufacturing.wo.manage', 'Manage Work Orders', 'Create, process work orders', 'Manufacturing');
GO

-- Projects Permissions
INSERT INTO Permissions (Id, Code, Name, Description, Module)
VALUES
(NEWID(), 'projects.view', 'View Projects', 'View project list', 'Projects'),
(NEWID(), 'projects.manage', 'Manage Projects', 'Create, edit projects', 'Projects'),
(NEWID(), 'projects.tasks.view', 'View Tasks', 'View project tasks', 'Projects'),
(NEWID(), 'projects.tasks.manage', 'Manage Tasks', 'Create, edit, assign tasks', 'Projects'),
(NEWID(), 'projects.time.log', 'Log Time', 'Log time entries', 'Projects'),
(NEWID(), 'projects.time.approve', 'Approve Time', 'Approve time entries', 'Projects');
GO

PRINT 'Permissions seeded successfully';
GO

SELECT Module, COUNT(*) AS PermissionCount FROM Permissions GROUP BY Module ORDER BY Module;
GO
