-- Seed remaining data that failed
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- Get product IDs
DECLARE @LaptopProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'LAPTOP-001');
DECLARE @MonitorProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'MONITOR-001');
DECLARE @KeyboardProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'KEYBOARD-001');
DECLARE @MouseProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'MOUSE-001');

-- Seed BOM
IF NOT EXISTS (SELECT 1 FROM BillOfMaterials WHERE BomNumber = 'BOM-WS-001')
BEGIN
    DECLARE @BomId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO BillOfMaterials (Id, BomNumber, Name, Description, ProductId, Quantity, UnitOfMeasure, Status, EffectiveFrom, IsActive, CreatedAt)
    VALUES (@BomId, 'BOM-WS-001', 'Workstation Kit', 'Complete workstation assembly kit', @LaptopProduct, 1, 'SET', 1, '2024-01-01', 1, GETUTCDATE());
    PRINT 'Seeded BillOfMaterials';
END
GO

-- Seed BOM Lines
DECLARE @BomRef UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM BillOfMaterials WHERE BomNumber = 'BOM-WS-001');
DECLARE @Monitor UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'MONITOR-001');
DECLARE @Keyboard UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'KEYBOARD-001');
DECLARE @Mouse UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'MOUSE-001');
DECLARE @Laptop UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'LAPTOP-001');

IF @BomRef IS NOT NULL AND NOT EXISTS (SELECT 1 FROM BomLines WHERE BillOfMaterialId = @BomRef)
BEGIN
    INSERT INTO BomLines (Id, BillOfMaterialId, LineNumber, ProductId, Quantity, UnitOfMeasure, Notes, CreatedAt)
    VALUES
    (NEWID(), @BomRef, 1, @Laptop, 1, 'PCS', 'Laptop unit', GETUTCDATE()),
    (NEWID(), @BomRef, 2, @Monitor, 1, 'PCS', 'External monitor', GETUTCDATE()),
    (NEWID(), @BomRef, 3, @Keyboard, 1, 'PCS', 'Wireless keyboard', GETUTCDATE()),
    (NEWID(), @BomRef, 4, @Mouse, 1, 'PCS', 'Wireless mouse', GETUTCDATE());
    PRINT 'Seeded BOM Lines';
END
GO

-- Get employee IDs
DECLARE @CTOEmp UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP003');
DECLARE @Dev1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP007');
DECLARE @Dev2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP008');
DECLARE @CustId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST003');

-- Seed Projects
IF NOT EXISTS (SELECT 1 FROM Projects WHERE Name = 'ERP Implementation')
BEGIN
    DECLARE @Proj1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Proj2 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO Projects (Id, Name, Description, StartDate, EndDate, Status, Priority, IsActive, CreatedAt)
    VALUES
    (@Proj1, 'ERP Implementation', 'Complete ERP system implementation', '2024-01-01', '2024-06-30', 1, 1, 1, GETUTCDATE()),
    (@Proj2, 'Website Redesign', 'Corporate website redesign', '2024-02-01', '2024-04-30', 0, 0, 1, GETUTCDATE());

    -- Add Project Tasks
    INSERT INTO ProjectTasks (Id, ProjectId, Title, Description, Status, Priority, DueDate, CreatedAt)
    VALUES
    (NEWID(), @Proj1, 'Requirements Gathering', 'Gather all requirements', 2, 1, '2024-01-15', GETUTCDATE()),
    (NEWID(), @Proj1, 'System Design', 'Design system architecture', 2, 1, '2024-01-31', GETUTCDATE()),
    (NEWID(), @Proj1, 'Development Phase 1', 'Core module development', 1, 1, '2024-03-15', GETUTCDATE()),
    (NEWID(), @Proj1, 'Development Phase 2', 'Additional modules', 0, 0, '2024-04-30', GETUTCDATE()),
    (NEWID(), @Proj2, 'Design Mockups', 'Create UI/UX designs', 1, 1, '2024-02-15', GETUTCDATE());

    PRINT 'Seeded Projects and Tasks';
END
GO

-- Get project and task IDs for time entries
DECLARE @Project1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Projects WHERE Name = 'ERP Implementation');
DECLARE @Task1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProjectTasks WHERE Title = 'Requirements Gathering');
DECLARE @Emp3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP003');

-- Seed Time Entries
IF @Project1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM TimeEntries WHERE ProjectId = @Project1)
BEGIN
    INSERT INTO TimeEntries (Id, ProjectId, Date, Hours, Description, IsBillable, HourlyRate, Status, CreatedAt)
    VALUES
    (NEWID(), @Project1, '2024-01-05', 8, 'Client meeting - requirements', 1, 2500, 1, GETUTCDATE()),
    (NEWID(), @Project1, '2024-01-08', 8, 'Document requirements', 1, 2500, 1, GETUTCDATE()),
    (NEWID(), @Project1, '2024-01-10', 6, 'Review session', 1, 2500, 1, GETUTCDATE()),
    (NEWID(), @Project1, '2024-01-15', 8, 'Finalize requirements', 1, 2500, 1, GETUTCDATE());
    PRINT 'Seeded Time Entries';
END
GO

-- Seed Work Orders
DECLARE @BomForWO UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM BillOfMaterials WHERE BomNumber = 'BOM-WS-001');
DECLARE @LaptopProd UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'LAPTOP-001');
DECLARE @MainWH UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Warehouses WHERE Code = 'WH-MAIN');

IF NOT EXISTS (SELECT 1 FROM WorkOrders WHERE WorkOrderNumber = 'WO-2024-001')
BEGIN
    INSERT INTO WorkOrders (Id, WorkOrderNumber, ProductId, WarehouseId, RequiredQuantity, Status, Priority, StartDate, DueDate, Notes, CreatedAt)
    VALUES
    (NEWID(), 'WO-2024-001', @LaptopProd, @MainWH, 10, 0, 1, '2024-02-01', '2024-02-15', 'Workstation assembly batch 1', GETUTCDATE()),
    (NEWID(), 'WO-2024-002', @LaptopProd, @MainWH, 5, 1, 0, '2024-01-20', '2024-01-30', 'Workstation assembly batch 2', GETUTCDATE());
    PRINT 'Seeded Work Orders';
END
GO

PRINT 'All remaining data seeded successfully';
GO
