-- Algora ERP Tenant Database Schema
-- This script creates the tables for a tenant-specific database
-- Run this script for each new tenant database

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- ADMINISTRATION TABLES
-- =============================================

-- Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Email NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        PhoneNumber NVARCHAR(50) NULL,
        Avatar NVARCHAR(500) NULL,
        [Status] INT NOT NULL DEFAULT 1, -- 0=Pending, 1=Active, 2=Inactive, 3=Locked
        EmailConfirmed BIT NOT NULL DEFAULT 0,
        TwoFactorEnabled BIT NOT NULL DEFAULT 0,
        TwoFactorSecret NVARCHAR(100) NULL,
        LastLoginAt DATETIME2 NULL,
        FailedLoginAttempts INT NOT NULL DEFAULT 0,
        LockoutEndAt DATETIME2 NULL,
        RefreshToken NVARCHAR(500) NULL,
        RefreshTokenExpiryAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Users_Email UNIQUE (Email)
    );

    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_Status ON Users([Status]);
    CREATE INDEX IX_Users_IsDeleted ON Users(IsDeleted);

    PRINT 'Created Users table';
END
GO

-- Roles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE Roles (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        IsSystemRole BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Roles_Name UNIQUE (Name)
    );

    CREATE INDEX IX_Roles_Name ON Roles(Name);

    PRINT 'Created Roles table';
END
GO

-- UserRoles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE UserRoles (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        RoleId UNIQUEIDENTIFIER NOT NULL,
        AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        AssignedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_UserRoles_UserId_RoleId UNIQUE (UserId, RoleId)
    );

    CREATE INDEX IX_UserRoles_UserId ON UserRoles(UserId);
    CREATE INDEX IX_UserRoles_RoleId ON UserRoles(RoleId);

    PRINT 'Created UserRoles table';
END
GO

-- Permissions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Permissions' AND xtype='U')
BEGIN
    CREATE TABLE Permissions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(100) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        [Module] NVARCHAR(100) NOT NULL,
        CONSTRAINT UQ_Permissions_Code UNIQUE (Code)
    );

    CREATE INDEX IX_Permissions_Code ON Permissions(Code);
    CREATE INDEX IX_Permissions_Module ON Permissions([Module]);

    PRINT 'Created Permissions table';
END
GO

-- RolePermissions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RolePermissions' AND xtype='U')
BEGIN
    CREATE TABLE RolePermissions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        RoleId UNIQUEIDENTIFIER NOT NULL,
        PermissionId UNIQUEIDENTIFIER NOT NULL,
        CONSTRAINT FK_RolePermissions_Roles FOREIGN KEY (RoleId) REFERENCES Roles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RolePermissions_Permissions FOREIGN KEY (PermissionId) REFERENCES Permissions(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_RolePermissions_RoleId_PermissionId UNIQUE (RoleId, PermissionId)
    );

    CREATE INDEX IX_RolePermissions_RoleId ON RolePermissions(RoleId);

    PRINT 'Created RolePermissions table';
END
GO

-- AuditLogs table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='AuditLogs' AND xtype='U')
BEGIN
    CREATE TABLE AuditLogs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        UserId UNIQUEIDENTIFIER NULL,
        UserEmail NVARCHAR(255) NULL,
        [Action] INT NOT NULL, -- 0=Create, 1=Read, 2=Update, 3=Delete, 4=Login, 5=Logout, 6=Export, 7=Import
        EntityType NVARCHAR(200) NOT NULL,
        EntityId NVARCHAR(50) NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        AffectedColumns NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );

    CREATE INDEX IX_AuditLogs_Timestamp ON AuditLogs([Timestamp]);
    CREATE INDEX IX_AuditLogs_EntityType_EntityId ON AuditLogs(EntityType, EntityId);
    CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);

    PRINT 'Created AuditLogs table';
END
GO

-- =============================================
-- HR TABLES
-- =============================================

-- Departments table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Departments' AND xtype='U')
BEGIN
    CREATE TABLE Departments (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        ParentDepartmentId UNIQUEIDENTIFIER NULL,
        ManagerId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Departments_Code UNIQUE (Code),
        CONSTRAINT FK_Departments_ParentDepartment FOREIGN KEY (ParentDepartmentId) REFERENCES Departments(Id)
    );

    CREATE INDEX IX_Departments_Code ON Departments(Code);
    CREATE INDEX IX_Departments_IsDeleted ON Departments(IsDeleted);

    PRINT 'Created Departments table';
END
GO

-- Positions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Positions' AND xtype='U')
BEGIN
    CREATE TABLE Positions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Title NVARCHAR(100) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        DepartmentId UNIQUEIDENTIFIER NULL,
        MinSalary DECIMAL(18, 2) NULL,
        MaxSalary DECIMAL(18, 2) NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Positions_Code UNIQUE (Code),
        CONSTRAINT FK_Positions_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_Positions_Code ON Positions(Code);
    CREATE INDEX IX_Positions_IsDeleted ON Positions(IsDeleted);

    PRINT 'Created Positions table';
END
GO

-- Employees table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Employees' AND xtype='U')
BEGIN
    CREATE TABLE Employees (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        EmployeeCode NVARCHAR(20) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(255) NOT NULL,
        Phone NVARCHAR(50) NULL,
        Mobile NVARCHAR(50) NULL,
        DateOfBirth DATE NOT NULL,
        Gender INT NOT NULL DEFAULT 0, -- 0=Male, 1=Female, 2=Other, 3=PreferNotToSay
        [Address] NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        [State] NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,
        NationalId NVARCHAR(50) NULL,
        PassportNumber NVARCHAR(50) NULL,
        TaxId NVARCHAR(50) NULL,
        BankAccountNumber NVARCHAR(50) NULL,
        BankName NVARCHAR(100) NULL,
        Avatar NVARCHAR(500) NULL,
        DepartmentId UNIQUEIDENTIFIER NULL,
        PositionId UNIQUEIDENTIFIER NULL,
        ManagerId UNIQUEIDENTIFIER NULL,
        HireDate DATE NOT NULL,
        TerminationDate DATE NULL,
        EmploymentType INT NOT NULL DEFAULT 0, -- 0=FullTime, 1=PartTime, 2=Contract, 3=Temporary, 4=Intern, 5=Freelance
        EmploymentStatus INT NOT NULL DEFAULT 0, -- 0=Active, 1=OnLeave, 2=Suspended, 3=Terminated, 4=Retired
        BaseSalary DECIMAL(18, 2) NULL,
        SalaryCurrency NVARCHAR(3) NULL DEFAULT 'USD',
        PayFrequency INT NOT NULL DEFAULT 3, -- 0=Weekly, 1=BiWeekly, 2=SemiMonthly, 3=Monthly, 4=Quarterly, 5=Annually
        EmergencyContactName NVARCHAR(100) NULL,
        EmergencyContactPhone NVARCHAR(50) NULL,
        EmergencyContactRelation NVARCHAR(50) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Employees_EmployeeCode UNIQUE (EmployeeCode),
        CONSTRAINT UQ_Employees_Email UNIQUE (Email),
        CONSTRAINT FK_Employees_Departments FOREIGN KEY (DepartmentId) REFERENCES Departments(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Employees_Positions FOREIGN KEY (PositionId) REFERENCES Positions(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Employees_Manager FOREIGN KEY (ManagerId) REFERENCES Employees(Id)
    );

    CREATE INDEX IX_Employees_EmployeeCode ON Employees(EmployeeCode);
    CREATE INDEX IX_Employees_Email ON Employees(Email);
    CREATE INDEX IX_Employees_EmploymentStatus ON Employees(EmploymentStatus);
    CREATE INDEX IX_Employees_DepartmentId ON Employees(DepartmentId);
    CREATE INDEX IX_Employees_IsDeleted ON Employees(IsDeleted);

    PRINT 'Created Employees table';
END
GO

-- Add ManagerId FK to Departments (after Employees table is created)
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Departments_Manager')
BEGIN
    ALTER TABLE Departments
    ADD CONSTRAINT FK_Departments_Manager FOREIGN KEY (ManagerId) REFERENCES Employees(Id);
    PRINT 'Added ManagerId FK to Departments';
END
GO

-- Attendances table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Attendances' AND xtype='U')
BEGIN
    CREATE TABLE Attendances (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        [Date] DATE NOT NULL,
        CheckInTime TIME NULL,
        CheckOutTime TIME NULL,
        BreakDuration TIME NULL,
        TotalWorkHours TIME NULL,
        OvertimeHours TIME NULL,
        [Status] INT NOT NULL DEFAULT 0, -- 0=Present, 1=Absent, 2=Late, 3=HalfDay, 4=OnLeave, 5=Holiday, 6=Weekend, 7=WorkFromHome
        Notes NVARCHAR(500) NULL,
        CheckInLocation NVARCHAR(200) NULL,
        CheckOutLocation NVARCHAR(200) NULL,
        CheckInLatitude DECIMAL(10, 7) NULL,
        CheckInLongitude DECIMAL(10, 7) NULL,
        CheckOutLatitude DECIMAL(10, 7) NULL,
        CheckOutLongitude DECIMAL(10, 7) NULL,
        CheckInIpAddress NVARCHAR(50) NULL,
        CheckOutIpAddress NVARCHAR(50) NULL,
        IsLate BIT NOT NULL DEFAULT 0,
        IsEarlyDeparture BIT NOT NULL DEFAULT 0,
        IsApproved BIT NOT NULL DEFAULT 0,
        ApprovedBy UNIQUEIDENTIFIER NULL,
        ApprovedAt DATETIME2 NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Attendances_EmployeeId_Date UNIQUE (EmployeeId, [Date]),
        CONSTRAINT FK_Attendances_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_Attendances_EmployeeId ON Attendances(EmployeeId);
    CREATE INDEX IX_Attendances_Date ON Attendances([Date]);
    CREATE INDEX IX_Attendances_Status ON Attendances([Status]);
    CREATE INDEX IX_Attendances_IsDeleted ON Attendances(IsDeleted);

    PRINT 'Created Attendances table';
END
GO

-- LeaveRequests table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LeaveRequests' AND xtype='U')
BEGIN
    CREATE TABLE LeaveRequests (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        LeaveType INT NOT NULL DEFAULT 0, -- 0=Annual, 1=Sick, 2=Personal, 3=Maternity, 4=Paternity, 5=Bereavement, 6=Unpaid, 7=Compensatory, 8=Marriage, 9=Study
        StartDate DATE NOT NULL,
        EndDate DATE NOT NULL,
        IsHalfDay BIT NOT NULL DEFAULT 0,
        HalfDayType INT NULL, -- 0=FirstHalf, 1=SecondHalf
        TotalDays DECIMAL(5, 1) NOT NULL,
        Reason NVARCHAR(1000) NOT NULL,
        AttachmentUrl NVARCHAR(500) NULL,
        [Status] INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Approved, 2=Rejected, 3=Cancelled
        RejectionReason NVARCHAR(500) NULL,
        ApprovedBy UNIQUEIDENTIFIER NULL,
        ApprovedAt DATETIME2 NULL,
        EmergencyContact NVARCHAR(100) NULL,
        HandoverNotes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_LeaveRequests_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE,
        CONSTRAINT FK_LeaveRequests_ApprovedBy FOREIGN KEY (ApprovedBy) REFERENCES Employees(Id)
    );

    CREATE INDEX IX_LeaveRequests_EmployeeId ON LeaveRequests(EmployeeId);
    CREATE INDEX IX_LeaveRequests_StartDate_EndDate ON LeaveRequests(StartDate, EndDate);
    CREATE INDEX IX_LeaveRequests_Status ON LeaveRequests([Status]);
    CREATE INDEX IX_LeaveRequests_IsDeleted ON LeaveRequests(IsDeleted);

    PRINT 'Created LeaveRequests table';
END
GO

-- LeaveBalances table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LeaveBalances' AND xtype='U')
BEGIN
    CREATE TABLE LeaveBalances (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        [Year] INT NOT NULL,
        LeaveType INT NOT NULL DEFAULT 0,
        TotalEntitlement DECIMAL(5, 1) NOT NULL DEFAULT 0,
        Used DECIMAL(5, 1) NOT NULL DEFAULT 0,
        Pending DECIMAL(5, 1) NOT NULL DEFAULT 0,
        CarriedForward DECIMAL(5, 1) NOT NULL DEFAULT 0,
        Adjustments DECIMAL(5, 1) NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_LeaveBalances_Employee_Year_Type UNIQUE (EmployeeId, [Year], LeaveType),
        CONSTRAINT FK_LeaveBalances_Employees FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_LeaveBalances_EmployeeId ON LeaveBalances(EmployeeId);
    CREATE INDEX IX_LeaveBalances_Year ON LeaveBalances([Year]);
    CREATE INDEX IX_LeaveBalances_IsDeleted ON LeaveBalances(IsDeleted);

    PRINT 'Created LeaveBalances table';
END
GO

-- =============================================
-- SEED DEFAULT DATA
-- =============================================

-- Seed default roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Administrator')
BEGIN
    INSERT INTO Roles (Name, [Description], IsSystemRole)
    VALUES
        ('Administrator', 'Full system access', 1),
        ('Manager', 'Department management access', 1),
        ('Employee', 'Standard employee access', 1),
        ('Accountant', 'Finance and accounting access', 1),
        ('HR Manager', 'Human resources management access', 1),
        ('Viewer', 'Read-only access', 1);

    PRINT 'Seeded default roles';
END
GO

-- Seed default permissions
IF NOT EXISTS (SELECT 1 FROM Permissions WHERE Code = 'users.view')
BEGIN
    INSERT INTO Permissions (Code, Name, [Module])
    VALUES
        -- Administration
        ('users.view', 'View Users', 'Administration'),
        ('users.create', 'Create Users', 'Administration'),
        ('users.edit', 'Edit Users', 'Administration'),
        ('users.delete', 'Delete Users', 'Administration'),
        ('roles.manage', 'Manage Roles', 'Administration'),
        ('settings.manage', 'Manage Settings', 'Administration'),

        -- Finance
        ('finance.accounts.view', 'View Chart of Accounts', 'Finance'),
        ('finance.accounts.manage', 'Manage Chart of Accounts', 'Finance'),
        ('finance.journal.view', 'View Journal Entries', 'Finance'),
        ('finance.journal.create', 'Create Journal Entries', 'Finance'),
        ('finance.invoices.view', 'View Invoices', 'Finance'),
        ('finance.invoices.manage', 'Manage Invoices', 'Finance'),
        ('finance.reports', 'View Financial Reports', 'Finance'),

        -- HR
        ('hr.employees.view', 'View Employees', 'HR'),
        ('hr.employees.manage', 'Manage Employees', 'HR'),
        ('hr.departments.manage', 'Manage Departments', 'HR'),
        ('hr.attendance.view', 'View Attendance', 'HR'),
        ('hr.attendance.manage', 'Manage Attendance', 'HR'),
        ('hr.leave.view', 'View Leave Requests', 'HR'),
        ('hr.leave.approve', 'Approve Leave Requests', 'HR'),

        -- Payroll
        ('payroll.view', 'View Payroll', 'Payroll'),
        ('payroll.process', 'Process Payroll', 'Payroll'),
        ('payroll.reports', 'View Payroll Reports', 'Payroll'),

        -- Inventory
        ('inventory.products.view', 'View Products', 'Inventory'),
        ('inventory.products.manage', 'Manage Products', 'Inventory'),
        ('inventory.stock.view', 'View Stock Levels', 'Inventory'),
        ('inventory.stock.adjust', 'Adjust Stock', 'Inventory'),
        ('inventory.warehouses.manage', 'Manage Warehouses', 'Inventory'),

        -- Procurement
        ('procurement.suppliers.view', 'View Suppliers', 'Procurement'),
        ('procurement.suppliers.manage', 'Manage Suppliers', 'Procurement'),
        ('procurement.po.view', 'View Purchase Orders', 'Procurement'),
        ('procurement.po.create', 'Create Purchase Orders', 'Procurement'),
        ('procurement.po.approve', 'Approve Purchase Orders', 'Procurement'),

        -- Sales
        ('sales.customers.view', 'View Customers', 'Sales'),
        ('sales.customers.manage', 'Manage Customers', 'Sales'),
        ('sales.orders.view', 'View Sales Orders', 'Sales'),
        ('sales.orders.create', 'Create Sales Orders', 'Sales'),
        ('sales.leads.manage', 'Manage Leads', 'Sales'),

        -- Manufacturing
        ('manufacturing.bom.view', 'View Bill of Materials', 'Manufacturing'),
        ('manufacturing.bom.manage', 'Manage Bill of Materials', 'Manufacturing'),
        ('manufacturing.workorders.view', 'View Work Orders', 'Manufacturing'),
        ('manufacturing.workorders.manage', 'Manage Work Orders', 'Manufacturing'),

        -- Projects
        ('projects.view', 'View Projects', 'Projects'),
        ('projects.manage', 'Manage Projects', 'Projects'),
        ('projects.time.view', 'View Time Entries', 'Projects'),
        ('projects.time.manage', 'Manage Time Entries', 'Projects');

    PRINT 'Seeded default permissions';
END
GO

-- Assign all permissions to Administrator role
DECLARE @AdminRoleId UNIQUEIDENTIFIER;
SELECT @AdminRoleId = Id FROM Roles WHERE Name = 'Administrator';

IF @AdminRoleId IS NOT NULL
BEGIN
    INSERT INTO RolePermissions (RoleId, PermissionId)
    SELECT @AdminRoleId, Id FROM Permissions
    WHERE Id NOT IN (SELECT PermissionId FROM RolePermissions WHERE RoleId = @AdminRoleId);

    PRINT 'Assigned all permissions to Administrator role';
END
GO

PRINT 'Tenant database schema created successfully';
GO
