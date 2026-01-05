SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Algora ERP - Procurement Module Tables
-- =============================================

-- Suppliers Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Suppliers') AND type in (N'U'))
BEGIN
    CREATE TABLE Suppliers (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,

        -- Contact Info
        ContactPerson NVARCHAR(100) NULL,
        Email NVARCHAR(255) NULL,
        Phone NVARCHAR(50) NULL,
        Fax NVARCHAR(50) NULL,
        Website NVARCHAR(500) NULL,

        -- Address
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,

        -- Bank Info
        BankName NVARCHAR(100) NULL,
        BankAccountNumber NVARCHAR(50) NULL,
        BankRoutingNumber NVARCHAR(50) NULL,

        -- Tax Info
        TaxId NVARCHAR(50) NULL,

        -- Payment Terms
        PaymentTermsDays INT NOT NULL DEFAULT 30,
        CurrentBalance DECIMAL(18,2) NOT NULL DEFAULT 0,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',

        -- Lead Time
        LeadTimeDays INT NOT NULL DEFAULT 0,
        MinimumOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,

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

    CREATE UNIQUE INDEX IX_Suppliers_Code ON Suppliers(Code) WHERE IsDeleted = 0;
END
GO

-- Purchase Orders Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'PurchaseOrders') AND type in (N'U'))
BEGIN
    CREATE TABLE PurchaseOrders (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        OrderNumber NVARCHAR(20) NOT NULL,
        OrderDate DATETIME2 NOT NULL,
        DueDate DATETIME2 NULL,
        ExpectedDeliveryDate DATETIME2 NULL,

        SupplierId UNIQUEIDENTIFIER NOT NULL,
        WarehouseId UNIQUEIDENTIFIER NULL,

        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Pending, 2=Approved, 3=Sent, etc.

        -- Shipping Info
        ShippingAddress NVARCHAR(500) NULL,
        ShippingMethod NVARCHAR(100) NULL,

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

        ApprovedBy UNIQUEIDENTIFIER NULL,
        ApprovedAt DATETIME2 NULL,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_PurchaseOrders_Supplier FOREIGN KEY (SupplierId)
            REFERENCES Suppliers(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_PurchaseOrders_OrderNumber ON PurchaseOrders(OrderNumber) WHERE IsDeleted = 0;
    CREATE INDEX IX_PurchaseOrders_OrderDate ON PurchaseOrders(OrderDate);
    CREATE INDEX IX_PurchaseOrders_SupplierId ON PurchaseOrders(SupplierId);
    CREATE INDEX IX_PurchaseOrders_Status ON PurchaseOrders(Status);
END
GO

-- Purchase Order Lines Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'PurchaseOrderLines') AND type in (N'U'))
BEGIN
    CREATE TABLE PurchaseOrderLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        PurchaseOrderId UNIQUEIDENTIFIER NOT NULL,
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

        QuantityReceived DECIMAL(18,4) NOT NULL DEFAULT 0,
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

        CONSTRAINT FK_PurchaseOrderLines_PurchaseOrder FOREIGN KEY (PurchaseOrderId)
            REFERENCES PurchaseOrders(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_PurchaseOrderLines_PurchaseOrderId ON PurchaseOrderLines(PurchaseOrderId);
END
GO

PRINT 'Procurement tables created successfully';
