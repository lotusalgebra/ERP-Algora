-- Recurring Invoices Tables
-- Run this script against the tenant database (e.g., AlgoraErp_Dev)

SET QUOTED_IDENTIFIER ON;
GO

-- Create RecurringInvoices table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RecurringInvoices')
BEGIN
    CREATE TABLE RecurringInvoices (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Status INT NOT NULL DEFAULT 0, -- 0=Active, 1=Paused, 2=Completed, 3=Cancelled

        -- Customer
        CustomerId UNIQUEIDENTIFIER NOT NULL,

        -- Schedule
        Frequency INT NOT NULL DEFAULT 2, -- 0=Daily, 1=Weekly, 2=Monthly, 3=Quarterly, 4=Yearly
        FrequencyInterval INT NOT NULL DEFAULT 1,
        DayOfMonth INT NULL,
        DayOfWeek INT NULL,
        StartDate DATE NOT NULL,
        EndDate DATE NULL,
        MaxOccurrences INT NULL,
        OccurrencesGenerated INT NOT NULL DEFAULT 0,
        NextGenerationDate DATE NULL,
        LastGeneratedDate DATE NULL,

        -- Invoice Template
        InvoiceType INT NOT NULL DEFAULT 0,
        PaymentTermDays INT NOT NULL DEFAULT 30,
        PaymentTerms NVARCHAR(50) NULL,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',

        -- Billing Address
        BillingName NVARCHAR(200) NULL,
        BillingAddress NVARCHAR(500) NULL,
        BillingCity NVARCHAR(100) NULL,
        BillingState NVARCHAR(100) NULL,
        BillingPostalCode NVARCHAR(20) NULL,
        BillingCountry NVARCHAR(100) NULL,

        -- Additional Info
        Reference NVARCHAR(100) NULL,
        Notes NVARCHAR(MAX) NULL,
        InternalNotes NVARCHAR(MAX) NULL,

        -- Auto-send options
        AutoSend BIT NOT NULL DEFAULT 0,
        AutoSendEmail BIT NOT NULL DEFAULT 0,
        EmailRecipients NVARCHAR(500) NULL,

        -- Audit fields
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_RecurringInvoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
    );

    CREATE INDEX IX_RecurringInvoices_CustomerId ON RecurringInvoices(CustomerId);
    CREATE INDEX IX_RecurringInvoices_Status ON RecurringInvoices(Status) WHERE IsDeleted = 0;
    CREATE INDEX IX_RecurringInvoices_NextGenerationDate ON RecurringInvoices(NextGenerationDate) WHERE IsDeleted = 0 AND Status = 0;

    PRINT 'Created RecurringInvoices table';
END
GO

-- Create RecurringInvoiceLines table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RecurringInvoiceLines')
BEGIN
    CREATE TABLE RecurringInvoiceLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        RecurringInvoiceId UNIQUEIDENTIFIER NOT NULL,
        LineNumber INT NOT NULL,
        ProductId UNIQUEIDENTIFIER NULL,
        ProductCode NVARCHAR(50) NULL,
        Description NVARCHAR(500) NOT NULL,
        Quantity DECIMAL(18, 4) NOT NULL DEFAULT 1,
        UnitOfMeasure NVARCHAR(20) NULL,
        UnitPrice DECIMAL(18, 4) NOT NULL DEFAULT 0,
        DiscountPercent DECIMAL(5, 2) NOT NULL DEFAULT 0,
        TaxPercent DECIMAL(5, 2) NOT NULL DEFAULT 0,
        AccountId UNIQUEIDENTIFIER NULL,
        Notes NVARCHAR(500) NULL,

        -- Audit fields
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_RecurringInvoiceLines_RecurringInvoices FOREIGN KEY (RecurringInvoiceId) REFERENCES RecurringInvoices(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_RecurringInvoiceLines_RecurringInvoiceId ON RecurringInvoiceLines(RecurringInvoiceId);

    PRINT 'Created RecurringInvoiceLines table';
END
GO

-- Add RecurringInvoiceId column to Invoices table
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'RecurringInvoiceId')
BEGIN
    ALTER TABLE Invoices ADD RecurringInvoiceId UNIQUEIDENTIFIER NULL;

    ALTER TABLE Invoices ADD CONSTRAINT FK_Invoices_RecurringInvoices
        FOREIGN KEY (RecurringInvoiceId) REFERENCES RecurringInvoices(Id);

    CREATE INDEX IX_Invoices_RecurringInvoiceId ON Invoices(RecurringInvoiceId) WHERE RecurringInvoiceId IS NOT NULL;

    PRINT 'Added RecurringInvoiceId column to Invoices table';
END
GO

PRINT 'Recurring Invoice tables setup complete!';
GO
