-- =============================================================================
-- Algora ERP - Tenant Database Production Seed Data
-- =============================================================================
-- This script seeds production-ready data for a tenant ERP database
-- Run this script in the tenant database (e.g., AlgoraErp)
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

PRINT '=============================================================================';
PRINT 'Starting ERP Database Seed Data...';
PRINT '=============================================================================';

-- =============================================================================
-- 1. ROLES
-- =============================================================================
PRINT 'Seeding Roles...';

DECLARE @AdminRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ManagerRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @AccountantRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @HRRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @PurchaseRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @WarehouseRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ProductionRoleId UNIQUEIDENTIFIER = NEWID();
DECLARE @ViewerRoleId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Administrator')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@AdminRoleId, 'Administrator', 'Full system access with all permissions', 1, 1, GETUTCDATE());
ELSE
    SELECT @AdminRoleId = Id FROM Roles WHERE Name = 'Administrator';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Manager')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@ManagerRoleId, 'Manager', 'Department manager with approval rights', 0, 1, GETUTCDATE());
ELSE
    SELECT @ManagerRoleId = Id FROM Roles WHERE Name = 'Manager';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Accountant')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@AccountantRoleId, 'Accountant', 'Finance and accounting module access', 0, 1, GETUTCDATE());
ELSE
    SELECT @AccountantRoleId = Id FROM Roles WHERE Name = 'Accountant';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'HR Manager')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@HRRoleId, 'HR Manager', 'HR and payroll management', 0, 1, GETUTCDATE());
ELSE
    SELECT @HRRoleId = Id FROM Roles WHERE Name = 'HR Manager';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Sales Representative')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@SalesRoleId, 'Sales Representative', 'Sales and CRM module access', 0, 1, GETUTCDATE());
ELSE
    SELECT @SalesRoleId = Id FROM Roles WHERE Name = 'Sales Representative';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Purchase Officer')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@PurchaseRoleId, 'Purchase Officer', 'Procurement module access', 0, 1, GETUTCDATE());
ELSE
    SELECT @PurchaseRoleId = Id FROM Roles WHERE Name = 'Purchase Officer';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Warehouse Staff')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@WarehouseRoleId, 'Warehouse Staff', 'Inventory and stock management', 0, 1, GETUTCDATE());
ELSE
    SELECT @WarehouseRoleId = Id FROM Roles WHERE Name = 'Warehouse Staff';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Production Supervisor')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@ProductionRoleId, 'Production Supervisor', 'Manufacturing and work order management', 0, 1, GETUTCDATE());
ELSE
    SELECT @ProductionRoleId = Id FROM Roles WHERE Name = 'Production Supervisor';

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Viewer')
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, IsActive, CreatedAt)
    VALUES (@ViewerRoleId, 'Viewer', 'Read-only access to all modules', 0, 1, GETUTCDATE());
ELSE
    SELECT @ViewerRoleId = Id FROM Roles WHERE Name = 'Viewer';

PRINT 'Roles seeded.';

-- =============================================================================
-- 2. USERS
-- =============================================================================
PRINT 'Seeding Users...';

-- Password hash for 'Password123!' using SHA256
DECLARE @PasswordHash NVARCHAR(255) = 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=';

DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @ManagerUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @AccountantUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @HRUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @PurchaseUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @WarehouseUserId UNIQUEIDENTIFIER = NEWID();
DECLARE @ProductionUserId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@AdminUserId, 'admin@company.com', @PasswordHash, 'System Administrator', '+91-9876543210', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @AdminUserId, @AdminRoleId, GETUTCDATE());
END
ELSE
    SELECT @AdminUserId = Id FROM Users WHERE Email = 'admin@company.com';

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'manager@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@ManagerUserId, 'manager@company.com', @PasswordHash, 'Rajesh Kumar', '+91-9876543211', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @ManagerUserId, @ManagerRoleId, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'accountant@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@AccountantUserId, 'accountant@company.com', @PasswordHash, 'Priya Sharma', '+91-9876543212', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @AccountantUserId, @AccountantRoleId, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'hr@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@HRUserId, 'hr@company.com', @PasswordHash, 'Amit Singh', '+91-9876543213', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @HRUserId, @HRRoleId, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'sales@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@SalesUserId, 'sales@company.com', @PasswordHash, 'Deepak Verma', '+91-9876543214', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @SalesUserId, @SalesRoleId, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'purchase@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@PurchaseUserId, 'purchase@company.com', @PasswordHash, 'Sunita Reddy', '+91-9876543215', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @PurchaseUserId, @PurchaseRoleId, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'warehouse@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@WarehouseUserId, 'warehouse@company.com', @PasswordHash, 'Mohan Das', '+91-9876543216', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @WarehouseUserId, @WarehouseRoleId, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'production@company.com')
BEGIN
    INSERT INTO Users (Id, Email, PasswordHash, FullName, Phone, IsActive, EmailConfirmed, CreatedAt)
    VALUES (@ProductionUserId, 'production@company.com', @PasswordHash, 'Suresh Rao', '+91-9876543217', 1, 1, GETUTCDATE());

    INSERT INTO UserRoles (Id, UserId, RoleId, CreatedAt)
    VALUES (NEWID(), @ProductionUserId, @ProductionRoleId, GETUTCDATE());
END

PRINT 'Users seeded.';

-- =============================================================================
-- 3. DEPARTMENTS
-- =============================================================================
PRINT 'Seeding Departments...';

DECLARE @MgmtDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @FinanceDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @HRDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @PurchaseDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @WarehouseDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @ProductionDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @QualityDeptId UNIQUEIDENTIFIER = NEWID();
DECLARE @ITDeptId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'MGMT')
BEGIN
    INSERT INTO Departments (Id, Code, Name, Description, IsActive, CreatedAt)
    VALUES
        (@MgmtDeptId, 'MGMT', 'Management', 'Executive management team', 1, GETUTCDATE()),
        (@FinanceDeptId, 'FIN', 'Finance', 'Finance and accounting department', 1, GETUTCDATE()),
        (@HRDeptId, 'HR', 'Human Resources', 'HR and administration', 1, GETUTCDATE()),
        (@SalesDeptId, 'SALES', 'Sales', 'Sales and marketing department', 1, GETUTCDATE()),
        (@PurchaseDeptId, 'PURCH', 'Procurement', 'Purchasing and vendor management', 1, GETUTCDATE()),
        (@WarehouseDeptId, 'WH', 'Warehouse', 'Warehouse and logistics', 1, GETUTCDATE()),
        (@ProductionDeptId, 'PROD', 'Production', 'Manufacturing and production', 1, GETUTCDATE()),
        (@QualityDeptId, 'QC', 'Quality Control', 'Quality assurance and control', 1, GETUTCDATE()),
        (@ITDeptId, 'IT', 'Information Technology', 'IT and technical support', 1, GETUTCDATE());
END
ELSE
BEGIN
    SELECT @MgmtDeptId = Id FROM Departments WHERE Code = 'MGMT';
    SELECT @FinanceDeptId = Id FROM Departments WHERE Code = 'FIN';
    SELECT @HRDeptId = Id FROM Departments WHERE Code = 'HR';
    SELECT @SalesDeptId = Id FROM Departments WHERE Code = 'SALES';
    SELECT @PurchaseDeptId = Id FROM Departments WHERE Code = 'PURCH';
    SELECT @WarehouseDeptId = Id FROM Departments WHERE Code = 'WH';
    SELECT @ProductionDeptId = Id FROM Departments WHERE Code = 'PROD';
    SELECT @QualityDeptId = Id FROM Departments WHERE Code = 'QC';
    SELECT @ITDeptId = Id FROM Departments WHERE Code = 'IT';
END

PRINT 'Departments seeded.';

-- =============================================================================
-- 4. POSITIONS
-- =============================================================================
PRINT 'Seeding Positions...';

IF NOT EXISTS (SELECT 1 FROM Positions WHERE Code = 'CEO')
BEGIN
    INSERT INTO Positions (Id, Code, Name, DepartmentId, Level, MinSalary, MaxSalary, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'CEO', 'Chief Executive Officer', @MgmtDeptId, 1, 200000, 500000, 1, GETUTCDATE()),
        (NEWID(), 'CFO', 'Chief Financial Officer', @FinanceDeptId, 1, 150000, 400000, 1, GETUTCDATE()),
        (NEWID(), 'ACCMGR', 'Accounting Manager', @FinanceDeptId, 2, 60000, 100000, 1, GETUTCDATE()),
        (NEWID(), 'ACCNT', 'Accountant', @FinanceDeptId, 3, 30000, 60000, 1, GETUTCDATE()),
        (NEWID(), 'HRMGR', 'HR Manager', @HRDeptId, 2, 50000, 90000, 1, GETUTCDATE()),
        (NEWID(), 'HREXE', 'HR Executive', @HRDeptId, 3, 25000, 45000, 1, GETUTCDATE()),
        (NEWID(), 'SALESMGR', 'Sales Manager', @SalesDeptId, 2, 60000, 120000, 1, GETUTCDATE()),
        (NEWID(), 'SALESEXE', 'Sales Executive', @SalesDeptId, 3, 25000, 50000, 1, GETUTCDATE()),
        (NEWID(), 'PURMGR', 'Purchase Manager', @PurchaseDeptId, 2, 50000, 90000, 1, GETUTCDATE()),
        (NEWID(), 'PUREXE', 'Purchase Executive', @PurchaseDeptId, 3, 25000, 45000, 1, GETUTCDATE()),
        (NEWID(), 'WHMGR', 'Warehouse Manager', @WarehouseDeptId, 2, 40000, 70000, 1, GETUTCDATE()),
        (NEWID(), 'WHSTF', 'Warehouse Staff', @WarehouseDeptId, 4, 15000, 30000, 1, GETUTCDATE()),
        (NEWID(), 'PRODMGR', 'Production Manager', @ProductionDeptId, 2, 60000, 100000, 1, GETUTCDATE()),
        (NEWID(), 'PRODSUP', 'Production Supervisor', @ProductionDeptId, 3, 35000, 60000, 1, GETUTCDATE()),
        (NEWID(), 'PRODOP', 'Machine Operator', @ProductionDeptId, 4, 18000, 35000, 1, GETUTCDATE()),
        (NEWID(), 'QCMGR', 'QC Manager', @QualityDeptId, 2, 50000, 85000, 1, GETUTCDATE()),
        (NEWID(), 'QCINSP', 'QC Inspector', @QualityDeptId, 3, 25000, 45000, 1, GETUTCDATE()),
        (NEWID(), 'ITMGR', 'IT Manager', @ITDeptId, 2, 70000, 120000, 1, GETUTCDATE()),
        (NEWID(), 'SYSADM', 'System Administrator', @ITDeptId, 3, 40000, 70000, 1, GETUTCDATE());
END

PRINT 'Positions seeded.';

-- =============================================================================
-- 5. EMPLOYEES
-- =============================================================================
PRINT 'Seeding Employees...';

DECLARE @PositionId UNIQUEIDENTIFIER;

IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeCode = 'EMP001')
BEGIN
    SELECT @PositionId = Id FROM Positions WHERE Code = 'CEO';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP001', 'Rajesh', 'Kumar', 'rajesh.kumar@company.com', '+91-9876543001', '1975-05-15', 'Male', '2015-01-01',
            @MgmtDeptId, @PositionId, NULL, 0, 1, 350000, 'HDFC Bank', '50100012345678', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'CFO';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP002', 'Priya', 'Sharma', 'priya.sharma@company.com', '+91-9876543002', '1980-08-22', 'Female', '2016-03-15',
            @FinanceDeptId, @PositionId, NULL, 0, 1, 280000, 'ICICI Bank', '60200023456789', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'HRMGR';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP003', 'Amit', 'Singh', 'amit.singh@company.com', '+91-9876543003', '1985-02-10', 'Male', '2017-06-01',
            @HRDeptId, @PositionId, NULL, 0, 1, 75000, 'State Bank', '70300034567890', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'SALESMGR';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP004', 'Deepak', 'Verma', 'deepak.verma@company.com', '+91-9876543004', '1982-11-25', 'Male', '2018-01-10',
            @SalesDeptId, @PositionId, NULL, 0, 1, 95000, 'Axis Bank', '80400045678901', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'SALESEXE';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'EMP005', 'Kavita', 'Iyer', 'kavita.iyer@company.com', '+91-9876543005', '1990-07-18', 'Female', '2019-04-15',
            @SalesDeptId, @PositionId, NULL, 0, 1, 40000, 'HDFC Bank', '50100056789012', 1, GETUTCDATE()),
        (NEWID(), 'EMP006', 'Rahul', 'Mehta', 'rahul.mehta@company.com', '+91-9876543006', '1992-03-05', 'Male', '2020-02-01',
            @SalesDeptId, @PositionId, NULL, 0, 1, 38000, 'ICICI Bank', '60200067890123', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'PURMGR';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP007', 'Sunita', 'Reddy', 'sunita.reddy@company.com', '+91-9876543007', '1983-09-12', 'Female', '2017-08-20',
            @PurchaseDeptId, @PositionId, NULL, 0, 1, 72000, 'State Bank', '70300078901234', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'WHMGR';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP008', 'Mohan', 'Das', 'mohan.das@company.com', '+91-9876543008', '1978-12-30', 'Male', '2016-05-10',
            @WarehouseDeptId, @PositionId, NULL, 0, 1, 55000, 'Axis Bank', '80400089012345', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'WHSTF';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'EMP009', 'Ravi', 'Kumar', 'ravi.kumar@company.com', '+91-9876543009', '1995-04-20', 'Male', '2021-03-01',
            @WarehouseDeptId, @PositionId, NULL, 0, 1, 22000, 'HDFC Bank', '50100090123456', 1, GETUTCDATE()),
        (NEWID(), 'EMP010', 'Vijay', 'Prasad', 'vijay.prasad@company.com', '+91-9876543010', '1993-06-15', 'Male', '2020-09-15',
            @WarehouseDeptId, @PositionId, NULL, 0, 1, 24000, 'State Bank', '70300001234567', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'PRODMGR';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP011', 'Suresh', 'Rao', 'suresh.rao@company.com', '+91-9876543011', '1980-01-08', 'Male', '2017-01-05',
            @ProductionDeptId, @PositionId, NULL, 0, 1, 82000, 'ICICI Bank', '60200012345678', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'PRODSUP';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'EMP012', 'Ganesh', 'Patil', 'ganesh.patil@company.com', '+91-9876543012', '1988-10-25', 'Male', '2018-07-10',
            @ProductionDeptId, @PositionId, NULL, 0, 1, 48000, 'Axis Bank', '80400023456789', 1, GETUTCDATE()),
        (NEWID(), 'EMP013', 'Lakshmi', 'Naidu', 'lakshmi.naidu@company.com', '+91-9876543013', '1991-05-18', 'Female', '2019-11-01',
            @ProductionDeptId, @PositionId, NULL, 0, 1, 45000, 'HDFC Bank', '50100034567890', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'QCMGR';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP014', 'Anand', 'Krishnan', 'anand.krishnan@company.com', '+91-9876543014', '1984-07-22', 'Male', '2018-03-01',
            @QualityDeptId, @PositionId, NULL, 0, 1, 68000, 'State Bank', '70300045678901', 1, GETUTCDATE());

    SELECT @PositionId = Id FROM Positions WHERE Code = 'ACCNT';
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, DateOfBirth, Gender, JoinDate,
                          DepartmentId, PositionId, ManagerId, EmploymentType, Status, BaseSalary, BankName, BankAccountNumber, IsActive, CreatedAt)
    VALUES (NEWID(), 'EMP015', 'Neha', 'Gupta', 'neha.gupta@company.com', '+91-9876543015', '1994-12-10', 'Female', '2021-01-15',
            @FinanceDeptId, @PositionId, NULL, 0, 1, 42000, 'ICICI Bank', '60200056789012', 1, GETUTCDATE());
END

PRINT 'Employees seeded.';

-- =============================================================================
-- 6. WAREHOUSES
-- =============================================================================
PRINT 'Seeding Warehouses...';

DECLARE @MainWarehouseId UNIQUEIDENTIFIER = NEWID();
DECLARE @RawMatWarehouseId UNIQUEIDENTIFIER = NEWID();
DECLARE @FinishedGoodsWarehouseId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE Code = 'MAIN')
BEGIN
    INSERT INTO Warehouses (Id, Code, Name, Description, Address, City, State, Country, PostalCode,
                           ManagerName, Phone, Email, IsActive, IsDefault, CreatedAt)
    VALUES
        (@MainWarehouseId, 'MAIN', 'Main Warehouse', 'Central storage facility', '123 Industrial Area, Phase 1',
         'Chennai', 'Tamil Nadu', 'India', '600001', 'Mohan Das', '+91-9876543008', 'warehouse@company.com', 1, 1, GETUTCDATE()),
        (@RawMatWarehouseId, 'RAW', 'Raw Materials Store', 'Raw materials and components', '124 Industrial Area, Phase 1',
         'Chennai', 'Tamil Nadu', 'India', '600001', 'Ravi Kumar', '+91-9876543009', 'rawmaterials@company.com', 1, 0, GETUTCDATE()),
        (@FinishedGoodsWarehouseId, 'FG', 'Finished Goods', 'Finished products ready for dispatch', '125 Industrial Area, Phase 1',
         'Chennai', 'Tamil Nadu', 'India', '600001', 'Vijay Prasad', '+91-9876543010', 'finishedgoods@company.com', 1, 0, GETUTCDATE());
END
ELSE
BEGIN
    SELECT @MainWarehouseId = Id FROM Warehouses WHERE Code = 'MAIN';
    SELECT @RawMatWarehouseId = Id FROM Warehouses WHERE Code = 'RAW';
    SELECT @FinishedGoodsWarehouseId = Id FROM Warehouses WHERE Code = 'FG';
END

PRINT 'Warehouses seeded.';

-- =============================================================================
-- 7. PRODUCT CATEGORIES
-- =============================================================================
PRINT 'Seeding Product Categories...';

DECLARE @RawMatCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @FinishedCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @PackagingCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @SparesCategoryId UNIQUEIDENTIFIER = NEWID();
DECLARE @ConsumablesCategoryId UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Code = 'RAW')
BEGIN
    INSERT INTO ProductCategories (Id, Code, Name, Description, ParentCategoryId, IsActive, CreatedAt)
    VALUES
        (@RawMatCategoryId, 'RAW', 'Raw Materials', 'Raw materials for production', NULL, 1, GETUTCDATE()),
        (@FinishedCategoryId, 'FIN', 'Finished Goods', 'Finished products for sale', NULL, 1, GETUTCDATE()),
        (@PackagingCategoryId, 'PKG', 'Packaging', 'Packaging materials', NULL, 1, GETUTCDATE()),
        (@SparesCategoryId, 'SPR', 'Spare Parts', 'Machine spare parts', NULL, 1, GETUTCDATE()),
        (@ConsumablesCategoryId, 'CON', 'Consumables', 'Consumable items', NULL, 1, GETUTCDATE());

    -- Sub-categories for Raw Materials
    INSERT INTO ProductCategories (Id, Code, Name, Description, ParentCategoryId, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'RAW-MTL', 'Metals', 'Metal raw materials', @RawMatCategoryId, 1, GETUTCDATE()),
        (NEWID(), 'RAW-PLT', 'Plastics', 'Plastic raw materials', @RawMatCategoryId, 1, GETUTCDATE()),
        (NEWID(), 'RAW-ELC', 'Electronics', 'Electronic components', @RawMatCategoryId, 1, GETUTCDATE());

    -- Sub-categories for Finished Goods
    INSERT INTO ProductCategories (Id, Code, Name, Description, ParentCategoryId, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'FIN-EQP', 'Equipment', 'Finished equipment', @FinishedCategoryId, 1, GETUTCDATE()),
        (NEWID(), 'FIN-ASM', 'Assemblies', 'Assembled products', @FinishedCategoryId, 1, GETUTCDATE());
END
ELSE
BEGIN
    SELECT @RawMatCategoryId = Id FROM ProductCategories WHERE Code = 'RAW';
    SELECT @FinishedCategoryId = Id FROM ProductCategories WHERE Code = 'FIN';
    SELECT @PackagingCategoryId = Id FROM ProductCategories WHERE Code = 'PKG';
END

PRINT 'Product Categories seeded.';

-- =============================================================================
-- 8. PRODUCTS
-- =============================================================================
PRINT 'Seeding Products...';

DECLARE @MetalCategoryId UNIQUEIDENTIFIER;
DECLARE @PlasticCategoryId UNIQUEIDENTIFIER;
DECLARE @ElecCategoryId UNIQUEIDENTIFIER;
DECLARE @EquipCategoryId UNIQUEIDENTIFIER;

SELECT @MetalCategoryId = Id FROM ProductCategories WHERE Code = 'RAW-MTL';
SELECT @PlasticCategoryId = Id FROM ProductCategories WHERE Code = 'RAW-PLT';
SELECT @ElecCategoryId = Id FROM ProductCategories WHERE Code = 'RAW-ELC';
SELECT @EquipCategoryId = Id FROM ProductCategories WHERE Code = 'FIN-EQP';

-- Use parent category if subcategories don't exist
IF @MetalCategoryId IS NULL SELECT @MetalCategoryId = @RawMatCategoryId;
IF @PlasticCategoryId IS NULL SELECT @PlasticCategoryId = @RawMatCategoryId;
IF @ElecCategoryId IS NULL SELECT @ElecCategoryId = @RawMatCategoryId;
IF @EquipCategoryId IS NULL SELECT @EquipCategoryId = @FinishedCategoryId;

DECLARE @Product1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product5Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product6Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product7Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product8Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product9Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Product10Id UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Products WHERE Sku = 'RAW-STL-001')
BEGIN
    -- Raw Materials
    INSERT INTO Products (Id, Sku, Name, Description, Barcode, CategoryId, ProductType, UnitOfMeasure, Brand,
                         CostPrice, SellingPrice, TaxRate, ReorderLevel, MinimumStock, MaximumStock,
                         Weight, IsSellable, IsPurchasable, IsActive, CreatedAt)
    VALUES
        (@Product1Id, 'RAW-STL-001', 'Steel Sheet 2mm', '2mm thick steel sheet for fabrication', '8901234567001',
         @MetalCategoryId, 2, 'KG', 'TATA Steel', 85.00, 0, 18, 500, 200, 2000, 1.0, 0, 1, 1, GETUTCDATE()),
        (@Product2Id, 'RAW-STL-002', 'Steel Rod 10mm', '10mm diameter steel rod', '8901234567002',
         @MetalCategoryId, 2, 'KG', 'SAIL', 72.00, 0, 18, 300, 100, 1500, 1.0, 0, 1, 1, GETUTCDATE()),
        (@Product3Id, 'RAW-ALM-001', 'Aluminum Sheet 1.5mm', '1.5mm aluminum sheet', '8901234567003',
         @MetalCategoryId, 2, 'KG', 'Hindalco', 220.00, 0, 18, 200, 100, 1000, 1.0, 0, 1, 1, GETUTCDATE()),
        (@Product4Id, 'RAW-PLS-001', 'ABS Plastic Granules', 'ABS plastic for injection molding', '8901234567004',
         @PlasticCategoryId, 2, 'KG', 'LG Chem', 145.00, 0, 18, 250, 100, 1200, 1.0, 0, 1, 1, GETUTCDATE()),
        (@Product5Id, 'RAW-ELC-001', 'PCB Board', 'Printed circuit board 10x15cm', '8901234567005',
         @ElecCategoryId, 2, 'EA', 'Generic', 35.00, 0, 18, 500, 200, 3000, 0.05, 0, 1, 1, GETUTCDATE()),
        (@Product6Id, 'RAW-ELC-002', 'LED Display Module', '7-segment LED display', '8901234567006',
         @ElecCategoryId, 2, 'EA', 'Samsung', 125.00, 0, 18, 200, 100, 1000, 0.02, 0, 1, 1, GETUTCDATE());

    -- Finished Goods
    INSERT INTO Products (Id, Sku, Name, Description, Barcode, CategoryId, ProductType, UnitOfMeasure, Brand, Manufacturer,
                         CostPrice, SellingPrice, TaxRate, ReorderLevel, MinimumStock, MaximumStock,
                         Weight, IsSellable, IsPurchasable, IsActive, CreatedAt)
    VALUES
        (@Product7Id, 'FIN-CTL-001', 'Industrial Control Panel', 'PLC-based control panel for automation', '8901234567101',
         @EquipCategoryId, 0, 'EA', 'TechMfg', 'Tech Manufacturing Ltd', 15000.00, 25000.00, 18, 10, 5, 50, 25.0, 1, 0, 1, GETUTCDATE()),
        (@Product8Id, 'FIN-MOT-001', 'AC Motor 2HP', '2HP three-phase AC motor', '8901234567102',
         @EquipCategoryId, 0, 'EA', 'TechMfg', 'Tech Manufacturing Ltd', 8000.00, 12500.00, 18, 20, 10, 100, 15.0, 1, 0, 1, GETUTCDATE()),
        (@Product9Id, 'FIN-PMP-001', 'Centrifugal Pump 1HP', '1HP centrifugal water pump', '8901234567103',
         @EquipCategoryId, 0, 'EA', 'TechMfg', 'Tech Manufacturing Ltd', 4500.00, 7500.00, 18, 25, 10, 100, 8.0, 1, 0, 1, GETUTCDATE()),
        (@Product10Id, 'FIN-SEN-001', 'Temperature Sensor Kit', 'Industrial temperature sensing kit', '8901234567104',
         @EquipCategoryId, 0, 'EA', 'TechMfg', 'Tech Manufacturing Ltd', 1200.00, 2200.00, 18, 50, 25, 300, 0.5, 1, 0, 1, GETUTCDATE());

    -- Packaging & Consumables
    INSERT INTO Products (Id, Sku, Name, Description, CategoryId, ProductType, UnitOfMeasure,
                         CostPrice, SellingPrice, TaxRate, ReorderLevel, MinimumStock, MaximumStock,
                         IsSellable, IsPurchasable, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'PKG-BOX-001', 'Corrugated Box Large', 'Large shipping box 60x40x40cm', @PackagingCategoryId, 2, 'EA',
         45.00, 0, 18, 200, 100, 1000, 0, 1, 1, GETUTCDATE()),
        (NEWID(), 'PKG-BOX-002', 'Corrugated Box Medium', 'Medium shipping box 40x30x30cm', @PackagingCategoryId, 2, 'EA',
         30.00, 0, 18, 300, 150, 1500, 0, 1, 1, GETUTCDATE()),
        (NEWID(), 'CON-OIL-001', 'Machine Oil', 'Industrial machine lubricant', @ConsumablesCategoryId, 2, 'LTR',
         180.00, 0, 18, 50, 20, 200, 0, 1, 1, GETUTCDATE()),
        (NEWID(), 'SPR-BRG-001', 'Ball Bearing 6205', 'Deep groove ball bearing', @SparesCategoryId, 2, 'EA',
         120.00, 0, 18, 100, 50, 500, 0, 1, 1, GETUTCDATE());
END
ELSE
BEGIN
    SELECT @Product1Id = Id FROM Products WHERE Sku = 'RAW-STL-001';
    SELECT @Product7Id = Id FROM Products WHERE Sku = 'FIN-CTL-001';
    SELECT @Product8Id = Id FROM Products WHERE Sku = 'FIN-MOT-001';
END

PRINT 'Products seeded.';

-- =============================================================================
-- 9. STOCK LEVELS
-- =============================================================================
PRINT 'Seeding Stock Levels...';

IF NOT EXISTS (SELECT 1 FROM StockLevels WHERE ProductId = @Product1Id)
BEGIN
    INSERT INTO StockLevels (Id, ProductId, WarehouseId, QuantityOnHand, QuantityReserved, QuantityOnOrder, CreatedAt)
    SELECT NEWID(), Id, @RawMatWarehouseId,
           CASE ProductType WHEN 2 THEN CAST(ABS(CHECKSUM(NEWID())) % 500 + 200 AS DECIMAL(18,4)) ELSE 0 END,
           0, 0, GETUTCDATE()
    FROM Products WHERE ProductType = 2 AND NOT EXISTS (SELECT 1 FROM StockLevels WHERE ProductId = Products.Id AND WarehouseId = @RawMatWarehouseId);

    INSERT INTO StockLevels (Id, ProductId, WarehouseId, QuantityOnHand, QuantityReserved, QuantityOnOrder, CreatedAt)
    SELECT NEWID(), Id, @FinishedGoodsWarehouseId,
           CASE ProductType WHEN 0 THEN CAST(ABS(CHECKSUM(NEWID())) % 30 + 10 AS DECIMAL(18,4)) ELSE 0 END,
           0, 0, GETUTCDATE()
    FROM Products WHERE ProductType = 0 AND NOT EXISTS (SELECT 1 FROM StockLevels WHERE ProductId = Products.Id AND WarehouseId = @FinishedGoodsWarehouseId);
END

PRINT 'Stock Levels seeded.';

-- =============================================================================
-- 10. SUPPLIERS
-- =============================================================================
PRINT 'Seeding Suppliers...';

DECLARE @Supplier1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Supplier2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Supplier3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Supplier4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Supplier5Id UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Code = 'SUP001')
BEGIN
    INSERT INTO Suppliers (Id, Code, Name, ContactPerson, Email, Phone, Address, City, State, Country, PostalCode,
                          TaxId, PaymentTermsDays, Currency, LeadTimeDays, IsActive, CreatedAt)
    VALUES
        (@Supplier1Id, 'SUP001', 'TATA Steel Limited', 'Amit Gupta', 'sales@tatasteel.com', '+91-22-66657000',
         '34 M.G. Road, Fort', 'Mumbai', 'Maharashtra', 'India', '400001', 'GSTIN27AAACT2727Q1ZS', 30, 'INR', 7, 1, GETUTCDATE()),
        (@Supplier2Id, 'SUP002', 'Hindalco Industries', 'Priya Menon', 'procurement@hindalco.com', '+91-22-66910691',
         'Century Bhavan, Dr. Annie Besant Road', 'Mumbai', 'Maharashtra', 'India', '400030', 'GSTIN27AAACH1234H1Z5', 30, 'INR', 5, 1, GETUTCDATE()),
        (@Supplier3Id, 'SUP003', 'LG Polymers India', 'Rajesh Khanna', 'orders@lgpolymers.in', '+91-891-2566789',
         'RR Venkatapuram', 'Visakhapatnam', 'Andhra Pradesh', 'India', '530040', 'GSTIN37AABCL5678L1Z2', 45, 'INR', 10, 1, GETUTCDATE()),
        (@Supplier4Id, 'SUP004', 'Samsung Electronics India', 'Neha Sharma', 'b2b@samsung.com', '+91-124-4882000',
         'Samsung Plaza, Sector 44', 'Gurugram', 'Haryana', 'India', '122001', 'GSTIN06AABCS1234E1Z0', 30, 'INR', 14, 1, GETUTCDATE()),
        (@Supplier5Id, 'SUP005', 'Generic Electronics Ltd', 'Suresh Kumar', 'info@genericelectronics.com', '+91-44-28150000',
         '100 Anna Salai', 'Chennai', 'Tamil Nadu', 'India', '600002', 'GSTIN33AAECG9876G1Z8', 30, 'INR', 3, 1, GETUTCDATE());
END
ELSE
BEGIN
    SELECT @Supplier1Id = Id FROM Suppliers WHERE Code = 'SUP001';
    SELECT @Supplier2Id = Id FROM Suppliers WHERE Code = 'SUP002';
    SELECT @Supplier3Id = Id FROM Suppliers WHERE Code = 'SUP003';
END

PRINT 'Suppliers seeded.';

-- =============================================================================
-- 11. CUSTOMERS
-- =============================================================================
PRINT 'Seeding Customers...';

DECLARE @Customer1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer3Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer4Id UNIQUEIDENTIFIER = NEWID();
DECLARE @Customer5Id UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM Customers WHERE Code = 'CUST001')
BEGIN
    INSERT INTO Customers (Id, Code, Name, CustomerType, ContactPerson, Email, Phone, Mobile,
                          BillingAddress, BillingCity, BillingState, BillingCountry, BillingPostalCode,
                          ShippingAddress, ShippingCity, ShippingState, ShippingCountry, ShippingPostalCode,
                          TaxId, PaymentTermsDays, CreditLimit, Currency, IsActive, CreatedAt)
    VALUES
        (@Customer1Id, 'CUST001', 'ABC Industries Pvt Ltd', 1, 'Vikram Singh', 'purchase@abcindustries.com', '+91-80-25678900', '+91-9845012345',
         '45 MIDC Industrial Area', 'Bangalore', 'Karnataka', 'India', '560058',
         '45 MIDC Industrial Area', 'Bangalore', 'Karnataka', 'India', '560058',
         'GSTIN29AADCA1234A1Z5', 30, 500000, 'INR', 1, GETUTCDATE()),
        (@Customer2Id, 'CUST002', 'XYZ Manufacturing Co', 1, 'Ramesh Agarwal', 'orders@xyzmfg.com', '+91-44-26781234', '+91-9840156789',
         '78 Industrial Estate', 'Chennai', 'Tamil Nadu', 'India', '600032',
         '78 Industrial Estate', 'Chennai', 'Tamil Nadu', 'India', '600032',
         'GSTIN33AABCX5678X1Z2', 45, 750000, 'INR', 1, GETUTCDATE()),
        (@Customer3Id, 'CUST003', 'Metro Engineering Works', 1, 'Sunil Kapoor', 'procurement@metroeng.in', '+91-22-24567890', '+91-9821234567',
         '123 Andheri Industrial Area', 'Mumbai', 'Maharashtra', 'India', '400053',
         '123 Andheri Industrial Area', 'Mumbai', 'Maharashtra', 'India', '400053',
         'GSTIN27AAECM9012M1Z8', 30, 300000, 'INR', 1, GETUTCDATE()),
        (@Customer4Id, 'CUST004', 'Delta Power Systems', 1, 'Anjali Deshmukh', 'sales@deltapower.co.in', '+91-40-27654321', '+91-9848765432',
         '56 Nacharam Industrial Area', 'Hyderabad', 'Telangana', 'India', '500076',
         '56 Nacharam Industrial Area', 'Hyderabad', 'Telangana', 'India', '500076',
         'GSTIN36AABCD3456D1Z4', 30, 600000, 'INR', 1, GETUTCDATE()),
        (@Customer5Id, 'CUST005', 'National Electricals', 1, 'Prakash Joshi', 'info@nationalelectricals.in', '+91-141-2345678', '+91-9414012345',
         '89 Sitapura Industrial Area', 'Jaipur', 'Rajasthan', 'India', '302022',
         '89 Sitapura Industrial Area', 'Jaipur', 'Rajasthan', 'India', '302022',
         'GSTIN08AAECN7890N1Z6', 45, 400000, 'INR', 1, GETUTCDATE());
END
ELSE
BEGIN
    SELECT @Customer1Id = Id FROM Customers WHERE Code = 'CUST001';
    SELECT @Customer2Id = Id FROM Customers WHERE Code = 'CUST002';
END

PRINT 'Customers seeded.';

-- =============================================================================
-- 12. CHART OF ACCOUNTS
-- =============================================================================
PRINT 'Seeding Chart of Accounts...';

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Code = '1000')
BEGIN
    INSERT INTO Accounts (Id, Code, Name, AccountType, ParentAccountId, Description, IsActive, Balance, CreatedAt)
    VALUES
        -- Assets (1000-1999)
        (NEWID(), '1000', 'Assets', 0, NULL, 'All assets', 1, 0, GETUTCDATE()),
        (NEWID(), '1100', 'Current Assets', 0, NULL, 'Current assets', 1, 0, GETUTCDATE()),
        (NEWID(), '1101', 'Cash in Hand', 0, NULL, 'Cash on premises', 1, 150000, GETUTCDATE()),
        (NEWID(), '1102', 'HDFC Bank - Current', 0, NULL, 'HDFC current account', 1, 2500000, GETUTCDATE()),
        (NEWID(), '1103', 'ICICI Bank - Current', 0, NULL, 'ICICI current account', 1, 1800000, GETUTCDATE()),
        (NEWID(), '1110', 'Accounts Receivable', 0, NULL, 'Trade receivables', 1, 3200000, GETUTCDATE()),
        (NEWID(), '1120', 'Inventory - Raw Materials', 0, NULL, 'Raw material stock', 1, 1500000, GETUTCDATE()),
        (NEWID(), '1121', 'Inventory - Work in Progress', 0, NULL, 'WIP inventory', 1, 450000, GETUTCDATE()),
        (NEWID(), '1122', 'Inventory - Finished Goods', 0, NULL, 'Finished goods stock', 1, 2200000, GETUTCDATE()),
        (NEWID(), '1130', 'Prepaid Expenses', 0, NULL, 'Prepaid expenses', 1, 85000, GETUTCDATE()),
        (NEWID(), '1200', 'Fixed Assets', 0, NULL, 'Fixed assets', 1, 0, GETUTCDATE()),
        (NEWID(), '1201', 'Land and Building', 0, NULL, 'Property', 1, 15000000, GETUTCDATE()),
        (NEWID(), '1202', 'Plant and Machinery', 0, NULL, 'Production equipment', 1, 8500000, GETUTCDATE()),
        (NEWID(), '1203', 'Furniture and Fixtures', 0, NULL, 'Office furniture', 1, 320000, GETUTCDATE()),
        (NEWID(), '1204', 'Vehicles', 0, NULL, 'Company vehicles', 1, 1200000, GETUTCDATE()),
        (NEWID(), '1205', 'Computer Equipment', 0, NULL, 'IT equipment', 1, 450000, GETUTCDATE()),
        (NEWID(), '1290', 'Accumulated Depreciation', 0, NULL, 'Total depreciation', 1, -2100000, GETUTCDATE()),

        -- Liabilities (2000-2999)
        (NEWID(), '2000', 'Liabilities', 1, NULL, 'All liabilities', 1, 0, GETUTCDATE()),
        (NEWID(), '2100', 'Current Liabilities', 1, NULL, 'Current liabilities', 1, 0, GETUTCDATE()),
        (NEWID(), '2101', 'Accounts Payable', 1, NULL, 'Trade payables', 1, 2100000, GETUTCDATE()),
        (NEWID(), '2102', 'Salaries Payable', 1, NULL, 'Outstanding salaries', 1, 850000, GETUTCDATE()),
        (NEWID(), '2110', 'GST Payable', 1, NULL, 'GST liability', 1, 320000, GETUTCDATE()),
        (NEWID(), '2111', 'TDS Payable', 1, NULL, 'TDS liability', 1, 75000, GETUTCDATE()),
        (NEWID(), '2120', 'Accrued Expenses', 1, NULL, 'Accrued liabilities', 1, 125000, GETUTCDATE()),
        (NEWID(), '2200', 'Long-term Liabilities', 1, NULL, 'Long-term debt', 1, 0, GETUTCDATE()),
        (NEWID(), '2201', 'Bank Loan - HDFC', 1, NULL, 'Term loan', 1, 5000000, GETUTCDATE()),
        (NEWID(), '2202', 'Vehicle Loan', 1, NULL, 'Vehicle finance', 1, 650000, GETUTCDATE()),

        -- Equity (3000-3999)
        (NEWID(), '3000', 'Equity', 2, NULL, 'Owner equity', 1, 0, GETUTCDATE()),
        (NEWID(), '3100', 'Share Capital', 2, NULL, 'Paid-up capital', 1, 10000000, GETUTCDATE()),
        (NEWID(), '3200', 'Retained Earnings', 2, NULL, 'Accumulated profits', 1, 5800000, GETUTCDATE()),
        (NEWID(), '3300', 'Current Year Profit', 2, NULL, 'Current year P&L', 1, 2500000, GETUTCDATE()),

        -- Revenue (4000-4999)
        (NEWID(), '4000', 'Revenue', 3, NULL, 'All revenue', 1, 0, GETUTCDATE()),
        (NEWID(), '4100', 'Sales Revenue', 3, NULL, 'Product sales', 1, 25000000, GETUTCDATE()),
        (NEWID(), '4101', 'Sales - Control Panels', 3, NULL, 'Control panel sales', 1, 12000000, GETUTCDATE()),
        (NEWID(), '4102', 'Sales - Motors', 3, NULL, 'Motor sales', 1, 8000000, GETUTCDATE()),
        (NEWID(), '4103', 'Sales - Pumps', 3, NULL, 'Pump sales', 1, 5000000, GETUTCDATE()),
        (NEWID(), '4200', 'Service Revenue', 3, NULL, 'Service income', 1, 2500000, GETUTCDATE()),
        (NEWID(), '4300', 'Other Income', 3, NULL, 'Miscellaneous income', 1, 150000, GETUTCDATE()),

        -- Expenses (5000-5999)
        (NEWID(), '5000', 'Expenses', 4, NULL, 'All expenses', 1, 0, GETUTCDATE()),
        (NEWID(), '5100', 'Cost of Goods Sold', 4, NULL, 'Direct costs', 1, 0, GETUTCDATE()),
        (NEWID(), '5101', 'Raw Material Cost', 4, NULL, 'Material purchases', 1, 8500000, GETUTCDATE()),
        (NEWID(), '5102', 'Direct Labor', 4, NULL, 'Production wages', 1, 2800000, GETUTCDATE()),
        (NEWID(), '5103', 'Manufacturing Overhead', 4, NULL, 'Factory overhead', 1, 1200000, GETUTCDATE()),
        (NEWID(), '5200', 'Operating Expenses', 4, NULL, 'Operating costs', 1, 0, GETUTCDATE()),
        (NEWID(), '5201', 'Salaries and Wages', 4, NULL, 'Employee salaries', 1, 4500000, GETUTCDATE()),
        (NEWID(), '5202', 'Rent Expense', 4, NULL, 'Office rent', 1, 600000, GETUTCDATE()),
        (NEWID(), '5203', 'Utilities', 4, NULL, 'Electricity, water, etc.', 1, 450000, GETUTCDATE()),
        (NEWID(), '5204', 'Insurance', 4, NULL, 'Business insurance', 1, 180000, GETUTCDATE()),
        (NEWID(), '5205', 'Depreciation', 4, NULL, 'Asset depreciation', 1, 420000, GETUTCDATE()),
        (NEWID(), '5206', 'Repairs and Maintenance', 4, NULL, 'R&M expenses', 1, 280000, GETUTCDATE()),
        (NEWID(), '5207', 'Office Supplies', 4, NULL, 'Office consumables', 1, 85000, GETUTCDATE()),
        (NEWID(), '5208', 'Travel Expense', 4, NULL, 'Business travel', 1, 220000, GETUTCDATE()),
        (NEWID(), '5209', 'Professional Fees', 4, NULL, 'Legal, audit, etc.', 1, 150000, GETUTCDATE()),
        (NEWID(), '5210', 'Marketing Expense', 4, NULL, 'Advertising and promotion', 1, 350000, GETUTCDATE()),
        (NEWID(), '5300', 'Financial Expenses', 4, NULL, 'Finance costs', 1, 0, GETUTCDATE()),
        (NEWID(), '5301', 'Bank Charges', 4, NULL, 'Banking fees', 1, 45000, GETUTCDATE()),
        (NEWID(), '5302', 'Interest Expense', 4, NULL, 'Loan interest', 1, 380000, GETUTCDATE());
END

PRINT 'Chart of Accounts seeded.';

-- =============================================================================
-- 13. SALARY COMPONENTS
-- =============================================================================
PRINT 'Seeding Salary Components...';

-- Check if already seeded by CreatePayrollTables.sql
IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'BASIC')
BEGIN
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder, IsActive, CreatedAt)
    VALUES
        ('BASIC', 'Basic Salary', 'Base salary component', 0, 0, 0, 1, 1, 1, 1, GETUTCDATE()),
        ('HRA', 'House Rent Allowance', 'Housing allowance - 40% of basic', 0, 1, 40, 1, 1, 2, 1, GETUTCDATE()),
        ('DA', 'Dearness Allowance', 'Cost of living adjustment', 0, 1, 10, 1, 1, 3, 1, GETUTCDATE()),
        ('CONVEY', 'Conveyance Allowance', 'Transportation allowance', 0, 0, 1600, 0, 1, 4, 1, GETUTCDATE()),
        ('MEDICAL', 'Medical Allowance', 'Health care allowance', 0, 0, 1250, 0, 1, 5, 1, GETUTCDATE()),
        ('SPECIAL', 'Special Allowance', 'Special allowance', 0, 0, 0, 1, 1, 6, 1, GETUTCDATE()),
        ('BONUS', 'Performance Bonus', 'Annual bonus', 0, 0, 0, 1, 0, 7, 1, GETUTCDATE()),
        ('OT', 'Overtime Pay', 'Overtime compensation', 0, 0, 0, 1, 0, 8, 1, GETUTCDATE()),
        ('PF', 'Provident Fund', 'EPF contribution - 12% of basic', 1, 1, 12, 0, 1, 10, 1, GETUTCDATE()),
        ('ESI', 'Employee State Insurance', 'ESI contribution - 0.75%', 1, 2, 0.75, 0, 1, 11, 1, GETUTCDATE()),
        ('PROF_TAX', 'Professional Tax', 'State professional tax', 1, 0, 200, 0, 1, 12, 1, GETUTCDATE()),
        ('LOAN', 'Loan Deduction', 'Employee loan EMI', 1, 0, 0, 0, 0, 13, 1, GETUTCDATE()),
        ('ADVANCE', 'Salary Advance', 'Advance recovery', 1, 0, 0, 0, 0, 14, 1, GETUTCDATE()),
        ('TDS', 'Tax Deducted at Source', 'Income tax TDS', 3, 0, 0, 0, 1, 20, 1, GETUTCDATE());
END

PRINT 'Salary Components seeded.';

-- =============================================================================
-- 14. SALARY STRUCTURES
-- =============================================================================
PRINT 'Seeding Salary Structures...';

DECLARE @SalaryStructure1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @SalaryStructure2Id UNIQUEIDENTIFIER = NEWID();
DECLARE @SalaryStructure3Id UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM SalaryStructures WHERE Code = 'SS-EXEC')
BEGIN
    INSERT INTO SalaryStructures (Id, Code, Name, Description, BaseSalary, Currency, PayFrequency, IsActive, CreatedAt)
    VALUES
        (@SalaryStructure1Id, 'SS-EXEC', 'Executive Structure', 'For management and executives', 100000, 'INR', 3, 1, GETUTCDATE()),
        (@SalaryStructure2Id, 'SS-STAFF', 'Staff Structure', 'For office staff', 40000, 'INR', 3, 1, GETUTCDATE()),
        (@SalaryStructure3Id, 'SS-WORKER', 'Worker Structure', 'For factory workers', 20000, 'INR', 3, 1, GETUTCDATE());

    -- Add components to Executive structure
    INSERT INTO SalaryStructureLines (Id, SalaryStructureId, SalaryComponentId, CalculationType, Value, Amount, SortOrder, CreatedAt)
    SELECT NEWID(), @SalaryStructure1Id, Id,
           CASE Code WHEN 'BASIC' THEN 0 WHEN 'HRA' THEN 1 WHEN 'DA' THEN 1 WHEN 'PF' THEN 1 ELSE 0 END,
           CASE Code WHEN 'BASIC' THEN 50 WHEN 'HRA' THEN 40 WHEN 'DA' THEN 10 WHEN 'PF' THEN 12 ELSE 0 END,
           CASE Code WHEN 'CONVEY' THEN 1600 WHEN 'MEDICAL' THEN 1250 WHEN 'PROF_TAX' THEN 200 ELSE 0 END,
           SortOrder, GETUTCDATE()
    FROM SalaryComponents WHERE Code IN ('BASIC', 'HRA', 'DA', 'CONVEY', 'MEDICAL', 'PF', 'PROF_TAX');
END

PRINT 'Salary Structures seeded.';

-- =============================================================================
-- 15. SALES LEADS
-- =============================================================================
PRINT 'Seeding Sales Leads...';

IF NOT EXISTS (SELECT 1 FROM Leads WHERE Email = 'inquiry@newprospect.com')
BEGIN
    INSERT INTO Leads (Id, Name, Company, Email, Phone, Source, Status, AssignedTo, Notes, CreatedAt)
    VALUES
        (NEWID(), 'Vikram Shah', 'New Prospect Industries', 'inquiry@newprospect.com', '+91-9876500001', 1, 0, @SalesUserId,
         'Interested in control panels for new factory', GETUTCDATE()),
        (NEWID(), 'Priya Nair', 'Green Energy Solutions', 'priya@greenenergy.in', '+91-9876500002', 2, 1, @SalesUserId,
         'Looking for motor and pump solutions', GETUTCDATE()),
        (NEWID(), 'Arjun Menon', 'Tech Innovations Ltd', 'arjun@techinnovations.co', '+91-9876500003', 0, 2, @SalesUserId,
         'Requires custom automation solution', GETUTCDATE()),
        (NEWID(), 'Sneha Patel', 'Precision Engineering', 'sneha@precisioneng.com', '+91-9876500004', 3, 0, NULL,
         'Website inquiry for temperature sensors', GETUTCDATE()),
        (NEWID(), 'Rohit Saxena', 'Industrial Automation Co', 'rohit@indauto.in', '+91-9876500005', 1, 3, @SalesUserId,
         'Converted to customer - order placed', GETUTCDATE());
END

PRINT 'Sales Leads seeded.';

-- =============================================================================
-- 16. PURCHASE ORDERS
-- =============================================================================
PRINT 'Seeding Purchase Orders...';

DECLARE @PO1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @PO2Id UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM PurchaseOrders WHERE OrderNumber = 'PO-2024-001')
BEGIN
    INSERT INTO PurchaseOrders (Id, OrderNumber, OrderDate, DueDate, SupplierId, WarehouseId, Status,
                                SubTotal, TaxAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
    VALUES
        (@PO1Id, 'PO-2024-001', DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -23, GETUTCDATE()), @Supplier1Id, @RawMatWarehouseId,
         4, 170000, 30600, 200600, 'INR', 'Steel order for Q1', 'Quarterly steel procurement', GETUTCDATE()),
        (@PO2Id, 'PO-2024-002', DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, -1, GETUTCDATE()), @Supplier5Id, @RawMatWarehouseId,
         3, 35000, 6300, 41300, 'INR', 'Electronic components', 'PCB and LED modules', GETUTCDATE()),
        (NEWID(), 'PO-2024-003', DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, 9, GETUTCDATE()), @Supplier3Id, @RawMatWarehouseId,
         2, 72500, 13050, 85550, 'INR', 'Plastic granules order', 'ABS for molding', GETUTCDATE()),
        (NEWID(), 'PO-2024-004', GETUTCDATE(), DATEADD(DAY, 14, GETUTCDATE()), @Supplier1Id, @RawMatWarehouseId,
         1, 127500, 22950, 150450, 'INR', NULL, 'Pending approval', GETUTCDATE()),
        (NEWID(), 'PO-2024-005', GETUTCDATE(), DATEADD(DAY, 7, GETUTCDATE()), @Supplier2Id, @RawMatWarehouseId,
         0, 44000, 7920, 51920, 'INR', NULL, 'Draft order', GETUTCDATE());

    -- PO Lines for PO-2024-001
    INSERT INTO PurchaseOrderLines (Id, PurchaseOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure,
                                    UnitPrice, TaxPercent, TaxAmount, LineTotal, QuantityReceived, CreatedAt)
    VALUES
        (NEWID(), @PO1Id, @Product1Id, 'Steel Sheet 2mm', 'RAW-STL-001', 1, 1000, 'KG', 85, 18, 15300, 100300, 1000, GETUTCDATE()),
        (NEWID(), @PO1Id, @Product2Id, 'Steel Rod 10mm', 'RAW-STL-002', 2, 1000, 'KG', 85, 18, 15300, 100300, 1000, GETUTCDATE());
END

PRINT 'Purchase Orders seeded.';

-- =============================================================================
-- 17. SALES ORDERS
-- =============================================================================
PRINT 'Seeding Sales Orders...';

DECLARE @SO1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @SO2Id UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM SalesOrders WHERE OrderNumber = 'SO-2024-001')
BEGIN
    INSERT INTO SalesOrders (Id, OrderNumber, OrderDate, DueDate, CustomerId, Status,
                            SubTotal, TaxAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
    VALUES
        (@SO1Id, 'SO-2024-001', DATEADD(DAY, -25, GETUTCDATE()), DATEADD(DAY, -10, GETUTCDATE()), @Customer1Id,
         3, 250000, 45000, 295000, 'INR', 'ABC/PO/2024/123', 'Control panels order - shipped', GETUTCDATE()),
        (@SO2Id, 'SO-2024-002', DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, 0, GETUTCDATE()), @Customer2Id,
         2, 125000, 22500, 147500, 'INR', 'XYZ/PO/2024/456', 'Motors and pumps order', GETUTCDATE()),
        (NEWID(), 'SO-2024-003', DATEADD(DAY, -5, GETUTCDATE()), DATEADD(DAY, 10, GETUTCDATE()), @Customer3Id,
         1, 75000, 13500, 88500, 'INR', 'METRO/PO/2024/789', 'Confirmed order', GETUTCDATE()),
        (NEWID(), 'SO-2024-004', GETUTCDATE(), DATEADD(DAY, 15, GETUTCDATE()), @Customer4Id,
         0, 220000, 39600, 259600, 'INR', NULL, 'Draft quotation', GETUTCDATE());

    -- SO Lines for SO-2024-001
    INSERT INTO SalesOrderLines (Id, SalesOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure,
                                UnitPrice, TaxPercent, TaxAmount, LineTotal, QuantityShipped, CreatedAt)
    VALUES
        (NEWID(), @SO1Id, @Product7Id, 'Industrial Control Panel', 'FIN-CTL-001', 1, 10, 'EA', 25000, 18, 45000, 295000, 10, GETUTCDATE());

    -- SO Lines for SO-2024-002
    INSERT INTO SalesOrderLines (Id, SalesOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure,
                                UnitPrice, TaxPercent, TaxAmount, LineTotal, QuantityShipped, CreatedAt)
    VALUES
        (NEWID(), @SO2Id, @Product8Id, 'AC Motor 2HP', 'FIN-MOT-001', 1, 5, 'EA', 12500, 18, 11250, 73750, 0, GETUTCDATE()),
        (NEWID(), @SO2Id, @Product9Id, 'Centrifugal Pump 1HP', 'FIN-PMP-001', 2, 5, 'EA', 7500, 18, 6750, 44250, 0, GETUTCDATE());
END

PRINT 'Sales Orders seeded.';

-- =============================================================================
-- 18. BILL OF MATERIALS
-- =============================================================================
PRINT 'Seeding Bill of Materials...';

DECLARE @BOM1Id UNIQUEIDENTIFIER = NEWID();
DECLARE @BOM2Id UNIQUEIDENTIFIER = NEWID();

IF NOT EXISTS (SELECT 1 FROM BillOfMaterials WHERE BomNumber = 'BOM-CTL-001')
BEGIN
    INSERT INTO BillOfMaterials (Id, BomNumber, Name, Description, ProductId, Quantity, UnitOfMeasure, Status, IsActive, CreatedAt)
    VALUES
        (@BOM1Id, 'BOM-CTL-001', 'Control Panel BOM', 'Bill of materials for industrial control panel', @Product7Id, 1, 'EA', 1, 1, GETUTCDATE()),
        (@BOM2Id, 'BOM-MOT-001', 'AC Motor BOM', 'Bill of materials for 2HP AC motor', @Product8Id, 1, 'EA', 1, 1, GETUTCDATE());

    INSERT INTO BomLines (Id, BillOfMaterialId, ProductId, LineNumber, Quantity, UnitOfMeasure, UnitCost, TotalCost, WastagePercent, CreatedAt)
    VALUES
        (NEWID(), @BOM1Id, @Product1Id, 1, 5, 'KG', 85, 425, 2, GETUTCDATE()),
        (NEWID(), @BOM1Id, @Product5Id, 2, 3, 'EA', 35, 105, 0, GETUTCDATE()),
        (NEWID(), @BOM1Id, @Product6Id, 3, 2, 'EA', 125, 250, 0, GETUTCDATE());
END

PRINT 'Bill of Materials seeded.';

-- =============================================================================
-- 19. WORK ORDERS
-- =============================================================================
PRINT 'Seeding Work Orders...';

IF NOT EXISTS (SELECT 1 FROM WorkOrders WHERE WorkOrderNumber = 'WO-2024-001')
BEGIN
    INSERT INTO WorkOrders (Id, WorkOrderNumber, Name, BillOfMaterialId, ProductId, PlannedQuantity, CompletedQuantity, UnitOfMeasure,
                           Status, Priority, PlannedStartDate, PlannedEndDate, ActualStartDate, WarehouseId, EstimatedCost, ActualCost, CreatedAt)
    VALUES
        (NEWID(), 'WO-2024-001', 'Control Panel Production Batch 1', @BOM1Id, @Product7Id, 20, 20, 'EA',
         4, 2, DATEADD(DAY, -30, GETUTCDATE()), DATEADD(DAY, -20, GETUTCDATE()), DATEADD(DAY, -30, GETUTCDATE()),
         @FinishedGoodsWarehouseId, 300000, 285000, GETUTCDATE()),
        (NEWID(), 'WO-2024-002', 'AC Motor Production Run', @BOM2Id, @Product8Id, 15, 10, 'EA',
         2, 1, DATEADD(DAY, -15, GETUTCDATE()), DATEADD(DAY, 5, GETUTCDATE()), DATEADD(DAY, -15, GETUTCDATE()),
         @FinishedGoodsWarehouseId, 120000, 80000, GETUTCDATE()),
        (NEWID(), 'WO-2024-003', 'Pump Assembly Batch', NULL, @Product9Id, 25, 0, 'EA',
         1, 1, DATEADD(DAY, 5, GETUTCDATE()), DATEADD(DAY, 20, GETUTCDATE()), NULL,
         @FinishedGoodsWarehouseId, 112500, 0, GETUTCDATE()),
        (NEWID(), 'WO-2024-004', 'Sensor Kit Assembly', NULL, @Product10Id, 50, 0, 'EA',
         0, 0, DATEADD(DAY, 10, GETUTCDATE()), DATEADD(DAY, 15, GETUTCDATE()), NULL,
         @FinishedGoodsWarehouseId, 60000, 0, GETUTCDATE());
END

PRINT 'Work Orders seeded.';

-- =============================================================================
-- 20. PROJECTS
-- =============================================================================
PRINT 'Seeding Projects...';

IF NOT EXISTS (SELECT 1 FROM Projects WHERE Code = 'PRJ-2024-001')
BEGIN
    INSERT INTO Projects (Id, Code, Name, Description, CustomerId, Status, Priority, StartDate, EndDate, PlannedEndDate,
                         Budget, ActualCost, Progress, ProjectManagerId, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'PRJ-2024-001', 'ABC Factory Automation', 'Complete factory automation project for ABC Industries', @Customer1Id,
         2, 2, DATEADD(MONTH, -2, GETUTCDATE()), NULL, DATEADD(MONTH, 2, GETUTCDATE()),
         2500000, 1200000, 45, @ManagerUserId, 1, GETUTCDATE()),
        (NEWID(), 'PRJ-2024-002', 'XYZ Production Line Setup', 'New production line installation', @Customer2Id,
         1, 1, DATEADD(DAY, -15, GETUTCDATE()), NULL, DATEADD(MONTH, 3, GETUTCDATE()),
         1800000, 150000, 10, @ManagerUserId, 1, GETUTCDATE()),
        (NEWID(), 'PRJ-2024-003', 'Metro Engineering Upgrade', 'Control system upgrade project', @Customer3Id,
         0, 1, DATEADD(MONTH, 1, GETUTCDATE()), NULL, DATEADD(MONTH, 4, GETUTCDATE()),
         950000, 0, 0, @ManagerUserId, 1, GETUTCDATE());
END

PRINT 'Projects seeded.';

-- =============================================================================
-- 21. E-COMMERCE CATEGORIES & PRODUCTS
-- =============================================================================
PRINT 'Seeding E-Commerce Data...';

IF NOT EXISTS (SELECT 1 FROM WebCategories WHERE Slug = 'control-panels')
BEGIN
    DECLARE @WebCat1Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @WebCat2Id UNIQUEIDENTIFIER = NEWID();
    DECLARE @WebCat3Id UNIQUEIDENTIFIER = NEWID();

    INSERT INTO WebCategories (Id, Name, Slug, Description, ParentCategoryId, ImageUrl, SortOrder, IsActive, IsFeatured, CreatedAt)
    VALUES
        (@WebCat1Id, 'Control Panels', 'control-panels', 'Industrial control panels and automation systems', NULL, '/images/categories/control-panels.jpg', 1, 1, 1, GETUTCDATE()),
        (@WebCat2Id, 'Motors', 'motors', 'AC and DC motors for industrial use', NULL, '/images/categories/motors.jpg', 2, 1, 1, GETUTCDATE()),
        (@WebCat3Id, 'Pumps', 'pumps', 'Industrial pumps and water systems', NULL, '/images/categories/pumps.jpg', 3, 1, 0, GETUTCDATE()),
        (NEWID(), 'Sensors', 'sensors', 'Temperature, pressure, and level sensors', NULL, '/images/categories/sensors.jpg', 4, 1, 0, GETUTCDATE()),
        (NEWID(), 'Spare Parts', 'spare-parts', 'Replacement parts and components', NULL, '/images/categories/spares.jpg', 5, 1, 0, GETUTCDATE());

    -- E-Commerce Products
    INSERT INTO EcommerceProducts (Id, Sku, Name, Slug, Description, ShortDescription, WebCategoryId, Price, ComparePrice,
                                   CostPrice, StockQuantity, LowStockThreshold, TaxRate, Weight, IsActive, IsFeatured, CreatedAt)
    VALUES
        (NEWID(), 'FIN-CTL-001', 'Industrial Control Panel', 'industrial-control-panel',
         'Advanced PLC-based industrial control panel with touchscreen HMI interface. Perfect for factory automation and process control.',
         'PLC-based control panel with HMI', @WebCat1Id, 25000, 28000, 15000, 50, 10, 18, 25, 1, 1, GETUTCDATE()),
        (NEWID(), 'FIN-MOT-001', 'AC Motor 2HP', '2hp-ac-motor',
         'Heavy-duty 2HP three-phase AC induction motor. Suitable for industrial machinery and equipment.',
         '2HP three-phase AC motor', @WebCat2Id, 12500, 14000, 8000, 80, 15, 18, 15, 1, 1, GETUTCDATE()),
        (NEWID(), 'FIN-PMP-001', 'Centrifugal Pump 1HP', '1hp-centrifugal-pump',
         'High-efficiency 1HP centrifugal water pump for industrial and commercial applications.',
         '1HP water pump', @WebCat3Id, 7500, 8500, 4500, 120, 20, 18, 8, 1, 0, GETUTCDATE()),
        (NEWID(), 'FIN-SEN-001', 'Temperature Sensor Kit', 'temperature-sensor-kit',
         'Professional industrial temperature sensing kit with display and alarms. Range: -50 to 500 degrees C.',
         'Industrial temp sensor kit', NULL, 2200, 2500, 1200, 200, 30, 18, 0.5, 1, 0, GETUTCDATE());
END

PRINT 'E-Commerce Data seeded.';

-- =============================================================================
-- 22. COUPONS
-- =============================================================================
PRINT 'Seeding Coupons...';

IF NOT EXISTS (SELECT 1 FROM Coupons WHERE Code = 'WELCOME10')
BEGIN
    INSERT INTO Coupons (Id, Code, Name, Description, DiscountType, DiscountValue, MinimumOrderAmount, MaximumDiscountAmount,
                        UsageLimit, UsageCount, StartDate, EndDate, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'WELCOME10', 'Welcome Discount', '10% off on first order', 0, 10, 5000, 2500, 100, 15,
         DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(MONTH, 3, GETUTCDATE()), 1, GETUTCDATE()),
        (NEWID(), 'BULK20', 'Bulk Order Discount', 'Flat Rs. 2000 off on orders above 50000', 1, 2000, 50000, 2000, 50, 8,
         DATEADD(MONTH, -1, GETUTCDATE()), DATEADD(MONTH, 2, GETUTCDATE()), 1, GETUTCDATE()),
        (NEWID(), 'MOTOR15', 'Motor Special', '15% off on motor products', 0, 15, 10000, 5000, 30, 0,
         GETUTCDATE(), DATEADD(MONTH, 1, GETUTCDATE()), 1, GETUTCDATE());
END

PRINT 'Coupons seeded.';

-- =============================================================================
-- SUMMARY
-- =============================================================================
PRINT '';
PRINT '=============================================================================';
PRINT 'ERP Database Seed Complete!';
PRINT '=============================================================================';
PRINT 'Seeded:';
PRINT '  - 9 Roles';
PRINT '  - 8 Users (Password: Password123!)';
PRINT '  - 9 Departments';
PRINT '  - 19 Positions';
PRINT '  - 15 Employees';
PRINT '  - 3 Warehouses';
PRINT '  - 10 Product Categories';
PRINT '  - 14 Products';
PRINT '  - 5 Suppliers';
PRINT '  - 5 Customers';
PRINT '  - 55+ Chart of Accounts';
PRINT '  - 14 Salary Components';
PRINT '  - 3 Salary Structures';
PRINT '  - 5 Sales Leads';
PRINT '  - 5 Purchase Orders';
PRINT '  - 4 Sales Orders';
PRINT '  - 2 Bill of Materials';
PRINT '  - 4 Work Orders';
PRINT '  - 3 Projects';
PRINT '  - E-Commerce Categories & Products';
PRINT '  - 3 Coupons';
PRINT '';
PRINT 'User Login: admin@company.com / Password123!';
PRINT '=============================================================================';
GO
