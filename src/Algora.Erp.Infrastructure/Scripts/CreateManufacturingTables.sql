SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- MANUFACTURING TABLES
-- Run this script in the tenant database
-- =============================================

-- Bill of Materials
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BillOfMaterials' AND xtype='U')
BEGIN
    CREATE TABLE BillOfMaterials (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        BomNumber NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500),
        ProductId UNIQUEIDENTIFIER NOT NULL,
        Quantity DECIMAL(18,4) DEFAULT 1,
        UnitOfMeasure NVARCHAR(20),
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Active, 2=Obsolete
        EffectiveFrom DATETIME2,
        EffectiveTo DATETIME2,
        IsActive BIT DEFAULT 1,
        Notes NVARCHAR(1000),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_BillOfMaterials_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_BillOfMaterials_BomNumber ON BillOfMaterials(BomNumber) WHERE IsDeleted = 0;
END
GO

-- BOM Lines
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='BomLines' AND xtype='U')
BEGIN
    CREATE TABLE BomLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        BillOfMaterialId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        LineNumber INT DEFAULT 1,
        Quantity DECIMAL(18,4) NOT NULL,
        UnitOfMeasure NVARCHAR(20),
        UnitCost DECIMAL(18,2) DEFAULT 0,
        TotalCost DECIMAL(18,2) DEFAULT 0,
        WastagePercent DECIMAL(5,2) DEFAULT 0,
        IsOptional BIT DEFAULT 0,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_BomLines_BillOfMaterial FOREIGN KEY (BillOfMaterialId) REFERENCES BillOfMaterials(Id) ON DELETE CASCADE,
        CONSTRAINT FK_BomLines_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION
    );
END
GO

-- Work Orders
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkOrders' AND xtype='U')
BEGIN
    CREATE TABLE WorkOrders (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        WorkOrderNumber NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        BillOfMaterialId UNIQUEIDENTIFIER,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        PlannedQuantity DECIMAL(18,4) NOT NULL,
        CompletedQuantity DECIMAL(18,4) DEFAULT 0,
        ScrapQuantity DECIMAL(18,4) DEFAULT 0,
        UnitOfMeasure NVARCHAR(20),
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Released, 2=InProgress, 3=OnHold, 4=Completed, 5=Cancelled
        Priority INT NOT NULL DEFAULT 1, -- 0=Low, 1=Normal, 2=High, 3=Urgent
        PlannedStartDate DATETIME2 NOT NULL,
        PlannedEndDate DATETIME2 NOT NULL,
        ActualStartDate DATETIME2,
        ActualEndDate DATETIME2,
        WarehouseId UNIQUEIDENTIFIER,
        EstimatedCost DECIMAL(18,2) DEFAULT 0,
        ActualCost DECIMAL(18,2) DEFAULT 0,
        Notes NVARCHAR(1000),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_WorkOrders_BillOfMaterial FOREIGN KEY (BillOfMaterialId) REFERENCES BillOfMaterials(Id) ON DELETE SET NULL,
        CONSTRAINT FK_WorkOrders_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_WorkOrders_Warehouse FOREIGN KEY (WarehouseId) REFERENCES Warehouses(Id) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX IX_WorkOrders_WorkOrderNumber ON WorkOrders(WorkOrderNumber) WHERE IsDeleted = 0;
    CREATE INDEX IX_WorkOrders_PlannedStartDate ON WorkOrders(PlannedStartDate);
END
GO

-- Work Order Operations
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkOrderOperations' AND xtype='U')
BEGIN
    CREATE TABLE WorkOrderOperations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        WorkOrderId UNIQUEIDENTIFIER NOT NULL,
        OperationNumber INT DEFAULT 1,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        Workstation NVARCHAR(100),
        PlannedHours DECIMAL(10,2) DEFAULT 0,
        ActualHours DECIMAL(10,2) DEFAULT 0,
        Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=InProgress, 2=Completed, 3=Skipped
        StartedAt DATETIME2,
        CompletedAt DATETIME2,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_WorkOrderOperations_WorkOrder FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id) ON DELETE CASCADE
    );
END
GO

-- Work Order Materials
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='WorkOrderMaterials' AND xtype='U')
BEGIN
    CREATE TABLE WorkOrderMaterials (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        WorkOrderId UNIQUEIDENTIFIER NOT NULL,
        ProductId UNIQUEIDENTIFIER NOT NULL,
        LineNumber INT DEFAULT 1,
        RequiredQuantity DECIMAL(18,4) NOT NULL,
        IssuedQuantity DECIMAL(18,4) DEFAULT 0,
        ReturnedQuantity DECIMAL(18,4) DEFAULT 0,
        UnitOfMeasure NVARCHAR(20),
        UnitCost DECIMAL(18,2) DEFAULT 0,
        Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=PartiallyIssued, 2=Issued, 3=Returned
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_WorkOrderMaterials_WorkOrder FOREIGN KEY (WorkOrderId) REFERENCES WorkOrders(Id) ON DELETE CASCADE,
        CONSTRAINT FK_WorkOrderMaterials_Product FOREIGN KEY (ProductId) REFERENCES Products(Id) ON DELETE NO ACTION
    );
END
GO

PRINT 'Manufacturing tables created successfully';
GO
