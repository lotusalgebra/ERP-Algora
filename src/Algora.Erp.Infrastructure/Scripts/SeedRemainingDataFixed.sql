-- Seed remaining data with correct column names
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- Get product IDs and warehouse ID
DECLARE @LaptopProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'LAPTOP-001');
DECLARE @MainWH UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Warehouses WHERE Code = 'WH-MAIN');
DECLARE @Customer1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST001');
DECLARE @Employee1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP001');

-- Seed Projects with correct column names
IF NOT EXISTS (SELECT 1 FROM Projects WHERE ProjectCode = 'PROJ-001')
BEGIN
    DECLARE @Proj1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Proj2 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO Projects (Id, ProjectCode, Name, Description, CustomerId, ProjectManagerId, Status, Priority, StartDate, EndDate, BudgetAmount, IsActive, CreatedAt)
    VALUES
    (@Proj1, 'PROJ-001', 'ERP Implementation', 'Complete ERP system implementation', @Customer1, @Employee1, 1, 1, '2024-01-01', '2024-06-30', 500000, 1, GETUTCDATE()),
    (@Proj2, 'PROJ-002', 'Website Redesign', 'Corporate website redesign', @Customer1, @Employee1, 0, 0, '2024-02-01', '2024-04-30', 150000, 1, GETUTCDATE());

    -- Add Project Tasks
    INSERT INTO ProjectTasks (Id, ProjectId, TaskNumber, Title, Description, Status, Priority, DueDate, EstimatedHours, CreatedAt)
    VALUES
    (NEWID(), @Proj1, 'TASK-001', 'Requirements Gathering', 'Gather all requirements', 2, 1, '2024-01-15', 40, GETUTCDATE()),
    (NEWID(), @Proj1, 'TASK-002', 'System Design', 'Design system architecture', 2, 1, '2024-01-31', 60, GETUTCDATE()),
    (NEWID(), @Proj1, 'TASK-003', 'Development Phase 1', 'Core module development', 1, 1, '2024-03-15', 200, GETUTCDATE()),
    (NEWID(), @Proj1, 'TASK-004', 'Development Phase 2', 'Additional modules', 0, 0, '2024-04-30', 160, GETUTCDATE()),
    (NEWID(), @Proj2, 'TASK-005', 'Design Mockups', 'Create UI/UX designs', 1, 1, '2024-02-15', 40, GETUTCDATE());

    PRINT 'Seeded Projects and Tasks';
END
GO

-- Get project ID for time entries
DECLARE @Project1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Projects WHERE ProjectCode = 'PROJ-001');
DECLARE @Task1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProjectTasks WHERE TaskNumber = 'TASK-001');
DECLARE @UserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users);

-- Seed Time Entries
IF @Project1 IS NOT NULL AND NOT EXISTS (SELECT 1 FROM TimeEntries WHERE ProjectId = @Project1)
BEGIN
    INSERT INTO TimeEntries (Id, ProjectId, TaskId, UserId, Date, Hours, Description, IsBillable, HourlyRate, Status, CreatedAt, IsDeleted)
    VALUES
    (NEWID(), @Project1, @Task1, @UserId, '2024-01-05', 8, 'Client meeting - requirements', 1, 2500, 1, GETUTCDATE(), 0),
    (NEWID(), @Project1, @Task1, @UserId, '2024-01-08', 8, 'Document requirements', 1, 2500, 1, GETUTCDATE(), 0),
    (NEWID(), @Project1, @Task1, @UserId, '2024-01-10', 6, 'Review session', 1, 2500, 1, GETUTCDATE(), 0),
    (NEWID(), @Project1, @Task1, @UserId, '2024-01-15', 8, 'Finalize requirements', 1, 2500, 1, GETUTCDATE(), 0);
    PRINT 'Seeded Time Entries';
END
GO

-- Get BOM ID for WorkOrders
DECLARE @BomRef UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM BillOfMaterials WHERE BomNumber = 'BOM-WS-001');
DECLARE @LaptopProd UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'LAPTOP-001');
DECLARE @MainWarehouse UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Warehouses WHERE Code = 'WH-MAIN');

-- Seed Work Orders with correct column names
IF NOT EXISTS (SELECT 1 FROM WorkOrders WHERE WorkOrderNumber = 'WO-2024-001')
BEGIN
    INSERT INTO WorkOrders (Id, WorkOrderNumber, Name, BillOfMaterialId, ProductId, PlannedQuantity, Quantity, Status, Priority, PlannedStartDate, PlannedEndDate, WarehouseId, Notes, CreatedAt)
    VALUES
    (NEWID(), 'WO-2024-001', 'Workstation Assembly Batch 1', @BomRef, @LaptopProd, 10, 10, 0, 1, '2024-02-01', '2024-02-15', @MainWarehouse, 'Workstation assembly batch 1', GETUTCDATE()),
    (NEWID(), 'WO-2024-002', 'Workstation Assembly Batch 2', @BomRef, @LaptopProd, 5, 5, 1, 0, '2024-01-20', '2024-01-30', @MainWarehouse, 'Workstation assembly batch 2', GETUTCDATE());
    PRINT 'Seeded Work Orders';
END
GO

PRINT 'All remaining data seeded successfully';
GO
