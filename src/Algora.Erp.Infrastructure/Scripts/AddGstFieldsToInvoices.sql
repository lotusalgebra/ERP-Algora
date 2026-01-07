-- Add GST fields to Invoices table
SET QUOTED_IDENTIFIER ON;
GO

-- Add GST columns to Invoices
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'GstSlabId')
BEGIN
    ALTER TABLE Invoices ADD GstSlabId UNIQUEIDENTIFIER NULL;
    ALTER TABLE Invoices ADD IsInterState BIT NOT NULL DEFAULT 0;
    ALTER TABLE Invoices ADD CgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    ALTER TABLE Invoices ADD SgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    ALTER TABLE Invoices ADD IgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    ALTER TABLE Invoices ADD CgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE Invoices ADD SgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE Invoices ADD IgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE Invoices ADD FromLocationId UNIQUEIDENTIFIER NULL;
    ALTER TABLE Invoices ADD CustomerGstin NVARCHAR(20) NULL;
    ALTER TABLE Invoices ADD CustomerStateCode NVARCHAR(5) NULL;

    -- Add foreign keys
    ALTER TABLE Invoices ADD CONSTRAINT FK_Invoices_GstSlab
        FOREIGN KEY (GstSlabId) REFERENCES GstSlabs(Id);
    ALTER TABLE Invoices ADD CONSTRAINT FK_Invoices_FromLocation
        FOREIGN KEY (FromLocationId) REFERENCES OfficeLocations(Id);

    PRINT 'Added GST columns to Invoices table';
END
ELSE
BEGIN
    PRINT 'GST columns already exist in Invoices table';
END
GO

-- Add GST columns to InvoiceLines
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'GstSlabId')
BEGIN
    ALTER TABLE InvoiceLines ADD GstSlabId UNIQUEIDENTIFIER NULL;
    ALTER TABLE InvoiceLines ADD CgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE InvoiceLines ADD SgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE InvoiceLines ADD IgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
    ALTER TABLE InvoiceLines ADD CgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    ALTER TABLE InvoiceLines ADD SgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    ALTER TABLE InvoiceLines ADD IgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
    ALTER TABLE InvoiceLines ADD HsnCode NVARCHAR(20) NULL;

    -- Add foreign key
    ALTER TABLE InvoiceLines ADD CONSTRAINT FK_InvoiceLines_GstSlab
        FOREIGN KEY (GstSlabId) REFERENCES GstSlabs(Id);

    PRINT 'Added GST columns to InvoiceLines table';
END
ELSE
BEGIN
    PRINT 'GST columns already exist in InvoiceLines table';
END
GO

-- Update default currency to INR
UPDATE Invoices SET Currency = 'INR' WHERE Currency = 'USD';
GO

PRINT 'GST fields added to Invoices successfully';
