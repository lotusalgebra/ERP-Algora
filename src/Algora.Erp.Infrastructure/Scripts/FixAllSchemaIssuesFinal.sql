-- =============================================
-- COMPREHENSIVE SCHEMA FIX - ALL MISSING/MISNAMED COLUMNS
-- =============================================
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- =============================================
-- STOCK LEVELS - Fix column names
-- =============================================
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockLevels') AND name = 'Quantity')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockLevels') AND name = 'QuantityOnHand')
BEGIN
    EXEC sp_rename 'StockLevels.Quantity', 'QuantityOnHand', 'COLUMN';
    PRINT 'Renamed StockLevels.Quantity to QuantityOnHand';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockLevels') AND name = 'ReservedQuantity')
    AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockLevels') AND name = 'QuantityReserved')
BEGIN
    EXEC sp_rename 'StockLevels.ReservedQuantity', 'QuantityReserved', 'COLUMN';
    PRINT 'Renamed StockLevels.ReservedQuantity to QuantityReserved';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockLevels') AND name = 'QuantityOnOrder')
    ALTER TABLE StockLevels ADD QuantityOnOrder DECIMAL(18,4) NOT NULL DEFAULT 0;
GO

-- =============================================
-- PRODUCTS - Add missing columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'QuantityOnHand')
    ALTER TABLE Products ADD QuantityOnHand DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'QuantityReserved')
    ALTER TABLE Products ADD QuantityReserved DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ReorderLevel')
    ALTER TABLE Products ADD ReorderLevel DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'ReorderQuantity')
    ALTER TABLE Products ADD ReorderQuantity DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'MaximumStock')
    ALTER TABLE Products ADD MaximumStock DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'MinimumStock')
    ALTER TABLE Products ADD MinimumStock DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'TrackInventory')
    ALTER TABLE Products ADD TrackInventory BIT NOT NULL DEFAULT 1;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'GstSlabId')
    ALTER TABLE Products ADD GstSlabId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'HsnCode')
    ALTER TABLE Products ADD HsnCode NVARCHAR(50) NULL;
GO

-- =============================================
-- INVOICES - Ensure all GST columns exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'CgstAmount')
    ALTER TABLE Invoices ADD CgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'SgstAmount')
    ALTER TABLE Invoices ADD SgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'IgstAmount')
    ALTER TABLE Invoices ADD IgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Invoices') AND name = 'GstType')
    ALTER TABLE Invoices ADD GstType INT NOT NULL DEFAULT 0;
GO

-- =============================================
-- INVOICE LINES - Ensure all columns exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'CgstRate')
    ALTER TABLE InvoiceLines ADD CgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'SgstRate')
    ALTER TABLE InvoiceLines ADD SgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'IgstRate')
    ALTER TABLE InvoiceLines ADD IgstRate DECIMAL(5,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'CgstAmount')
    ALTER TABLE InvoiceLines ADD CgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'SgstAmount')
    ALTER TABLE InvoiceLines ADD SgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'IgstAmount')
    ALTER TABLE InvoiceLines ADD IgstAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('InvoiceLines') AND name = 'HsnCode')
    ALTER TABLE InvoiceLines ADD HsnCode NVARCHAR(50) NULL;
GO

-- =============================================
-- CUSTOMERS - Add missing columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'GstNumber')
    ALTER TABLE Customers ADD GstNumber NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'StateCode')
    ALTER TABLE Customers ADD StateCode NVARCHAR(10) NULL;
GO

-- =============================================
-- SUPPLIERS - Add missing columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'GstNumber')
    ALTER TABLE Suppliers ADD GstNumber NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'StateCode')
    ALTER TABLE Suppliers ADD StateCode NVARCHAR(10) NULL;
GO

-- =============================================
-- ECOMMERCE PRODUCTS - Fix column issues
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('EcommerceProducts') AND name = 'QuantityOnHand')
    ALTER TABLE EcommerceProducts ADD QuantityOnHand DECIMAL(18,4) NOT NULL DEFAULT 0;
GO

-- =============================================
-- WEB CATEGORIES - Ensure columns exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WebCategories') AND name = 'DisplayOrder')
    ALTER TABLE WebCategories ADD DisplayOrder INT NOT NULL DEFAULT 0;
GO

-- =============================================
-- WEB ORDERS - Ensure columns exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'ShippingCost')
    ALTER TABLE WebOrders ADD ShippingCost DECIMAL(18,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'TaxAmount')
    ALTER TABLE WebOrders ADD TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0;
GO

-- =============================================
-- STOCK MOVEMENTS - Ensure columns exist
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockMovements') AND name = 'SourceDocumentType')
    ALTER TABLE StockMovements ADD SourceDocumentType NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockMovements') AND name = 'SourceDocumentId')
    ALTER TABLE StockMovements ADD SourceDocumentId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockMovements') AND name = 'SourceDocumentNumber')
    ALTER TABLE StockMovements ADD SourceDocumentNumber NVARCHAR(50) NULL;
GO

-- =============================================
-- RECURRING INVOICES - Ensure soft delete columns
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RecurringInvoices') AND name = 'IsDeleted')
    ALTER TABLE RecurringInvoices ADD IsDeleted BIT NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RecurringInvoices') AND name = 'DeletedAt')
    ALTER TABLE RecurringInvoices ADD DeletedAt DATETIME2 NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RecurringInvoices') AND name = 'DeletedBy')
    ALTER TABLE RecurringInvoices ADD DeletedBy UNIQUEIDENTIFIER NULL;
GO

PRINT 'All schema fixes applied successfully!';
GO
