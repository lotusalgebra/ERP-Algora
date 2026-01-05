-- =============================================
-- Algora ERP - Sales Module Tables
-- =============================================

-- Customers Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Customers') AND type in (N'U'))
BEGIN
    CREATE TABLE Customers (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        CustomerType INT NOT NULL DEFAULT 1, -- 0=Individual, 1=Company

        -- Contact Info
        ContactPerson NVARCHAR(100) NULL,
        Email NVARCHAR(255) NULL,
        Phone NVARCHAR(50) NULL,
        Mobile NVARCHAR(50) NULL,
        Website NVARCHAR(500) NULL,

        -- Billing Address
        BillingAddress NVARCHAR(500) NULL,
        BillingCity NVARCHAR(100) NULL,
        BillingState NVARCHAR(100) NULL,
        BillingCountry NVARCHAR(100) NULL,
        BillingPostalCode NVARCHAR(20) NULL,

        -- Shipping Address
        ShippingAddress NVARCHAR(500) NULL,
        ShippingCity NVARCHAR(100) NULL,
        ShippingState NVARCHAR(100) NULL,
        ShippingCountry NVARCHAR(100) NULL,
        ShippingPostalCode NVARCHAR(20) NULL,

        -- Tax Info
        TaxId NVARCHAR(50) NULL,
        IsTaxExempt BIT NOT NULL DEFAULT 0,

        -- Credit Terms
        PaymentTermsDays INT NOT NULL DEFAULT 30,
        CreditLimit DECIMAL(18,2) NOT NULL DEFAULT 0,
        CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',

        IsActive BIT NOT NULL DEFAULT 1,
        Notes NVARCHAR(1000) NULL,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    CREATE UNIQUE INDEX IX_Customers_Code ON Customers(Code) WHERE IsDeleted = 0;
END
GO

-- Sales Orders Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'SalesOrders') AND type in (N'U'))
BEGIN
    CREATE TABLE SalesOrders (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        OrderNumber NVARCHAR(20) NOT NULL,
        OrderDate DATETIME2 NOT NULL,
        DueDate DATETIME2 NULL,

        CustomerId UNIQUEIDENTIFIER NOT NULL,

        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Confirmed, 2=PartiallyShipped, 3=Shipped, etc.
        OrderType INT NOT NULL DEFAULT 0, -- 0=Standard, 1=Return, 2=Quote

        -- Shipping Info
        ShippingAddress NVARCHAR(500) NULL,
        ShippingMethod NVARCHAR(100) NULL,
        ExpectedDeliveryDate DATETIME2 NULL,

        -- Totals
        SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        ShippingAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',

        -- Payment
        AmountPaid DECIMAL(18,2) NOT NULL DEFAULT 0,

        Reference NVARCHAR(100) NULL,
        Notes NVARCHAR(1000) NULL,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_SalesOrders_Customer FOREIGN KEY (CustomerId)
            REFERENCES Customers(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_SalesOrders_OrderNumber ON SalesOrders(OrderNumber) WHERE IsDeleted = 0;
    CREATE INDEX IX_SalesOrders_OrderDate ON SalesOrders(OrderDate);
    CREATE INDEX IX_SalesOrders_CustomerId ON SalesOrders(CustomerId);
    CREATE INDEX IX_SalesOrders_Status ON SalesOrders(Status);
END
GO

-- Sales Order Lines Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'SalesOrderLines') AND type in (N'U'))
BEGIN
    CREATE TABLE SalesOrderLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        SalesOrderId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        ProductName NVARCHAR(200) NULL,
        ProductSku NVARCHAR(50) NULL,

        LineNumber INT NOT NULL DEFAULT 1,
        Quantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        UnitOfMeasure NVARCHAR(20) NULL,
        UnitPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        DiscountPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        LineTotal DECIMAL(18,2) NOT NULL DEFAULT 0,

        QuantityShipped DECIMAL(18,4) NOT NULL DEFAULT 0,
        QuantityReturned DECIMAL(18,4) NOT NULL DEFAULT 0,

        Notes NVARCHAR(500) NULL,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_SalesOrderLines_SalesOrder FOREIGN KEY (SalesOrderId)
            REFERENCES SalesOrders(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_SalesOrderLines_SalesOrderId ON SalesOrderLines(SalesOrderId);
END
GO

PRINT 'Sales tables created successfully';
