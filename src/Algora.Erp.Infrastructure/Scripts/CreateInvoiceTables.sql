SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- INVOICE TABLES
-- Run this script in the tenant database
-- =============================================

-- Invoices
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Invoices' AND xtype='U')
BEGIN
    CREATE TABLE Invoices (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        InvoiceNumber NVARCHAR(20) NOT NULL,
        Type INT NOT NULL DEFAULT 0, -- 0=SalesInvoice, 1=PurchaseInvoice, 2=CreditNote, 3=DebitNote
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Pending, 2=Sent, 3=PartiallyPaid, 4=Paid, 5=Overdue, 6=Void, 7=Cancelled

        -- Customer/Supplier
        CustomerId UNIQUEIDENTIFIER,
        SupplierId UNIQUEIDENTIFIER,

        -- Related documents
        SalesOrderId UNIQUEIDENTIFIER,
        PurchaseOrderId UNIQUEIDENTIFIER,

        -- Dates
        InvoiceDate DATETIME2 NOT NULL,
        DueDate DATETIME2 NOT NULL,
        PaidDate DATETIME2,

        -- Amounts
        SubTotal DECIMAL(18,2) DEFAULT 0,
        DiscountPercent DECIMAL(5,2) DEFAULT 0,
        DiscountAmount DECIMAL(18,2) DEFAULT 0,
        TaxAmount DECIMAL(18,2) DEFAULT 0,
        TotalAmount DECIMAL(18,2) DEFAULT 0,
        PaidAmount DECIMAL(18,2) DEFAULT 0,
        BalanceDue DECIMAL(18,2) DEFAULT 0,
        Currency NVARCHAR(3) DEFAULT 'USD',

        -- Payment Terms
        PaymentTerms NVARCHAR(100),
        PaymentTermDays INT DEFAULT 30,

        -- Billing Address
        BillingName NVARCHAR(200),
        BillingAddress NVARCHAR(500),
        BillingCity NVARCHAR(100),
        BillingState NVARCHAR(100),
        BillingPostalCode NVARCHAR(20),
        BillingCountry NVARCHAR(100),

        -- Additional Info
        Reference NVARCHAR(100),
        Notes NVARCHAR(1000),
        InternalNotes NVARCHAR(1000),
        AttachmentUrl NVARCHAR(500),

        -- Tracking
        SentBy UNIQUEIDENTIFIER,
        SentAt DATETIME2,
        ApprovedBy UNIQUEIDENTIFIER,
        ApprovedAt DATETIME2,

        -- Audit
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,

        CONSTRAINT FK_Invoices_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Invoices_Supplier FOREIGN KEY (SupplierId) REFERENCES Suppliers(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Invoices_SalesOrder FOREIGN KEY (SalesOrderId) REFERENCES SalesOrders(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Invoices_PurchaseOrder FOREIGN KEY (PurchaseOrderId) REFERENCES PurchaseOrders(Id) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber) WHERE IsDeleted = 0;
    CREATE INDEX IX_Invoices_CustomerId ON Invoices(CustomerId) WHERE IsDeleted = 0;
    CREATE INDEX IX_Invoices_SupplierId ON Invoices(SupplierId) WHERE IsDeleted = 0;
    CREATE INDEX IX_Invoices_Status ON Invoices(Status) WHERE IsDeleted = 0;
    CREATE INDEX IX_Invoices_DueDate ON Invoices(DueDate) WHERE IsDeleted = 0;
    CREATE INDEX IX_Invoices_InvoiceDate ON Invoices(InvoiceDate) WHERE IsDeleted = 0;

    PRINT 'Created Invoices table';
END
GO

-- Invoice Lines
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InvoiceLines' AND xtype='U')
BEGIN
    CREATE TABLE InvoiceLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        InvoiceId UNIQUEIDENTIFIER NOT NULL,

        LineNumber INT DEFAULT 1,
        ProductId UNIQUEIDENTIFIER,
        ProductCode NVARCHAR(50),
        Description NVARCHAR(500) NOT NULL,

        Quantity DECIMAL(18,4) DEFAULT 1,
        UnitOfMeasure NVARCHAR(20),
        UnitPrice DECIMAL(18,2) DEFAULT 0,
        DiscountPercent DECIMAL(5,2) DEFAULT 0,
        DiscountAmount DECIMAL(18,2) DEFAULT 0,
        TaxPercent DECIMAL(5,2) DEFAULT 0,
        TaxAmount DECIMAL(18,2) DEFAULT 0,
        LineTotal DECIMAL(18,2) DEFAULT 0,

        AccountId UNIQUEIDENTIFIER,
        Notes NVARCHAR(500),

        -- Audit
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,

        CONSTRAINT FK_InvoiceLines_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE,
        CONSTRAINT FK_InvoiceLines_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE SET NULL,
        CONSTRAINT FK_InvoiceLines_Account FOREIGN KEY (AccountId) REFERENCES Accounts(Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_InvoiceLines_InvoiceId ON InvoiceLines(InvoiceId);

    PRINT 'Created InvoiceLines table';
END
GO

-- Invoice Payments
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='InvoicePayments' AND xtype='U')
BEGIN
    CREATE TABLE InvoicePayments (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        InvoiceId UNIQUEIDENTIFIER NOT NULL,

        PaymentNumber NVARCHAR(20) NOT NULL,
        PaymentDate DATETIME2 NOT NULL,
        Amount DECIMAL(18,2) NOT NULL,
        PaymentMethod INT NOT NULL DEFAULT 0, -- 0=Cash, 1=Check, 2=BankTransfer, 3=CreditCard, 4=DebitCard, 5=PayPal, 6=Other
        Reference NVARCHAR(100),
        Notes NVARCHAR(500),

        ReceivedBy UNIQUEIDENTIFIER,

        -- Audit
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,

        CONSTRAINT FK_InvoicePayments_Invoice FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_InvoicePayments_InvoiceId ON InvoicePayments(InvoiceId);
    CREATE INDEX IX_InvoicePayments_PaymentDate ON InvoicePayments(PaymentDate);

    PRINT 'Created InvoicePayments table';
END
GO

PRINT 'Invoice tables created successfully';
GO
