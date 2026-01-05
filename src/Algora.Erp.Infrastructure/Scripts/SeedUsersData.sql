-- Seed sample users data
SET QUOTED_IDENTIFIER ON;
GO

-- First, ensure we have some roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Administrator')
BEGIN
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, CreatedAt)
    VALUES (NEWID(), 'Administrator', 'Full system access', 1, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Manager')
BEGIN
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, CreatedAt)
    VALUES (NEWID(), 'Manager', 'Department management access', 0, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Accountant')
BEGIN
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, CreatedAt)
    VALUES (NEWID(), 'Accountant', 'Finance and accounting access', 0, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'HR Manager')
BEGIN
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, CreatedAt)
    VALUES (NEWID(), 'HR Manager', 'Human resources management', 0, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Sales Rep')
BEGIN
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, CreatedAt)
    VALUES (NEWID(), 'Sales Rep', 'Sales and CRM access', 0, GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Warehouse Staff')
BEGIN
    INSERT INTO Roles (Id, Name, Description, IsSystemRole, CreatedAt)
    VALUES (NEWID(), 'Warehouse Staff', 'Inventory management access', 0, GETUTCDATE());
END
GO

PRINT 'Roles created/verified';
GO

-- Insert sample users (password hash is for 'Password123!')
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Administrator');
DECLARE @ManagerRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Manager');
DECLARE @AccountantRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Accountant');
DECLARE @HRRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'HR Manager');
DECLARE @SalesRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Sales Rep');
DECLARE @WarehouseRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Warehouse Staff');

DECLARE @User1 UNIQUEIDENTIFIER = NEWID();
DECLARE @User2 UNIQUEIDENTIFIER = NEWID();
DECLARE @User3 UNIQUEIDENTIFIER = NEWID();
DECLARE @User4 UNIQUEIDENTIFIER = NEWID();
DECLARE @User5 UNIQUEIDENTIFIER = NEWID();
DECLARE @User6 UNIQUEIDENTIFIER = NEWID();
DECLARE @User7 UNIQUEIDENTIFIER = NEWID();
DECLARE @User8 UNIQUEIDENTIFIER = NEWID();

-- Insert users
INSERT INTO Users (Id, Email, FirstName, LastName, PhoneNumber, PasswordHash, Status, LastLoginAt, CreatedAt)
VALUES
(@User1, 'admin@algora.com', 'System', 'Administrator', '+1-555-0001', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 0, DATEADD(hour, -1, GETUTCDATE()), GETUTCDATE()),
(@User2, 'john.manager@algora.com', 'John', 'Peterson', '+1-555-0002', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 0, DATEADD(hour, -3, GETUTCDATE()), GETUTCDATE()),
(@User3, 'sarah.finance@algora.com', 'Sarah', 'Mitchell', '+1-555-0003', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 0, DATEADD(day, -1, GETUTCDATE()), GETUTCDATE()),
(@User4, 'mike.hr@algora.com', 'Michael', 'Thompson', '+1-555-0004', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 0, DATEADD(hour, -5, GETUTCDATE()), GETUTCDATE()),
(@User5, 'emily.sales@algora.com', 'Emily', 'Rodriguez', '+1-555-0005', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 0, DATEADD(minute, -30, GETUTCDATE()), GETUTCDATE()),
(@User6, 'david.warehouse@algora.com', 'David', 'Kim', '+1-555-0006', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 0, DATEADD(day, -2, GETUTCDATE()), GETUTCDATE()),
(@User7, 'lisa.inactive@algora.com', 'Lisa', 'Brown', '+1-555-0007', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 1, DATEADD(day, -30, GETUTCDATE()), GETUTCDATE()),
(@User8, 'james.locked@algora.com', 'James', 'Wilson', '+1-555-0008', 'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', 2, DATEADD(day, -7, GETUTCDATE()), GETUTCDATE());
GO

PRINT 'Users created';
GO

-- Assign roles to users
DECLARE @AdminRoleId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Administrator');
DECLARE @ManagerRoleId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Manager');
DECLARE @AccountantRoleId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Accountant');
DECLARE @HRRoleId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'HR Manager');
DECLARE @SalesRoleId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Sales Rep');
DECLARE @WarehouseRoleId2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Warehouse Staff');

INSERT INTO UserRoles (Id, UserId, RoleId)
SELECT NEWID(), u.Id, @AdminRoleId2 FROM Users u WHERE u.Email = 'admin@algora.com'
UNION ALL
SELECT NEWID(), u.Id, @ManagerRoleId2 FROM Users u WHERE u.Email = 'john.manager@algora.com'
UNION ALL
SELECT NEWID(), u.Id, @AccountantRoleId2 FROM Users u WHERE u.Email = 'sarah.finance@algora.com'
UNION ALL
SELECT NEWID(), u.Id, @HRRoleId2 FROM Users u WHERE u.Email = 'mike.hr@algora.com'
UNION ALL
SELECT NEWID(), u.Id, @SalesRoleId2 FROM Users u WHERE u.Email = 'emily.sales@algora.com'
UNION ALL
SELECT NEWID(), u.Id, @WarehouseRoleId2 FROM Users u WHERE u.Email = 'david.warehouse@algora.com'
UNION ALL
SELECT NEWID(), u.Id, @SalesRoleId2 FROM Users u WHERE u.Email = 'lisa.inactive@algora.com'
UNION ALL
SELECT NEWID(), u.Id, @ManagerRoleId2 FROM Users u WHERE u.Email = 'james.locked@algora.com';
GO

PRINT 'User roles assigned';
GO

SELECT COUNT(*) AS 'Total Users' FROM Users;
SELECT COUNT(*) AS 'Total Roles' FROM Roles;
GO
