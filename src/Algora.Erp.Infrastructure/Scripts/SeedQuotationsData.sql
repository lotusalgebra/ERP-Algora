-- Seed Quotations Sample Data for Algora.Erp
-- Run this script after base data is seeded

SET QUOTED_IDENTIFIER ON;
GO

-- Get some customer IDs
DECLARE @Customer1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE IsActive = 1 ORDER BY Name);
DECLARE @Customer2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE IsActive = 1 AND Id != @Customer1Id ORDER BY Name);
DECLARE @Customer3Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE IsActive = 1 AND Id != @Customer1Id AND Id != @Customer2Id ORDER BY Name);

-- Get some product IDs
DECLARE @Product1Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE IsActive = 1 AND IsSellable = 1 ORDER BY Name);
DECLARE @Product2Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE IsActive = 1 AND IsSellable = 1 AND Id != @Product1Id ORDER BY Name);
DECLARE @Product3Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE IsActive = 1 AND IsSellable = 1 AND Id != @Product1Id AND Id != @Product2Id ORDER BY Name);

-- Only proceed if we have customers and products
IF @Customer1Id IS NOT NULL AND @Product1Id IS NOT NULL
BEGIN
    -- Quotation 1: Draft (new quote)
    DECLARE @Quote1Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO SalesOrders (Id, OrderNumber, OrderDate, DueDate, CustomerId, Status, OrderType, SubTotal, DiscountAmount, TaxAmount, ShippingAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
    VALUES (
        @Quote1Id,
        'QT202501001',
        DATEADD(DAY, -2, GETUTCDATE()),
        DATEADD(DAY, 28, GETUTCDATE()),
        @Customer1Id,
        0, -- Draft
        2, -- Quote
        2500.00,
        0,
        200.00,
        0,
        2700.00,
        'USD',
        'RFQ-2025-001',
        'Initial quote for office equipment',
        GETUTCDATE()
    );

    -- Quote 1 Lines
    IF @Product1Id IS NOT NULL
    BEGIN
        INSERT INTO SalesOrderLines (Id, SalesOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, LineTotal, CreatedAt)
        SELECT NEWID(), @Quote1Id, Id, Name, Sku, 1, 5, UnitOfMeasure, 500.00, 0, 0, 8, 200.00, 2700.00, GETUTCDATE()
        FROM Products WHERE Id = @Product1Id;
    END

    -- Quotation 2: Sent (waiting for response)
    DECLARE @Quote2Id UNIQUEIDENTIFIER = NEWID();
    IF @Customer2Id IS NOT NULL
    BEGIN
        INSERT INTO SalesOrders (Id, OrderNumber, OrderDate, DueDate, CustomerId, Status, OrderType, SubTotal, DiscountAmount, TaxAmount, ShippingAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
        VALUES (
            @Quote2Id,
            'QT202501002',
            DATEADD(DAY, -5, GETUTCDATE()),
            DATEADD(DAY, 25, GETUTCDATE()),
            @Customer2Id,
            1, -- Confirmed = Sent for quotes
            2, -- Quote
            8750.00,
            250.00,
            680.00,
            0,
            9180.00,
            'USD',
            'RFQ-2025-002',
            'Manufacturing equipment quote - awaiting customer response',
            GETUTCDATE()
        );

        -- Quote 2 Lines
        IF @Product2Id IS NOT NULL
        BEGIN
            INSERT INTO SalesOrderLines (Id, SalesOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, LineTotal, CreatedAt)
            SELECT NEWID(), @Quote2Id, Id, Name, Sku, 1, 10, UnitOfMeasure, 875.00, 0, 0, 8, 680.00, 9180.00, GETUTCDATE()
            FROM Products WHERE Id = @Product2Id;
        END
    END

    -- Quotation 3: Accepted (converted to order)
    DECLARE @Quote3Id UNIQUEIDENTIFIER = NEWID();
    IF @Customer3Id IS NOT NULL
    BEGIN
        INSERT INTO SalesOrders (Id, OrderNumber, OrderDate, DueDate, CustomerId, Status, OrderType, SubTotal, DiscountAmount, TaxAmount, ShippingAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
        VALUES (
            @Quote3Id,
            'QT202501003',
            DATEADD(DAY, -10, GETUTCDATE()),
            DATEADD(DAY, 20, GETUTCDATE()),
            @Customer3Id,
            7, -- Paid = Accepted for quotes
            2, -- Quote
            4500.00,
            500.00,
            320.00,
            0,
            4320.00,
            'USD',
            'RFQ-2025-003',
            'Office supplies quote - accepted and converted to SO202501015',
            GETUTCDATE()
        );

        -- Quote 3 Lines
        IF @Product3Id IS NOT NULL
        BEGIN
            INSERT INTO SalesOrderLines (Id, SalesOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, LineTotal, CreatedAt)
            SELECT NEWID(), @Quote3Id, Id, Name, Sku, 1, 15, UnitOfMeasure, 300.00, 0, 0, 8, 320.00, 4320.00, GETUTCDATE()
            FROM Products WHERE Id = @Product3Id;
        END
    END

    -- Quotation 4: Rejected
    DECLARE @Quote4Id UNIQUEIDENTIFIER = NEWID();
    INSERT INTO SalesOrders (Id, OrderNumber, OrderDate, DueDate, CustomerId, Status, OrderType, SubTotal, DiscountAmount, TaxAmount, ShippingAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
    VALUES (
        @Quote4Id,
        'QT202501004',
        DATEADD(DAY, -15, GETUTCDATE()),
        DATEADD(DAY, -5, GETUTCDATE()),
        @Customer1Id,
        8, -- Cancelled = Rejected for quotes
        2, -- Quote
        12000.00,
        0,
        960.00,
        0,
        12960.00,
        'USD',
        'RFQ-2025-004',
        'Customer chose competitor - price was not competitive',
        GETUTCDATE()
    );

    -- Quote 4 Lines
    IF @Product1Id IS NOT NULL
    BEGIN
        INSERT INTO SalesOrderLines (Id, SalesOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, LineTotal, CreatedAt)
        SELECT NEWID(), @Quote4Id, Id, Name, Sku, 1, 20, UnitOfMeasure, 600.00, 0, 0, 8, 960.00, 12960.00, GETUTCDATE()
        FROM Products WHERE Id = @Product1Id;
    END

    -- Quotation 5: Expired
    DECLARE @Quote5Id UNIQUEIDENTIFIER = NEWID();
    IF @Customer2Id IS NOT NULL
    BEGIN
        INSERT INTO SalesOrders (Id, OrderNumber, OrderDate, DueDate, CustomerId, Status, OrderType, SubTotal, DiscountAmount, TaxAmount, ShippingAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
        VALUES (
            @Quote5Id,
            'QT202412001',
            DATEADD(DAY, -45, GETUTCDATE()),
            DATEADD(DAY, -15, GETUTCDATE()),
            @Customer2Id,
            9, -- OnHold = Expired for quotes
            2, -- Quote
            3200.00,
            0,
            256.00,
            0,
            3456.00,
            'USD',
            'RFQ-2024-050',
            'Quote expired - customer did not respond within validity period',
            GETUTCDATE()
        );

        -- Quote 5 Lines
        IF @Product2Id IS NOT NULL
        BEGIN
            INSERT INTO SalesOrderLines (Id, SalesOrderId, ProductId, ProductName, ProductSku, LineNumber, Quantity, UnitOfMeasure, UnitPrice, DiscountPercent, DiscountAmount, TaxPercent, TaxAmount, LineTotal, CreatedAt)
            SELECT NEWID(), @Quote5Id, Id, Name, Sku, 1, 8, UnitOfMeasure, 400.00, 0, 0, 8, 256.00, 3456.00, GETUTCDATE()
            FROM Products WHERE Id = @Product2Id;
        END
    END

    PRINT 'Quotations sample data inserted successfully!';
END
ELSE
BEGIN
    PRINT 'Skipping quotations seed - no customers or products found. Run customer and product seeds first.';
END
GO
