-- Seed sample stock data
SET QUOTED_IDENTIFIER ON;
GO

-- Insert stock levels for all products across all warehouses
INSERT INTO StockLevels (Id, ProductId, WarehouseId, LocationId, QuantityOnHand, QuantityReserved, QuantityOnOrder, CreatedAt, IsDeleted)
SELECT
    NEWID(),
    p.Id,
    w.Id,
    NULL,
    CASE
        WHEN w.Name = 'Main Warehouse' THEN ABS(CHECKSUM(NEWID())) % 500 + 100
        WHEN w.Name = 'East Coast DC' THEN ABS(CHECKSUM(NEWID())) % 300 + 50
        WHEN w.Name = 'West Coast DC' THEN ABS(CHECKSUM(NEWID())) % 300 + 50
        WHEN w.Name = 'Central Hub' THEN ABS(CHECKSUM(NEWID())) % 200 + 25
        ELSE ABS(CHECKSUM(NEWID())) % 100 + 10
    END,
    ABS(CHECKSUM(NEWID())) % 20, -- QuantityReserved
    ABS(CHECKSUM(NEWID())) % 50, -- QuantityOnOrder
    GETUTCDATE(),
    0
FROM Products p
CROSS JOIN Warehouses w
WHERE p.IsActive = 1 AND w.IsActive = 1;
GO

PRINT 'Stock levels created';
GO

-- Add some low stock items (update a few to be below reorder level from product)
UPDATE TOP (5) sl
SET sl.QuantityOnHand = 8
FROM StockLevels sl
INNER JOIN Products p ON sl.ProductId = p.Id
WHERE sl.QuantityOnHand > 50 AND p.ReorderLevel > 10;
GO

-- Add some out of stock items
UPDATE TOP (3) StockLevels
SET QuantityOnHand = 0
WHERE QuantityOnHand > 100;
GO

PRINT 'Low stock and out of stock items created';
GO

-- Insert some stock movements for history
INSERT INTO StockMovements (Id, ProductId, WarehouseId, MovementType, Quantity, Reference, Notes, MovementDate, CreatedAt, IsDeleted)
SELECT TOP 25
    NEWID(),
    sl.ProductId,
    sl.WarehouseId,
    CASE ABS(CHECKSUM(NEWID())) % 4
        WHEN 0 THEN 0  -- Receipt
        WHEN 1 THEN 1  -- Issue
        WHEN 2 THEN 2  -- Transfer
        ELSE 3         -- Adjustment
    END,
    ABS(CHECKSUM(NEWID())) % 100 + 10,
    CONCAT('MOV-', FORMAT(GETUTCDATE(), 'yyyyMMdd'), '-', RIGHT('0000' + CAST(ABS(CHECKSUM(NEWID())) % 10000 AS VARCHAR), 4)),
    CASE ABS(CHECKSUM(NEWID())) % 4
        WHEN 0 THEN 'Purchase order received'
        WHEN 1 THEN 'Sales order fulfilled'
        WHEN 2 THEN 'Inter-warehouse transfer'
        ELSE 'Inventory count adjustment'
    END,
    DATEADD(day, -ABS(CHECKSUM(NEWID())) % 14, GETUTCDATE()),
    GETUTCDATE(),
    0
FROM StockLevels sl;
GO

PRINT 'Stock movements created';
GO

-- Summary
SELECT COUNT(*) AS 'Total Warehouses' FROM Warehouses WHERE IsActive = 1;
SELECT COUNT(*) AS 'Total Stock Levels' FROM StockLevels WHERE IsDeleted = 0;
SELECT COUNT(*) AS 'Low Stock Items' FROM StockLevels sl
    INNER JOIN Products p ON sl.ProductId = p.Id
    WHERE sl.QuantityOnHand > 0 AND sl.QuantityOnHand <= p.ReorderLevel AND sl.IsDeleted = 0;
SELECT COUNT(*) AS 'Out of Stock Items' FROM StockLevels WHERE QuantityOnHand = 0 AND IsDeleted = 0;
SELECT COUNT(*) AS 'Stock Movements' FROM StockMovements WHERE IsDeleted = 0;
GO
