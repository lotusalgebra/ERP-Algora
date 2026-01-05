-- =============================================
-- Algora ERP - Inventory Module Tables
-- =============================================

-- Product Categories Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'ProductCategories') AND type in (N'U'))
BEGIN
    CREATE TABLE ProductCategories (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        ParentCategoryId UNIQUEIDENTIFIER NULL,
        IsActive BIT NOT NULL DEFAULT 1,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_ProductCategories_Parent FOREIGN KEY (ParentCategoryId)
            REFERENCES ProductCategories(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_ProductCategories_Code ON ProductCategories(Code) WHERE IsDeleted = 0;
END
GO

-- Products Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Products') AND type in (N'U'))
BEGIN
    CREATE TABLE Products (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Sku NVARCHAR(50) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000) NULL,
        Barcode NVARCHAR(100) NULL,

        CategoryId UNIQUEIDENTIFIER NULL,
        ProductType INT NOT NULL DEFAULT 0, -- 0=Goods, 1=Service, 2=RawMaterial, etc.

        UnitOfMeasure NVARCHAR(20) NULL,
        Brand NVARCHAR(100) NULL,
        Manufacturer NVARCHAR(100) NULL,
        ImageUrl NVARCHAR(500) NULL,

        -- Pricing
        CostPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        SellingPrice DECIMAL(18,2) NOT NULL DEFAULT 0,
        TaxRate DECIMAL(5,2) NOT NULL DEFAULT 0,

        -- Inventory
        ReorderLevel DECIMAL(18,4) NOT NULL DEFAULT 0,
        MinimumStock DECIMAL(18,4) NOT NULL DEFAULT 0,
        MaximumStock DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- Dimensions
        Weight DECIMAL(10,3) NULL,
        Length DECIMAL(10,3) NULL,
        Width DECIMAL(10,3) NULL,
        Height DECIMAL(10,3) NULL,

        IsSellable BIT NOT NULL DEFAULT 1,
        IsPurchasable BIT NOT NULL DEFAULT 1,
        IsActive BIT NOT NULL DEFAULT 1,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_Products_Category FOREIGN KEY (CategoryId)
            REFERENCES ProductCategories(Id) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX IX_Products_Sku ON Products(Sku) WHERE IsDeleted = 0;
    CREATE INDEX IX_Products_Barcode ON Products(Barcode) WHERE IsDeleted = 0;
    CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
END
GO

-- Warehouses Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'Warehouses') AND type in (N'U'))
BEGIN
    CREATE TABLE Warehouses (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,

        -- Location
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,

        -- Contact
        ManagerName NVARCHAR(100) NULL,
        Phone NVARCHAR(50) NULL,
        Email NVARCHAR(255) NULL,

        IsActive BIT NOT NULL DEFAULT 1,
        IsDefault BIT NOT NULL DEFAULT 0,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    CREATE UNIQUE INDEX IX_Warehouses_Code ON Warehouses(Code) WHERE IsDeleted = 0;
END
GO

-- Warehouse Locations Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'WarehouseLocations') AND type in (N'U'))
BEGIN
    CREATE TABLE WarehouseLocations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        WarehouseId UNIQUEIDENTIFIER NOT NULL,
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,

        Zone NVARCHAR(50) NULL,
        Aisle NVARCHAR(50) NULL,
        Rack NVARCHAR(50) NULL,
        Shelf NVARCHAR(50) NULL,
        Bin NVARCHAR(50) NULL,

        IsActive BIT NOT NULL DEFAULT 1,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_WarehouseLocations_Warehouse FOREIGN KEY (WarehouseId)
            REFERENCES Warehouses(Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX IX_WarehouseLocations_WarehouseCode ON WarehouseLocations(WarehouseId, Code) WHERE IsDeleted = 0;
END
GO

-- Stock Levels Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'StockLevels') AND type in (N'U'))
BEGIN
    CREATE TABLE StockLevels (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProductId UNIQUEIDENTIFIER NOT NULL,
        WarehouseId UNIQUEIDENTIFIER NOT NULL,
        LocationId UNIQUEIDENTIFIER NULL,

        QuantityOnHand DECIMAL(18,4) NOT NULL DEFAULT 0,
        QuantityReserved DECIMAL(18,4) NOT NULL DEFAULT 0,
        QuantityOnOrder DECIMAL(18,4) NOT NULL DEFAULT 0,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_StockLevels_Product FOREIGN KEY (ProductId)
            REFERENCES Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_StockLevels_Warehouse FOREIGN KEY (WarehouseId)
            REFERENCES Warehouses(Id) ON DELETE CASCADE,
        CONSTRAINT FK_StockLevels_Location FOREIGN KEY (LocationId)
            REFERENCES WarehouseLocations(Id) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX IX_StockLevels_ProductWarehouseLocation ON StockLevels(ProductId, WarehouseId, LocationId) WHERE IsDeleted = 0;
END
GO

-- Stock Movements Table
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'StockMovements') AND type in (N'U'))
BEGIN
    CREATE TABLE StockMovements (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        ProductId UNIQUEIDENTIFIER NOT NULL,
        WarehouseId UNIQUEIDENTIFIER NOT NULL,
        FromLocationId UNIQUEIDENTIFIER NULL,
        ToLocationId UNIQUEIDENTIFIER NULL,

        MovementType INT NOT NULL, -- 0=Receipt, 1=Issue, 2=Transfer, 3=Adjustment, etc.
        Quantity DECIMAL(18,4) NOT NULL,

        Reference NVARCHAR(100) NULL,
        SourceDocumentType NVARCHAR(50) NULL,
        SourceDocumentId UNIQUEIDENTIFIER NULL,

        MovementDate DATETIME2 NOT NULL,
        Notes NVARCHAR(500) NULL,

        -- Audit columns
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_StockMovements_Product FOREIGN KEY (ProductId)
            REFERENCES Products(Id) ON DELETE CASCADE,
        CONSTRAINT FK_StockMovements_Warehouse FOREIGN KEY (WarehouseId)
            REFERENCES Warehouses(Id) ON DELETE CASCADE,
        CONSTRAINT FK_StockMovements_FromLocation FOREIGN KEY (FromLocationId)
            REFERENCES WarehouseLocations(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_StockMovements_ToLocation FOREIGN KEY (ToLocationId)
            REFERENCES WarehouseLocations(Id) ON DELETE NO ACTION
    );

    CREATE INDEX IX_StockMovements_MovementDate ON StockMovements(MovementDate);
    CREATE INDEX IX_StockMovements_SourceDocument ON StockMovements(SourceDocumentType, SourceDocumentId);
END
GO

-- Insert default product categories
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Code = 'GEN')
BEGIN
    INSERT INTO ProductCategories (Id, Code, Name, Description, IsActive)
    VALUES
        (NEWID(), 'GEN', 'General', 'General products', 1),
        (NEWID(), 'RAW', 'Raw Materials', 'Raw materials for production', 1),
        (NEWID(), 'FIN', 'Finished Goods', 'Finished products ready for sale', 1),
        (NEWID(), 'PKG', 'Packaging', 'Packaging materials', 1),
        (NEWID(), 'SPR', 'Spare Parts', 'Spare parts and components', 1),
        (NEWID(), 'CON', 'Consumables', 'Consumable items', 1);
END
GO

-- Insert default warehouse
IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE Code = 'MAIN')
BEGIN
    INSERT INTO Warehouses (Id, Code, Name, Description, IsActive, IsDefault)
    VALUES (NEWID(), 'MAIN', 'Main Warehouse', 'Primary warehouse location', 1, 1);
END
GO

PRINT 'Inventory tables created successfully';
