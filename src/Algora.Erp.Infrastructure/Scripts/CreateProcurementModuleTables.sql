-- =============================================================================
-- Procurement & Quality Module Tables Migration Script
-- Algora ERP - Enterprise Resource Planning
-- =============================================================================
-- NOTE: This system uses database-per-tenant multi-tenancy. Each tenant has
--       its own database, so TenantId columns are NOT needed in tables.
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
GO

-- =============================================================================
-- DELIVERY CHALLAN TABLES (Dispatch Module)
-- =============================================================================

-- DeliveryChallans - Outbound dispatch documents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeliveryChallans')
BEGIN
    CREATE TABLE DeliveryChallans (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        ChallanNumber NVARCHAR(50) NOT NULL,
        ChallanDate DATETIME2 NOT NULL,

        -- Source Document
        SalesOrderId UNIQUEIDENTIFIER NULL,

        -- Customer
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        CustomerName NVARCHAR(200) NULL,

        -- Warehouse
        WarehouseId UNIQUEIDENTIFIER NOT NULL,

        -- Status
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Confirmed, 2=Dispatched, 3=Delivered, 9=Cancelled

        -- Transport Details
        VehicleNumber NVARCHAR(50) NULL,
        DriverName NVARCHAR(100) NULL,
        DriverPhone NVARCHAR(20) NULL,
        TransportMode NVARCHAR(50) NULL,
        TransporterName NVARCHAR(200) NULL,

        -- Confirmation Details
        ConfirmedAt DATETIME2 NULL,
        ConfirmedBy UNIQUEIDENTIFIER NULL,

        -- Dispatch Details
        DispatchedBy UNIQUEIDENTIFIER NULL,
        DispatchedAt DATETIME2 NULL,

        -- Delivery Details
        DeliveredAt DATETIME2 NULL,
        DeliveredBy UNIQUEIDENTIFIER NULL,

        -- Shipping Address (ShipTo)
        ShipToName NVARCHAR(200) NULL,
        ShipToPhone NVARCHAR(20) NULL,
        ShipToAddress1 NVARCHAR(500) NULL,
        ShipToAddress2 NVARCHAR(500) NULL,
        ShipToCity NVARCHAR(100) NULL,
        ShipToState NVARCHAR(100) NULL,
        ShipToPostalCode NVARCHAR(20) NULL,
        ShipToCountry NVARCHAR(100) NULL,

        -- Legacy shipping address (backward compatibility)
        ShippingAddress NVARCHAR(500) NULL,
        ShippingCity NVARCHAR(100) NULL,
        ShippingState NVARCHAR(100) NULL,
        ShippingCountry NVARCHAR(100) NULL,
        ShippingPostalCode NVARCHAR(20) NULL,
        ContactPerson NVARCHAR(100) NULL,
        ContactPhone NVARCHAR(20) NULL,

        -- Totals
        TotalQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalPackages INT NULL DEFAULT 1,
        TotalWeight DECIMAL(18,4) NULL,
        WeightUnit NVARCHAR(10) NULL DEFAULT 'KG',

        Reference NVARCHAR(200) NULL,
        Notes NVARCHAR(MAX) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    PRINT 'Created table: DeliveryChallans';
END
GO

-- DeliveryChallanLines - Line items for delivery challan
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeliveryChallanLines')
BEGIN
    CREATE TABLE DeliveryChallanLines (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        DeliveryChallanId UNIQUEIDENTIFIER NOT NULL,
        LineNumber INT NOT NULL,

        -- Product
        ProductId UNIQUEIDENTIFIER NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        ProductSku NVARCHAR(50) NOT NULL,

        -- Sales Order Line Reference
        SalesOrderLineId UNIQUEIDENTIFIER NULL,

        -- Quantities
        Quantity DECIMAL(18,4) NOT NULL,
        UnitOfMeasure NVARCHAR(20) NOT NULL DEFAULT 'EA',

        -- Batch/Serial
        BatchNumber NVARCHAR(100) NULL,
        SerialNumbers NVARCHAR(MAX) NULL,

        Notes NVARCHAR(500) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_DeliveryChallanLines_DeliveryChallans
            FOREIGN KEY (DeliveryChallanId) REFERENCES DeliveryChallans(Id)
    );

    PRINT 'Created table: DeliveryChallanLines';
END
GO

-- =============================================================================
-- GOODS RECEIPT TABLES (Procurement Module)
-- =============================================================================

-- GoodsReceiptNotes - Inbound goods receipt documents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoodsReceiptNotes')
BEGIN
    CREATE TABLE GoodsReceiptNotes (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        GrnNumber NVARCHAR(50) NOT NULL,
        GrnDate DATETIME2 NOT NULL,

        -- Source Document
        PurchaseOrderId UNIQUEIDENTIFIER NULL,
        PurchaseOrderNumber NVARCHAR(50) NULL,

        -- Supplier
        SupplierId UNIQUEIDENTIFIER NOT NULL,
        SupplierName NVARCHAR(200) NULL,
        SupplierInvoiceNumber NVARCHAR(100) NULL,
        SupplierInvoiceDate DATETIME2 NULL,

        -- Warehouse
        WarehouseId UNIQUEIDENTIFIER NOT NULL,

        -- Status
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Pending, 2=QCPending, 3=QCCompleted, 4=Accepted, 5=PartiallyAccepted, 6=Rejected, 9=Cancelled

        -- Transport Details
        VehicleNumber NVARCHAR(50) NULL,
        DriverName NVARCHAR(100) NULL,
        TransporterName NVARCHAR(200) NULL,
        WayBillNumber NVARCHAR(100) NULL,
        LrNumber NVARCHAR(100) NULL, -- Lorry Receipt Number
        LrDate DATETIME2 NULL,

        -- Receipt Details
        ReceivedBy UNIQUEIDENTIFIER NULL,
        ReceivedAt DATETIME2 NULL,

        -- Quantity Totals
        TotalOrderedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalReceivedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalAcceptedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalRejectedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- Value Totals
        SubTotal DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        TotalValue DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- QC
        QCRequired BIT NOT NULL DEFAULT 0,
        QCCompletedAt DATETIME2 NULL,

        Reference NVARCHAR(200) NULL,
        Notes NVARCHAR(MAX) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    PRINT 'Created table: GoodsReceiptNotes';
END
GO

-- GoodsReceiptLines - Line items for goods receipt
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoodsReceiptLines')
BEGIN
    CREATE TABLE GoodsReceiptLines (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        GoodsReceiptNoteId UNIQUEIDENTIFIER NOT NULL,
        LineNumber INT NOT NULL,

        -- Purchase Order Line Reference
        PurchaseOrderLineId UNIQUEIDENTIFIER NULL,

        -- Product
        ProductId UNIQUEIDENTIFIER NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        ProductSku NVARCHAR(50) NOT NULL,

        -- Quantities
        OrderedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        ReceivedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        AcceptedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        RejectedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        UnitOfMeasure NVARCHAR(20) NOT NULL DEFAULT 'EA',

        -- Pricing
        UnitPrice DECIMAL(18,4) NOT NULL DEFAULT 0,
        TaxPercent DECIMAL(5,2) NOT NULL DEFAULT 0,
        TaxAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
        LineTotal DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- QC Status
        QCStatus INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Passed, 2=Failed, 3=PartialPass, 4=NotRequired
        QualityInspectionId UNIQUEIDENTIFIER NULL,

        -- Batch/Serial Tracking
        BatchNumber NVARCHAR(100) NULL,
        ExpiryDate DATETIME2 NULL,
        SerialNumbers NVARCHAR(MAX) NULL,

        Notes NVARCHAR(500) NULL,
        RejectionReason NVARCHAR(500) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_GoodsReceiptLines_GoodsReceiptNotes
            FOREIGN KEY (GoodsReceiptNoteId) REFERENCES GoodsReceiptNotes(Id)
    );

    PRINT 'Created table: GoodsReceiptLines';
END
GO

-- =============================================================================
-- QUALITY INSPECTION TABLES (Quality Module)
-- =============================================================================

-- QualityInspections - Quality check documents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QualityInspections')
BEGIN
    CREATE TABLE QualityInspections (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        InspectionNumber NVARCHAR(50) NOT NULL,
        InspectionDate DATETIME2 NOT NULL,

        -- Inspection Type: 0=Incoming, 1=Outgoing, 2=InProcess, 3=Random, 4=Final
        InspectionType INT NOT NULL DEFAULT 0,

        -- Source Document
        SourceDocumentType NVARCHAR(50) NOT NULL,
        SourceDocumentId UNIQUEIDENTIFIER NOT NULL,
        SourceDocumentNumber NVARCHAR(50) NULL,

        -- Product
        ProductId UNIQUEIDENTIFIER NOT NULL,

        -- Warehouse
        WarehouseId UNIQUEIDENTIFIER NOT NULL,

        -- Supplier (for incoming inspections)
        SupplierId UNIQUEIDENTIFIER NULL,

        -- Status: 0=Pending, 1=InProgress, 2=Completed, 3=Approved, 4=Rejected, 9=Cancelled
        Status INT NOT NULL DEFAULT 0,

        -- Quantities
        TotalQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        SampleSize DECIMAL(18,4) NOT NULL DEFAULT 0,
        InspectedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        PassedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        FailedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- Inspector
        InspectedBy UNIQUEIDENTIFIER NULL,
        InspectedAt DATETIME2 NULL,
        InspectorName NVARCHAR(100) NULL,

        -- Approval
        ApprovedBy UNIQUEIDENTIFIER NULL,
        ApprovedAt DATETIME2 NULL,
        ApproverName NVARCHAR(100) NULL,

        -- Result: 0=Pending, 1=Pass, 2=Fail, 3=PartialPass, 4=ConditionalPass
        OverallResult INT NOT NULL DEFAULT 0,
        ResultRemarks NVARCHAR(500) NULL,

        Notes NVARCHAR(MAX) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    PRINT 'Created table: QualityInspections';
END
GO

-- QualityParameters - Parameters for quality inspection
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QualityParameters')
BEGIN
    CREATE TABLE QualityParameters (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        QualityInspectionId UNIQUEIDENTIFIER NOT NULL,
        SequenceNumber INT NOT NULL,

        -- Parameter Details
        ParameterName NVARCHAR(100) NOT NULL,
        ParameterCode NVARCHAR(50) NULL,

        -- Expected Values
        ExpectedValue NVARCHAR(500) NULL,
        MinValue DECIMAL(18,6) NULL,
        MaxValue DECIMAL(18,6) NULL,
        Tolerance DECIMAL(18,6) NULL,
        Unit NVARCHAR(20) NULL,

        -- Actual Values
        ActualValue NVARCHAR(500) NULL,
        MeasuredValue DECIMAL(18,6) NULL,

        -- Result: 0=Pending, 1=Pass, 2=Fail, 3=Warning, 4=NotApplicable
        Result INT NOT NULL DEFAULT 0,
        Remarks NVARCHAR(500) NULL,

        -- Evidence
        AttachmentUrl NVARCHAR(1000) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_QualityParameters_QualityInspections
            FOREIGN KEY (QualityInspectionId) REFERENCES QualityInspections(Id)
    );

    PRINT 'Created table: QualityParameters';
END
GO

-- =============================================================================
-- REJECTION NOTE TABLES (Quality Module)
-- =============================================================================

-- RejectionNotes - Rejection documents
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RejectionNotes')
BEGIN
    CREATE TABLE RejectionNotes (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        RejectionNumber NVARCHAR(50) NOT NULL,
        RejectionDate DATETIME2 NOT NULL,

        -- Source Document
        SourceDocumentType NVARCHAR(50) NOT NULL,
        SourceDocumentId UNIQUEIDENTIFIER NOT NULL,
        SourceDocumentNumber NVARCHAR(50) NULL,

        -- Quality Inspection Reference
        QualityInspectionId UNIQUEIDENTIFIER NULL,

        -- Product
        ProductId UNIQUEIDENTIFIER NOT NULL,
        ProductName NVARCHAR(200) NOT NULL,
        ProductSku NVARCHAR(50) NOT NULL,

        -- Warehouse
        WarehouseId UNIQUEIDENTIFIER NOT NULL,

        -- Supplier
        SupplierId UNIQUEIDENTIFIER NULL,

        -- Quantities
        RejectedQuantity DECIMAL(18,4) NOT NULL,
        UnitOfMeasure NVARCHAR(20) NOT NULL DEFAULT 'EA',
        UnitCost DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalValue DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- Rejection Details
        RejectionReason NVARCHAR(500) NOT NULL,
        -- Category: 0=QualityDefect, 1=DamagedInTransit, 2=WrongProduct, 3=QuantityMismatch,
        --           4=ExpiredProduct, 5=PackagingDefect, 6=DocumentationError, 7=ContaminationIssue,
        --           8=SpecificationMismatch, 99=Other
        RejectionCategory INT NOT NULL DEFAULT 0,
        DefectDescription NVARCHAR(MAX) NULL,

        -- Disposition: 0=Pending, 1=ReturnToSupplier, 2=Scrapped, 3=Reworked, 4=Disposed,
        --              5=AcceptedWithDeviation, 6=HoldForReview
        DispositionStatus INT NOT NULL DEFAULT 0,
        DispositionAction NVARCHAR(500) NULL,
        DispositionDate DATETIME2 NULL,
        DisposedBy UNIQUEIDENTIFIER NULL,
        DisposerName NVARCHAR(100) NULL,

        -- Supplier Return (if applicable)
        DebitNoteId UNIQUEIDENTIFIER NULL,
        DebitNoteNumber NVARCHAR(50) NULL,
        ReturnDate DATETIME2 NULL,
        ReturnReference NVARCHAR(100) NULL,

        -- Scrap (if applicable)
        ScrapDate DATETIME2 NULL,
        ScrapReference NVARCHAR(100) NULL,
        ScrapValue DECIMAL(18,4) NULL,

        -- Rework (if applicable)
        ReworkInstructions NVARCHAR(MAX) NULL,
        ReworkCompletedAt DATETIME2 NULL,
        ReworkCost DECIMAL(18,4) NULL,

        Notes NVARCHAR(MAX) NULL,
        AttachmentUrls NVARCHAR(MAX) NULL, -- JSON array

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    PRINT 'Created table: RejectionNotes';
END
GO

-- =============================================================================
-- CANCELLATION LOG TABLE (Common Module)
-- =============================================================================

-- CancellationLogs - Audit trail for document cancellations
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'CancellationLogs')
BEGIN
    CREATE TABLE CancellationLogs (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        -- Document Reference
        DocumentType NVARCHAR(50) NOT NULL,
        DocumentId UNIQUEIDENTIFIER NOT NULL,
        DocumentNumber NVARCHAR(50) NOT NULL,

        -- Cancellation Details
        CancelledAt DATETIME2 NOT NULL,
        CancelledBy UNIQUEIDENTIFIER NOT NULL,
        CancelledByName NVARCHAR(100) NULL,

        -- Reason
        CancellationReason NVARCHAR(500) NOT NULL,
        -- Category: 0=CustomerRequest, 1=DuplicateEntry, 2=DataEntryError, 3=SupplierIssue,
        --           4=QualityIssue, 5=PriceDispute, 6=DeliveryIssue, 7=PaymentIssue,
        --           8=OutOfStock, 9=SystemError, 99=Other
        ReasonCategory INT NOT NULL DEFAULT 0,

        -- Original Document State (JSON)
        OriginalDocumentState NVARCHAR(MAX) NULL,

        -- Reversal Details
        StockReversed BIT NOT NULL DEFAULT 0,
        StockReversalDetails NVARCHAR(MAX) NULL, -- JSON

        FinancialReversed BIT NOT NULL DEFAULT 0,
        FinancialReversalDetails NVARCHAR(MAX) NULL, -- JSON

        RelatedDocuments NVARCHAR(MAX) NULL, -- JSON array of affected documents

        Notes NVARCHAR(MAX) NULL,

        -- Approval
        ApprovalRequired BIT NOT NULL DEFAULT 0,
        ApprovalReference NVARCHAR(100) NULL,
        ApprovedBy UNIQUEIDENTIFIER NULL,
        ApprovedAt DATETIME2 NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    PRINT 'Created table: CancellationLogs';
END
GO

-- =============================================================================
-- UPDATE EXISTING TABLES (Add missing columns)
-- =============================================================================

-- Add SourceDocumentNumber to StockMovements if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('StockMovements') AND name = 'SourceDocumentNumber')
BEGIN
    ALTER TABLE StockMovements ADD SourceDocumentNumber NVARCHAR(100) NULL;
    PRINT 'Added SourceDocumentNumber column to StockMovements';
END
GO

-- =============================================================================
-- INDEXES (without TenantId since we use database-per-tenant)
-- =============================================================================

-- DeliveryChallans indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_ChallanNumber')
    CREATE UNIQUE INDEX IX_DeliveryChallans_ChallanNumber
    ON DeliveryChallans(ChallanNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_ChallanDate')
    CREATE INDEX IX_DeliveryChallans_ChallanDate
    ON DeliveryChallans(ChallanDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_Status')
    CREATE INDEX IX_DeliveryChallans_Status
    ON DeliveryChallans(Status) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_CustomerId')
    CREATE INDEX IX_DeliveryChallans_CustomerId
    ON DeliveryChallans(CustomerId) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_SalesOrderId')
    CREATE INDEX IX_DeliveryChallans_SalesOrderId
    ON DeliveryChallans(SalesOrderId) WHERE SalesOrderId IS NOT NULL;

-- DeliveryChallanLines indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallanLines_DeliveryChallanId')
    CREATE INDEX IX_DeliveryChallanLines_DeliveryChallanId
    ON DeliveryChallanLines(DeliveryChallanId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallanLines_ProductId')
    CREATE INDEX IX_DeliveryChallanLines_ProductId
    ON DeliveryChallanLines(ProductId);

-- GoodsReceiptNotes indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_GrnNumber')
    CREATE UNIQUE INDEX IX_GoodsReceiptNotes_GrnNumber
    ON GoodsReceiptNotes(GrnNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_GrnDate')
    CREATE INDEX IX_GoodsReceiptNotes_GrnDate
    ON GoodsReceiptNotes(GrnDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_Status')
    CREATE INDEX IX_GoodsReceiptNotes_Status
    ON GoodsReceiptNotes(Status) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_SupplierId')
    CREATE INDEX IX_GoodsReceiptNotes_SupplierId
    ON GoodsReceiptNotes(SupplierId) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_PurchaseOrderId')
    CREATE INDEX IX_GoodsReceiptNotes_PurchaseOrderId
    ON GoodsReceiptNotes(PurchaseOrderId) WHERE PurchaseOrderId IS NOT NULL;

-- GoodsReceiptLines indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptLines_GoodsReceiptNoteId')
    CREATE INDEX IX_GoodsReceiptLines_GoodsReceiptNoteId
    ON GoodsReceiptLines(GoodsReceiptNoteId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptLines_ProductId')
    CREATE INDEX IX_GoodsReceiptLines_ProductId
    ON GoodsReceiptLines(ProductId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptLines_PurchaseOrderLineId')
    CREATE INDEX IX_GoodsReceiptLines_PurchaseOrderLineId
    ON GoodsReceiptLines(PurchaseOrderLineId) WHERE PurchaseOrderLineId IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptLines_QCStatus')
    CREATE INDEX IX_GoodsReceiptLines_QCStatus
    ON GoodsReceiptLines(QCStatus);

-- QualityInspections indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_InspectionNumber')
    CREATE UNIQUE INDEX IX_QualityInspections_InspectionNumber
    ON QualityInspections(InspectionNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_InspectionDate')
    CREATE INDEX IX_QualityInspections_InspectionDate
    ON QualityInspections(InspectionDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_Status')
    CREATE INDEX IX_QualityInspections_Status
    ON QualityInspections(Status) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_SourceDocument')
    CREATE INDEX IX_QualityInspections_SourceDocument
    ON QualityInspections(SourceDocumentType, SourceDocumentId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_ProductId')
    CREATE INDEX IX_QualityInspections_ProductId
    ON QualityInspections(ProductId);

-- QualityParameters indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityParameters_QualityInspectionId')
    CREATE INDEX IX_QualityParameters_QualityInspectionId
    ON QualityParameters(QualityInspectionId);

-- RejectionNotes indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_RejectionNumber')
    CREATE UNIQUE INDEX IX_RejectionNotes_RejectionNumber
    ON RejectionNotes(RejectionNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_RejectionDate')
    CREATE INDEX IX_RejectionNotes_RejectionDate
    ON RejectionNotes(RejectionDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_DispositionStatus')
    CREATE INDEX IX_RejectionNotes_DispositionStatus
    ON RejectionNotes(DispositionStatus) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_SourceDocument')
    CREATE INDEX IX_RejectionNotes_SourceDocument
    ON RejectionNotes(SourceDocumentType, SourceDocumentId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_QualityInspectionId')
    CREATE INDEX IX_RejectionNotes_QualityInspectionId
    ON RejectionNotes(QualityInspectionId) WHERE QualityInspectionId IS NOT NULL;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_ProductId')
    CREATE INDEX IX_RejectionNotes_ProductId
    ON RejectionNotes(ProductId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_SupplierId')
    CREATE INDEX IX_RejectionNotes_SupplierId
    ON RejectionNotes(SupplierId) WHERE SupplierId IS NOT NULL;

-- CancellationLogs indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CancellationLogs_DocumentType')
    CREATE INDEX IX_CancellationLogs_DocumentType
    ON CancellationLogs(DocumentType);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CancellationLogs_CancelledAt')
    CREATE INDEX IX_CancellationLogs_CancelledAt
    ON CancellationLogs(CancelledAt);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CancellationLogs_DocumentId')
    CREATE INDEX IX_CancellationLogs_DocumentId
    ON CancellationLogs(DocumentId);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CancellationLogs_DocumentType_DocumentNumber')
    CREATE INDEX IX_CancellationLogs_DocumentType_DocumentNumber
    ON CancellationLogs(DocumentType, DocumentNumber);

PRINT 'All indexes created successfully';
GO

-- =============================================================================
-- SUMMARY
-- =============================================================================
PRINT '';
PRINT '=============================================================================';
PRINT 'Procurement & Quality Module Tables Migration Complete';
PRINT '=============================================================================';
PRINT 'Tables Created:';
PRINT '  - DeliveryChallans (Dispatch)';
PRINT '  - DeliveryChallanLines (Dispatch)';
PRINT '  - GoodsReceiptNotes (Procurement)';
PRINT '  - GoodsReceiptLines (Procurement)';
PRINT '  - QualityInspections (Quality)';
PRINT '  - QualityParameters (Quality)';
PRINT '  - RejectionNotes (Quality)';
PRINT '  - CancellationLogs (Common)';
PRINT '';
PRINT 'Existing Tables Updated:';
PRINT '  - StockMovements (added SourceDocumentNumber)';
PRINT '';
PRINT 'NOTE: This script uses database-per-tenant multi-tenancy.';
PRINT '      TenantId columns are NOT included in tables.';
PRINT '=============================================================================';
GO
