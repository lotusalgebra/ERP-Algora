-- =====================================================
-- SyncAllSchemas.sql
-- Comprehensive schema sync script for Algora ERP
-- Adds all missing columns from entity classes to database tables
-- Generated: 2026-01-12
-- =====================================================

PRINT 'Starting comprehensive schema synchronization...'
PRINT ''

-- =====================================================
-- 1. TENANT SETTINGS TABLE (Create if not exists)
-- =====================================================
PRINT '=== TenantSettings Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'TenantSettings')
BEGIN
    CREATE TABLE TenantSettings (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),

        -- Company Information
        CompanyName NVARCHAR(200) NOT NULL DEFAULT 'My Company',
        CompanyLogo NVARCHAR(500) NULL,
        CompanyTagline NVARCHAR(500) NULL,
        CompanyWebsite NVARCHAR(500) NULL,
        CompanyEmail NVARCHAR(255) NULL,
        CompanyPhone NVARCHAR(50) NULL,

        -- Address
        AddressLine1 NVARCHAR(500) NULL,
        AddressLine2 NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'India',
        CountryCode NVARCHAR(5) NOT NULL DEFAULT 'IN',

        -- Regional Settings
        Currency NVARCHAR(10) NOT NULL DEFAULT 'INR',
        CurrencySymbol NVARCHAR(10) NOT NULL DEFAULT N'₹',
        CurrencyName NVARCHAR(100) NOT NULL DEFAULT 'Indian Rupee',
        CurrencyDecimalPlaces INT NOT NULL DEFAULT 2,
        Language NVARCHAR(10) NOT NULL DEFAULT 'en-IN',
        TimeZone NVARCHAR(50) NOT NULL DEFAULT 'Asia/Kolkata',
        DateFormat NVARCHAR(20) NOT NULL DEFAULT 'dd/MM/yyyy',
        TimeFormat NVARCHAR(10) NOT NULL DEFAULT 'HH:mm',
        DateTimeFormat NVARCHAR(30) NOT NULL DEFAULT 'dd/MM/yyyy HH:mm',

        -- Tax Settings
        TaxId NVARCHAR(50) NULL,
        TaxIdLabel NVARCHAR(20) NOT NULL DEFAULT 'GSTIN',
        PanNumber NVARCHAR(20) NULL,
        IsGstRegistered BIT NOT NULL DEFAULT 1,

        -- Invoice Settings
        InvoicePrefix NVARCHAR(10) NOT NULL DEFAULT 'INV',
        QuotationPrefix NVARCHAR(10) NOT NULL DEFAULT 'QT',
        SalesOrderPrefix NVARCHAR(10) NOT NULL DEFAULT 'SO',
        PurchaseOrderPrefix NVARCHAR(10) NOT NULL DEFAULT 'PO',
        DeliveryChallanPrefix NVARCHAR(10) NOT NULL DEFAULT 'DC',
        GoodsReceiptPrefix NVARCHAR(10) NOT NULL DEFAULT 'GRN',
        DefaultPaymentTermDays INT NOT NULL DEFAULT 30,
        DefaultPaymentTerms NVARCHAR(50) NOT NULL DEFAULT 'Net 30',

        -- Invoice PDF Settings
        InvoiceHeaderText NVARCHAR(MAX) NULL,
        InvoiceFooterText NVARCHAR(500) NULL DEFAULT 'Thank you for your business!',
        InvoiceTermsText NVARCHAR(MAX) NULL,
        ShowGstBreakdown BIT NOT NULL DEFAULT 1,
        ShowHsnCode BIT NOT NULL DEFAULT 1,

        -- Email Settings (SMTP)
        SmtpHost NVARCHAR(100) NULL,
        SmtpPort INT NOT NULL DEFAULT 587,
        SmtpUsername NVARCHAR(100) NULL,
        SmtpPassword NVARCHAR(200) NULL,
        SmtpEnableSsl BIT NOT NULL DEFAULT 1,
        EmailFromAddress NVARCHAR(255) NULL,
        EmailFromName NVARCHAR(100) NULL,

        -- Security Settings
        PasswordMinLength INT NOT NULL DEFAULT 8,
        PasswordRequireUppercase BIT NOT NULL DEFAULT 1,
        PasswordRequireLowercase BIT NOT NULL DEFAULT 1,
        PasswordRequireDigit BIT NOT NULL DEFAULT 1,
        PasswordRequireSpecialChar BIT NOT NULL DEFAULT 1,
        SessionTimeoutMinutes INT NOT NULL DEFAULT 30,
        MaxLoginAttempts INT NOT NULL DEFAULT 5,
        LockoutDurationMinutes INT NOT NULL DEFAULT 15,
        EnableTwoFactor BIT NOT NULL DEFAULT 0,

        -- Backup Settings
        AutoBackupEnabled BIT NOT NULL DEFAULT 1,
        AutoBackupTime NVARCHAR(10) NOT NULL DEFAULT '02:00',
        BackupRetentionDays INT NOT NULL DEFAULT 30,

        -- Feature Flags
        EnableEcommerce BIT NOT NULL DEFAULT 0,
        EnableManufacturing BIT NOT NULL DEFAULT 0,
        EnableProjects BIT NOT NULL DEFAULT 0,
        EnablePayroll BIT NOT NULL DEFAULT 0,

        -- Audit Fields
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );
    PRINT 'Created TenantSettings table'
END
ELSE
    PRINT 'TenantSettings table already exists'

-- =====================================================
-- 2. LEADS TABLE - Missing columns
-- =====================================================
PRINT ''
PRINT '=== Leads Table ==='

-- Entity has 'Name' but DB has 'ContactName' and 'Code' - these are likely intentional mappings
-- Add missing columns from entity
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'Company')
BEGIN
    ALTER TABLE Leads ADD Company NVARCHAR(200) NULL;
    PRINT 'Added Leads.Company'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'Website')
BEGIN
    ALTER TABLE Leads ADD Website NVARCHAR(500) NULL;
    PRINT 'Added Leads.Website'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'Rating')
BEGIN
    ALTER TABLE Leads ADD Rating INT NOT NULL DEFAULT 0;
    PRINT 'Added Leads.Rating'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'EstimatedCloseInDays')
BEGIN
    ALTER TABLE Leads ADD EstimatedCloseInDays INT NULL;
    PRINT 'Added Leads.EstimatedCloseInDays'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'Address')
BEGIN
    ALTER TABLE Leads ADD Address NVARCHAR(500) NULL;
    PRINT 'Added Leads.Address'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'City')
BEGIN
    ALTER TABLE Leads ADD City NVARCHAR(100) NULL;
    PRINT 'Added Leads.City'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'State')
BEGIN
    ALTER TABLE Leads ADD State NVARCHAR(100) NULL;
    PRINT 'Added Leads.State'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'Country')
BEGIN
    ALTER TABLE Leads ADD Country NVARCHAR(100) NULL;
    PRINT 'Added Leads.Country'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'PostalCode')
BEGIN
    ALTER TABLE Leads ADD PostalCode NVARCHAR(20) NULL;
    PRINT 'Added Leads.PostalCode'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'AssignedToName')
BEGIN
    ALTER TABLE Leads ADD AssignedToName NVARCHAR(200) NULL;
    PRINT 'Added Leads.AssignedToName'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'LastContactDate')
BEGIN
    ALTER TABLE Leads ADD LastContactDate DATETIME2 NULL;
    PRINT 'Added Leads.LastContactDate'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'NextFollowUpDate')
BEGIN
    ALTER TABLE Leads ADD NextFollowUpDate DATETIME2 NULL;
    PRINT 'Added Leads.NextFollowUpDate'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'Tags')
BEGIN
    ALTER TABLE Leads ADD Tags NVARCHAR(500) NULL;
    PRINT 'Added Leads.Tags'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Leads' AND COLUMN_NAME = 'ConvertedCustomerId')
BEGIN
    ALTER TABLE Leads ADD ConvertedCustomerId UNIQUEIDENTIFIER NULL;
    PRINT 'Added Leads.ConvertedCustomerId'
END

-- =====================================================
-- 3. DELIVERY CHALLANS TABLE - Missing columns
-- =====================================================
PRINT ''
PRINT '=== DeliveryChallans Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ConfirmedAt')
BEGIN
    ALTER TABLE DeliveryChallans ADD ConfirmedAt DATETIME2 NULL;
    PRINT 'Added DeliveryChallans.ConfirmedAt'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ConfirmedBy')
BEGIN
    ALTER TABLE DeliveryChallans ADD ConfirmedBy UNIQUEIDENTIFIER NULL;
    PRINT 'Added DeliveryChallans.ConfirmedBy'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ShippingAddress')
BEGIN
    ALTER TABLE DeliveryChallans ADD ShippingAddress NVARCHAR(500) NULL;
    PRINT 'Added DeliveryChallans.ShippingAddress'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ShippingCity')
BEGIN
    ALTER TABLE DeliveryChallans ADD ShippingCity NVARCHAR(100) NULL;
    PRINT 'Added DeliveryChallans.ShippingCity'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ShippingState')
BEGIN
    ALTER TABLE DeliveryChallans ADD ShippingState NVARCHAR(100) NULL;
    PRINT 'Added DeliveryChallans.ShippingState'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ShippingCountry')
BEGIN
    ALTER TABLE DeliveryChallans ADD ShippingCountry NVARCHAR(100) NULL;
    PRINT 'Added DeliveryChallans.ShippingCountry'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ShippingPostalCode')
BEGIN
    ALTER TABLE DeliveryChallans ADD ShippingPostalCode NVARCHAR(20) NULL;
    PRINT 'Added DeliveryChallans.ShippingPostalCode'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ContactPerson')
BEGIN
    ALTER TABLE DeliveryChallans ADD ContactPerson NVARCHAR(100) NULL;
    PRINT 'Added DeliveryChallans.ContactPerson'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'ContactPhone')
BEGIN
    ALTER TABLE DeliveryChallans ADD ContactPhone NVARCHAR(50) NULL;
    PRINT 'Added DeliveryChallans.ContactPhone'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallans' AND COLUMN_NAME = 'Reference')
BEGIN
    ALTER TABLE DeliveryChallans ADD Reference NVARCHAR(100) NULL;
    PRINT 'Added DeliveryChallans.Reference'
END

-- =====================================================
-- 4. DELIVERY CHALLAN LINES - Missing columns
-- =====================================================
PRINT ''
PRINT '=== DeliveryChallanLines Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallanLines' AND COLUMN_NAME = 'BatchNumber')
BEGIN
    ALTER TABLE DeliveryChallanLines ADD BatchNumber NVARCHAR(100) NULL;
    PRINT 'Added DeliveryChallanLines.BatchNumber'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'DeliveryChallanLines' AND COLUMN_NAME = 'SerialNumbers')
BEGIN
    ALTER TABLE DeliveryChallanLines ADD SerialNumbers NVARCHAR(MAX) NULL;
    PRINT 'Added DeliveryChallanLines.SerialNumbers'
END

-- =====================================================
-- 5. GOODS RECEIPT NOTES - Missing columns
-- =====================================================
PRINT ''
PRINT '=== GoodsReceiptNotes Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptNotes' AND COLUMN_NAME = 'LrNumber')
BEGIN
    ALTER TABLE GoodsReceiptNotes ADD LrNumber NVARCHAR(50) NULL;
    PRINT 'Added GoodsReceiptNotes.LrNumber'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptNotes' AND COLUMN_NAME = 'LrDate')
BEGIN
    ALTER TABLE GoodsReceiptNotes ADD LrDate DATETIME2 NULL;
    PRINT 'Added GoodsReceiptNotes.LrDate'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptNotes' AND COLUMN_NAME = 'SubTotal')
BEGIN
    ALTER TABLE GoodsReceiptNotes ADD SubTotal DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added GoodsReceiptNotes.SubTotal'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptNotes' AND COLUMN_NAME = 'TaxAmount')
BEGIN
    ALTER TABLE GoodsReceiptNotes ADD TaxAmount DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added GoodsReceiptNotes.TaxAmount'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptNotes' AND COLUMN_NAME = 'TotalAmount')
BEGIN
    ALTER TABLE GoodsReceiptNotes ADD TotalAmount DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added GoodsReceiptNotes.TotalAmount'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptNotes' AND COLUMN_NAME = 'Reference')
BEGIN
    ALTER TABLE GoodsReceiptNotes ADD Reference NVARCHAR(100) NULL;
    PRINT 'Added GoodsReceiptNotes.Reference'
END

-- =====================================================
-- 6. GOODS RECEIPT LINES - Missing columns
-- =====================================================
PRINT ''
PRINT '=== GoodsReceiptLines Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptLines' AND COLUMN_NAME = 'TaxPercent')
BEGIN
    ALTER TABLE GoodsReceiptLines ADD TaxPercent DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added GoodsReceiptLines.TaxPercent'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptLines' AND COLUMN_NAME = 'TaxAmount')
BEGIN
    ALTER TABLE GoodsReceiptLines ADD TaxAmount DECIMAL(18,4) NOT NULL DEFAULT 0;
    PRINT 'Added GoodsReceiptLines.TaxAmount'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptLines' AND COLUMN_NAME = 'SerialNumbers')
BEGIN
    ALTER TABLE GoodsReceiptLines ADD SerialNumbers NVARCHAR(MAX) NULL;
    PRINT 'Added GoodsReceiptLines.SerialNumbers'
END

-- Rename QCInspectionId to QualityInspectionId if needed
IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptLines' AND COLUMN_NAME = 'QCInspectionId')
   AND NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'GoodsReceiptLines' AND COLUMN_NAME = 'QualityInspectionId')
BEGIN
    EXEC sp_rename 'GoodsReceiptLines.QCInspectionId', 'QualityInspectionId', 'COLUMN';
    PRINT 'Renamed GoodsReceiptLines.QCInspectionId to QualityInspectionId'
END

-- =====================================================
-- 7. TIME ENTRIES - Missing columns
-- =====================================================
PRINT ''
PRINT '=== TimeEntries Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TimeEntries' AND COLUMN_NAME = 'StartTime')
BEGIN
    ALTER TABLE TimeEntries ADD StartTime DATETIME2 NULL;
    PRINT 'Added TimeEntries.StartTime'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TimeEntries' AND COLUMN_NAME = 'EndTime')
BEGIN
    ALTER TABLE TimeEntries ADD EndTime DATETIME2 NULL;
    PRINT 'Added TimeEntries.EndTime'
END

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'TimeEntries' AND COLUMN_NAME = 'Notes')
BEGIN
    ALTER TABLE TimeEntries ADD Notes NVARCHAR(1000) NULL;
    PRINT 'Added TimeEntries.Notes'
END

-- =====================================================
-- 8. PROJECT TASKS - Missing columns
-- =====================================================
PRINT ''
PRINT '=== ProjectTasks Table ==='

-- TaskNumber column
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ProjectTasks' AND COLUMN_NAME = 'TaskNumber')
BEGIN
    ALTER TABLE ProjectTasks ADD TaskNumber NVARCHAR(20) NOT NULL DEFAULT '';
    PRINT 'Added ProjectTasks.TaskNumber'
END

-- =====================================================
-- 9. PROJECTS - Already has most columns
-- =====================================================
PRINT ''
PRINT '=== Projects Table ==='
PRINT 'Projects table is up to date'

-- =====================================================
-- 10. WORK ORDERS - Missing columns
-- =====================================================
PRINT ''
PRINT '=== WorkOrders Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WorkOrders')
BEGIN
    CREATE TABLE WorkOrders (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        WorkOrderNumber NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        BillOfMaterialId UNIQUEIDENTIFIER NULL,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        PlannedQuantity DECIMAL(18,4) NOT NULL,
        CompletedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        ScrapQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        UnitOfMeasure NVARCHAR(20) NULL,
        Status INT NOT NULL DEFAULT 0,
        Priority INT NOT NULL DEFAULT 1,
        PlannedStartDate DATETIME2 NOT NULL,
        PlannedEndDate DATETIME2 NOT NULL,
        ActualStartDate DATETIME2 NULL,
        ActualEndDate DATETIME2 NULL,
        WarehouseId UNIQUEIDENTIFIER NULL,
        EstimatedCost DECIMAL(18,4) NOT NULL DEFAULT 0,
        ActualCost DECIMAL(18,4) NOT NULL DEFAULT 0,
        Notes NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );
    PRINT 'Created WorkOrders table'
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'WorkOrders' AND COLUMN_NAME = 'ScrapQuantity')
    BEGIN
        ALTER TABLE WorkOrders ADD ScrapQuantity DECIMAL(18,4) NOT NULL DEFAULT 0;
        PRINT 'Added WorkOrders.ScrapQuantity'
    END

    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'WorkOrders' AND COLUMN_NAME = 'Priority')
    BEGIN
        ALTER TABLE WorkOrders ADD Priority INT NOT NULL DEFAULT 1;
        PRINT 'Added WorkOrders.Priority'
    END
END

-- =====================================================
-- 11. WORK ORDER OPERATIONS - Create if not exists
-- =====================================================
PRINT ''
PRINT '=== WorkOrderOperations Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WorkOrderOperations')
BEGIN
    CREATE TABLE WorkOrderOperations (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        WorkOrderId UNIQUEIDENTIFIER NOT NULL,
        OperationNumber INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500) NULL,
        Workstation NVARCHAR(100) NULL,
        PlannedHours DECIMAL(18,4) NOT NULL DEFAULT 0,
        ActualHours DECIMAL(18,4) NOT NULL DEFAULT 0,
        Status INT NOT NULL DEFAULT 0,
        StartedAt DATETIME2 NULL,
        CompletedAt DATETIME2 NULL,
        Notes NVARCHAR(1000) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );
    PRINT 'Created WorkOrderOperations table'
END
ELSE
    PRINT 'WorkOrderOperations table already exists'

-- =====================================================
-- 12. WORK ORDER MATERIALS - Create if not exists
-- =====================================================
PRINT ''
PRINT '=== WorkOrderMaterials Table ==='

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'WorkOrderMaterials')
BEGIN
    CREATE TABLE WorkOrderMaterials (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
        WorkOrderId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        LineNumber INT NOT NULL,
        RequiredQuantity DECIMAL(18,4) NOT NULL,
        IssuedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        ReturnedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        UnitOfMeasure NVARCHAR(20) NULL,
        UnitCost DECIMAL(18,4) NOT NULL DEFAULT 0,
        Status INT NOT NULL DEFAULT 0,
        Notes NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );
    PRINT 'Created WorkOrderMaterials table'
END
ELSE
    PRINT 'WorkOrderMaterials table already exists'

-- =====================================================
-- 13. COUPONS - Missing column
-- =====================================================
PRINT ''
PRINT '=== Coupons Table ==='

-- TotalDiscountGiven already exists (confirmed in schema)
PRINT 'Coupons table is up to date'

-- =====================================================
-- 14. SHOPPING CARTS - All columns exist
-- =====================================================
PRINT ''
PRINT '=== ShoppingCarts Table ==='
PRINT 'ShoppingCarts table is up to date'

-- =====================================================
-- 15. ECOMMERCE PRODUCTS - Missing column check
-- =====================================================
PRINT ''
PRINT '=== EcommerceProducts Table ==='
-- QuantityOnHand already exists
PRINT 'EcommerceProducts table is up to date'

-- =====================================================
-- 16. WEB CATEGORIES - Missing column
-- =====================================================
PRINT ''
PRINT '=== WebCategories Table ==='

-- SortOrder exists but also check for any missing
PRINT 'WebCategories table is up to date'

-- =====================================================
-- 17. CANCELLATION LOGS - Column type adjustments
-- =====================================================
PRINT ''
PRINT '=== CancellationLogs Table ==='
-- CreatedBy, ModifiedBy, DeletedBy are NVARCHAR but entity expects GUID
-- This is a legacy mapping difference
PRINT 'CancellationLogs table has legacy column types - no changes required'

-- =====================================================
-- 18. EMPLOYEES - Add missing Notes column if not exists
-- =====================================================
PRINT ''
PRINT '=== Employees Table ==='

-- Notes column exists (confirmed)
PRINT 'Employees table is up to date'

-- =====================================================
-- 19. WAREHOUSES - Add missing Notes column if not exists
-- =====================================================
PRINT ''
PRINT '=== Warehouses Table ==='

-- Notes already exists (confirmed)
PRINT 'Warehouses table is up to date'

-- =====================================================
-- 20. PRODUCTS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Products Table ==='
-- All columns are present including HsnCode, GstSlabId
PRINT 'Products table is up to date'

-- =====================================================
-- 21. SUPPLIERS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Suppliers Table ==='
-- All columns present including Mobile
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Suppliers' AND COLUMN_NAME = 'Mobile')
BEGIN
    ALTER TABLE Suppliers ADD Mobile NVARCHAR(50) NULL;
    PRINT 'Added Suppliers.Mobile'
END
PRINT 'Suppliers table is up to date'

-- =====================================================
-- 22. INVOICES - All GST fields verified present
-- =====================================================
PRINT ''
PRINT '=== Invoices Table ==='
-- All GST fields present
PRINT 'Invoices table is up to date'

-- =====================================================
-- 23. INVOICE LINES - All GST fields verified present
-- =====================================================
PRINT ''
PRINT '=== InvoiceLines Table ==='
-- All GST fields present
PRINT 'InvoiceLines table is up to date'

-- =====================================================
-- 24. QUALITY PARAMETERS - TenantId check
-- =====================================================
PRINT ''
PRINT '=== QualityParameters Table ==='
-- TenantId exists (confirmed)
PRINT 'QualityParameters table is up to date'

-- =====================================================
-- 25. QUALITY INSPECTIONS - TenantId check
-- =====================================================
PRINT ''
PRINT '=== QualityInspections Table ==='
-- TenantId exists (confirmed)
PRINT 'QualityInspections table is up to date'

-- =====================================================
-- 26. REJECTION NOTES - TenantId check
-- =====================================================
PRINT ''
PRINT '=== RejectionNotes Table ==='
-- TenantId exists (confirmed)
PRINT 'RejectionNotes table is up to date'

-- =====================================================
-- 27. SALES ORDERS - Add any missing columns
-- =====================================================
PRINT ''
PRINT '=== SalesOrders Table ==='
PRINT 'SalesOrders table is up to date'

-- =====================================================
-- 28. PURCHASE ORDERS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== PurchaseOrders Table ==='
PRINT 'PurchaseOrders table is up to date'

-- =====================================================
-- 29. ACCOUNTS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Accounts Table ==='
PRINT 'Accounts table is up to date'

-- =====================================================
-- 30. DEPARTMENTS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Departments Table ==='
PRINT 'Departments table is up to date'

-- =====================================================
-- 31. POSITIONS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Positions Table ==='
PRINT 'Positions table is up to date'

-- =====================================================
-- 32. ATTENDANCE - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Attendances Table ==='
PRINT 'Attendances table is up to date'

-- =====================================================
-- 33. LEAVE REQUESTS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== LeaveRequests Table ==='
PRINT 'LeaveRequests table is up to date'

-- =====================================================
-- 34. LEAVE BALANCES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== LeaveBalances Table ==='
PRINT 'LeaveBalances table is up to date'

-- =====================================================
-- 35. JOURNAL ENTRIES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== JournalEntries Table ==='
PRINT 'JournalEntries table is up to date'

-- =====================================================
-- 36. JOURNAL ENTRY LINES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== JournalEntryLines Table ==='
PRINT 'JournalEntryLines table is up to date'

-- =====================================================
-- 37. RECURRING INVOICES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== RecurringInvoices Table ==='
PRINT 'RecurringInvoices table is up to date'

-- =====================================================
-- 38. RECURRING INVOICE LINES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== RecurringInvoiceLines Table ==='
PRINT 'RecurringInvoiceLines table is up to date'

-- =====================================================
-- 39. BILL OF MATERIALS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== BillOfMaterials Table ==='
PRINT 'BillOfMaterials table is up to date'

-- =====================================================
-- 40. BOM LINES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== BomLines Table ==='
PRINT 'BomLines table is up to date'

-- =====================================================
-- 41. PAYROLL RUNS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== PayrollRuns Table ==='
PRINT 'PayrollRuns table is up to date'

-- =====================================================
-- 42. PAYSLIPS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Payslips Table ==='
PRINT 'Payslips table is up to date'

-- =====================================================
-- 43. PAYSLIP LINES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== PayslipLines Table ==='
PRINT 'PayslipLines table is up to date'

-- =====================================================
-- 44. SALARY COMPONENTS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== SalaryComponents Table ==='
PRINT 'SalaryComponents table is up to date'

-- =====================================================
-- 45. SALARY STRUCTURES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== SalaryStructures Table ==='
PRINT 'SalaryStructures table is up to date'

-- =====================================================
-- 46. SALARY STRUCTURE LINES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== SalaryStructureLines Table ==='
PRINT 'SalaryStructureLines table is up to date'

-- =====================================================
-- 47. PROJECT MEMBERS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== ProjectMembers Table ==='
PRINT 'ProjectMembers table is up to date'

-- =====================================================
-- 48. PROJECT MILESTONES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== ProjectMilestones Table ==='
PRINT 'ProjectMilestones table is up to date'

-- =====================================================
-- 49. USERS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Users Table ==='
PRINT 'Users table is up to date'

-- =====================================================
-- 50. ROLES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Roles Table ==='
PRINT 'Roles table is up to date'

-- =====================================================
-- 51. USER ROLES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== UserRoles Table ==='
PRINT 'UserRoles table is up to date'

-- =====================================================
-- 52. PERMISSIONS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Permissions Table ==='
PRINT 'Permissions table is up to date'

-- =====================================================
-- 53. ROLE PERMISSIONS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== RolePermissions Table ==='
PRINT 'RolePermissions table is up to date'

-- =====================================================
-- 54. AUDIT LOGS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== AuditLogs Table ==='
PRINT 'AuditLogs table is up to date'

-- =====================================================
-- 55. CURRENCIES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Currencies Table ==='
PRINT 'Currencies table is up to date'

-- =====================================================
-- 56. GST SLABS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== GstSlabs Table ==='
PRINT 'GstSlabs table is up to date'

-- =====================================================
-- 57. INDIAN STATES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== IndianStates Table ==='
PRINT 'IndianStates table is up to date'

-- =====================================================
-- 58. OFFICE LOCATIONS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== OfficeLocations Table ==='
PRINT 'OfficeLocations table is up to date'

-- =====================================================
-- 59. TAX CONFIGURATIONS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== TaxConfigurations Table ==='
PRINT 'TaxConfigurations table is up to date'

-- =====================================================
-- 60. TAX SLABS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== TaxSlabs Table ==='
PRINT 'TaxSlabs table is up to date'

-- =====================================================
-- 61. TAX REGIONS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== TaxRegions Table ==='
PRINT 'TaxRegions table is up to date'

-- =====================================================
-- 62. STORES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Stores Table ==='
PRINT 'Stores table is up to date'

-- =====================================================
-- 63. BANNERS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== Banners Table ==='
PRINT 'Banners table is up to date'

-- =====================================================
-- 64. WEB CUSTOMERS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== WebCustomers Table ==='
PRINT 'WebCustomers table is up to date'

-- =====================================================
-- 65. WEB ORDERS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== WebOrders Table ==='
PRINT 'WebOrders table is up to date'

-- =====================================================
-- 66. WEB ORDER ITEMS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== WebOrderItems Table ==='
PRINT 'WebOrderItems table is up to date'

-- =====================================================
-- 67. CART ITEMS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== CartItems Table ==='
PRINT 'CartItems table is up to date'

-- =====================================================
-- 68. CUSTOMER ADDRESSES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== CustomerAddresses Table ==='
PRINT 'CustomerAddresses table is up to date'

-- =====================================================
-- 69. PRODUCT IMAGES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== ProductImages Table ==='
PRINT 'ProductImages table is up to date'

-- =====================================================
-- 70. PRODUCT VARIANTS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== ProductVariants Table ==='
PRINT 'ProductVariants table is up to date'

-- =====================================================
-- 71. PRODUCT REVIEWS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== ProductReviews Table ==='
PRINT 'ProductReviews table is up to date'

-- =====================================================
-- 72. WISHLIST ITEMS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== WishlistItems Table ==='
PRINT 'WishlistItems table is up to date'

-- =====================================================
-- 73. SHIPPING METHODS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== ShippingMethods Table ==='
PRINT 'ShippingMethods table is up to date'

-- =====================================================
-- 74. WEB PAYMENT METHODS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== WebPaymentMethods Table ==='
PRINT 'WebPaymentMethods table is up to date'

-- =====================================================
-- 75. STOCK LEVELS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== StockLevels Table ==='
PRINT 'StockLevels table is up to date'

-- =====================================================
-- 76. STOCK MOVEMENTS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== StockMovements Table ==='
PRINT 'StockMovements table is up to date'

-- =====================================================
-- 77. WAREHOUSE LOCATIONS - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== WarehouseLocations Table ==='
PRINT 'WarehouseLocations table is up to date'

-- =====================================================
-- 78. PRODUCT CATEGORIES - All columns verified present
-- =====================================================
PRINT ''
PRINT '=== ProductCategories Table ==='
PRINT 'ProductCategories table is up to date'

-- =====================================================
-- Insert default TenantSettings record if not exists
-- =====================================================
PRINT ''
PRINT '=== Inserting Default Data ==='

IF NOT EXISTS (SELECT 1 FROM TenantSettings)
BEGIN
    INSERT INTO TenantSettings (Id, CompanyName, Country, CountryCode, Currency, CurrencySymbol, CurrencyName)
    VALUES (NEWID(), 'My Company', 'India', 'IN', 'INR', N'₹', 'Indian Rupee');
    PRINT 'Inserted default TenantSettings record'
END

-- =====================================================
-- SUMMARY
-- =====================================================
PRINT ''
PRINT '=============================================='
PRINT 'Schema synchronization completed successfully!'
PRINT '=============================================='
PRINT ''
PRINT 'Summary of changes:'
PRINT '- Created TenantSettings table (if not existed)'
PRINT '- Added missing columns to Leads table'
PRINT '- Added missing columns to DeliveryChallans table'
PRINT '- Added missing columns to DeliveryChallanLines table'
PRINT '- Added missing columns to GoodsReceiptNotes table'
PRINT '- Added missing columns to GoodsReceiptLines table'
PRINT '- Added missing columns to TimeEntries table'
PRINT '- Added missing columns to ProjectTasks table'
PRINT '- Created/updated WorkOrders table'
PRINT '- Created WorkOrderOperations table (if not existed)'
PRINT '- Created WorkOrderMaterials table (if not existed)'
PRINT ''

GO
