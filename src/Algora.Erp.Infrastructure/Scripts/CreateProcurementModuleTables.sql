-- =============================================================================
-- Procurement & Quality Module Tables Migration Script
-- Algora ERP - Enterprise Resource Planning
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
        TenantId UNIQUEIDENTIFIER NOT NULL,

        ChallanNumber NVARCHAR(50) NOT NULL,
        ChallanDate DATETIME2 NOT NULL,

        -- Source Document
        SalesOrderId UNIQUEIDENTIFIER NULL,

        -- Customer
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        CustomerName NVARCHAR(200) NOT NULL,

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

        -- Dispatch Details
        DispatchedBy UNIQUEIDENTIFIER NULL,
        DispatchedAt DATETIME2 NULL,
        DispatcherName NVARCHAR(100) NULL,

        -- Delivery Details
        DeliveredAt DATETIME2 NULL,
        ReceiverName NVARCHAR(100) NULL,
        ReceiverSignature NVARCHAR(MAX) NULL,

        -- Shipping Address
        ShipToName NVARCHAR(200) NULL,
        ShipToAddress1 NVARCHAR(500) NULL,
        ShipToAddress2 NVARCHAR(500) NULL,
        ShipToCity NVARCHAR(100) NULL,
        ShipToState NVARCHAR(100) NULL,
        ShipToPostalCode NVARCHAR(20) NULL,
        ShipToCountry NVARCHAR(100) NULL,
        ShipToPhone NVARCHAR(20) NULL,

        -- Totals
        TotalQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalPackages INT NOT NULL DEFAULT 0,
        TotalWeight DECIMAL(18,4) NULL,
        WeightUnit NVARCHAR(10) NULL DEFAULT 'KG',

        Notes NVARCHAR(MAX) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL
    );

    PRINT 'Created table: DeliveryChallans';
END
GO

-- DeliveryChallanLines - Line items for delivery challan
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DeliveryChallanLines')
BEGIN
    CREATE TABLE DeliveryChallanLines (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,

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

        -- Packaging
        PackageCount INT NULL DEFAULT 1,
        PackageType NVARCHAR(50) NULL,

        Notes NVARCHAR(500) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,

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
        TenantId UNIQUEIDENTIFIER NOT NULL,

        GrnNumber NVARCHAR(50) NOT NULL,
        GrnDate DATETIME2 NOT NULL,

        -- Source Document
        PurchaseOrderId UNIQUEIDENTIFIER NULL,
        PurchaseOrderNumber NVARCHAR(50) NULL,

        -- Supplier
        SupplierId UNIQUEIDENTIFIER NOT NULL,
        SupplierName NVARCHAR(200) NOT NULL,
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

        -- Receipt Details
        ReceivedBy UNIQUEIDENTIFIER NULL,
        ReceivedAt DATETIME2 NULL,
        ReceiverName NVARCHAR(100) NULL,

        -- Totals
        TotalOrderedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalReceivedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalAcceptedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalRejectedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0,
        TotalValue DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- QC
        QCRequired BIT NOT NULL DEFAULT 0,
        QCCompletedAt DATETIME2 NULL,
        QCCompletedBy UNIQUEIDENTIFIER NULL,

        Notes NVARCHAR(MAX) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL
    );

    PRINT 'Created table: GoodsReceiptNotes';
END
GO

-- GoodsReceiptLines - Line items for goods receipt
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GoodsReceiptLines')
BEGIN
    CREATE TABLE GoodsReceiptLines (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,

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
        LineTotal DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- QC Status
        QCStatus INT NOT NULL DEFAULT 0, -- 0=Pending, 1=Passed, 2=Failed, 3=PartialPass
        QCInspectionId UNIQUEIDENTIFIER NULL,

        -- Storage Location
        WarehouseLocationId UNIQUEIDENTIFIER NULL,
        BatchNumber NVARCHAR(100) NULL,
        ExpiryDate DATETIME2 NULL,

        Notes NVARCHAR(500) NULL,
        RejectionReason NVARCHAR(500) NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,

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
        TenantId UNIQUEIDENTIFIER NOT NULL,

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
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL
    );

    PRINT 'Created table: QualityInspections';
END
GO

-- QualityParameters - Parameters for quality inspection
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'QualityParameters')
BEGIN
    CREATE TABLE QualityParameters (
        Id UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        TenantId UNIQUEIDENTIFIER NOT NULL,

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
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL,

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
        TenantId UNIQUEIDENTIFIER NOT NULL,

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
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL
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
        TenantId UNIQUEIDENTIFIER NOT NULL,

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
        ApprovalReference NVARCHAR(100) NULL,
        ApprovedBy UNIQUEIDENTIFIER NULL,
        ApprovedAt DATETIME2 NULL,

        -- Audit
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy NVARCHAR(100) NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL
    );

    PRINT 'Created table: CancellationLogs';
END
GO

-- =============================================================================
-- INDEXES
-- =============================================================================

-- DeliveryChallans indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_TenantId_ChallanNumber')
    CREATE UNIQUE INDEX IX_DeliveryChallans_TenantId_ChallanNumber
    ON DeliveryChallans(TenantId, ChallanNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_TenantId_ChallanDate')
    CREATE INDEX IX_DeliveryChallans_TenantId_ChallanDate
    ON DeliveryChallans(TenantId, ChallanDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_TenantId_Status')
    CREATE INDEX IX_DeliveryChallans_TenantId_Status
    ON DeliveryChallans(TenantId, Status) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DeliveryChallans_TenantId_CustomerId')
    CREATE INDEX IX_DeliveryChallans_TenantId_CustomerId
    ON DeliveryChallans(TenantId, CustomerId) WHERE IsDeleted = 0;

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
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_TenantId_GrnNumber')
    CREATE UNIQUE INDEX IX_GoodsReceiptNotes_TenantId_GrnNumber
    ON GoodsReceiptNotes(TenantId, GrnNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_TenantId_GrnDate')
    CREATE INDEX IX_GoodsReceiptNotes_TenantId_GrnDate
    ON GoodsReceiptNotes(TenantId, GrnDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_TenantId_Status')
    CREATE INDEX IX_GoodsReceiptNotes_TenantId_Status
    ON GoodsReceiptNotes(TenantId, Status) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptNotes_TenantId_SupplierId')
    CREATE INDEX IX_GoodsReceiptNotes_TenantId_SupplierId
    ON GoodsReceiptNotes(TenantId, SupplierId) WHERE IsDeleted = 0;

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

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_GoodsReceiptLines_QCStatus')
    CREATE INDEX IX_GoodsReceiptLines_QCStatus
    ON GoodsReceiptLines(QCStatus);

-- QualityInspections indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_TenantId_InspectionNumber')
    CREATE UNIQUE INDEX IX_QualityInspections_TenantId_InspectionNumber
    ON QualityInspections(TenantId, InspectionNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_TenantId_InspectionDate')
    CREATE INDEX IX_QualityInspections_TenantId_InspectionDate
    ON QualityInspections(TenantId, InspectionDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_QualityInspections_TenantId_Status')
    CREATE INDEX IX_QualityInspections_TenantId_Status
    ON QualityInspections(TenantId, Status) WHERE IsDeleted = 0;

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
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_TenantId_RejectionNumber')
    CREATE UNIQUE INDEX IX_RejectionNotes_TenantId_RejectionNumber
    ON RejectionNotes(TenantId, RejectionNumber) WHERE IsDeleted = 0;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_TenantId_RejectionDate')
    CREATE INDEX IX_RejectionNotes_TenantId_RejectionDate
    ON RejectionNotes(TenantId, RejectionDate);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_RejectionNotes_TenantId_DispositionStatus')
    CREATE INDEX IX_RejectionNotes_TenantId_DispositionStatus
    ON RejectionNotes(TenantId, DispositionStatus) WHERE IsDeleted = 0;

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
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CancellationLogs_TenantId_DocumentType')
    CREATE INDEX IX_CancellationLogs_TenantId_DocumentType
    ON CancellationLogs(TenantId, DocumentType);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_CancellationLogs_TenantId_CancelledAt')
    CREATE INDEX IX_CancellationLogs_TenantId_CancelledAt
    ON CancellationLogs(TenantId, CancelledAt);

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
PRINT '=============================================================================';
GO
