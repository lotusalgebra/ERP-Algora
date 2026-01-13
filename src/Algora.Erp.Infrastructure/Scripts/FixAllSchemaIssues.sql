-- =============================================
-- FIX ALL MISSING COLUMNS ACROSS ALL TABLES
-- =============================================
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- =============================================
-- INDIAN STATES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IndianStates') AND name = 'GstStateCode')
    ALTER TABLE IndianStates ADD GstStateCode NVARCHAR(5) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IndianStates') AND name = 'DisplayOrder')
    ALTER TABLE IndianStates ADD DisplayOrder INT NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('IndianStates') AND name = 'IsUnionTerritory')
    ALTER TABLE IndianStates ADD IsUnionTerritory BIT NOT NULL DEFAULT 0;
GO

-- =============================================
-- PRODUCT CATEGORIES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProductCategories') AND name = 'DisplayOrder')
    ALTER TABLE ProductCategories ADD DisplayOrder INT NOT NULL DEFAULT 0;
GO

-- =============================================
-- WAREHOUSES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Warehouses') AND name = 'ContactPerson')
    ALTER TABLE Warehouses ADD ContactPerson NVARCHAR(200) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Warehouses') AND name = 'IsPrimary')
    ALTER TABLE Warehouses ADD IsPrimary BIT NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Warehouses') AND name = 'Phone')
    ALTER TABLE Warehouses ADD Phone NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Warehouses') AND name = 'Email')
    ALTER TABLE Warehouses ADD Email NVARCHAR(255) NULL;
GO

-- =============================================
-- CUSTOMERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Address')
    ALTER TABLE Customers ADD Address NVARCHAR(500) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'City')
    ALTER TABLE Customers ADD City NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'State')
    ALTER TABLE Customers ADD [State] NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PostalCode')
    ALTER TABLE Customers ADD PostalCode NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'Country')
    ALTER TABLE Customers ADD Country NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'PaymentTermDays')
    ALTER TABLE Customers ADD PaymentTermDays INT NOT NULL DEFAULT 30;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Customers') AND name = 'CreditLimit')
    ALTER TABLE Customers ADD CreditLimit DECIMAL(18,2) NULL;
GO

-- =============================================
-- SALES ORDERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'RequiredDate')
    ALTER TABLE SalesOrders ADD RequiredDate DATETIME2 NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('SalesOrders') AND name = 'SalesPersonId')
    ALTER TABLE SalesOrders ADD SalesPersonId UNIQUEIDENTIFIER NULL;
GO

-- =============================================
-- SUPPLIERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'Mobile')
    ALTER TABLE Suppliers ADD Mobile NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'PaymentTermDays')
    ALTER TABLE Suppliers ADD PaymentTermDays INT NOT NULL DEFAULT 30;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'Address')
    ALTER TABLE Suppliers ADD Address NVARCHAR(500) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'City')
    ALTER TABLE Suppliers ADD City NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'State')
    ALTER TABLE Suppliers ADD [State] NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'PostalCode')
    ALTER TABLE Suppliers ADD PostalCode NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'Country')
    ALTER TABLE Suppliers ADD Country NVARCHAR(100) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Suppliers') AND name = 'ContactPerson')
    ALTER TABLE Suppliers ADD ContactPerson NVARCHAR(200) NULL;
GO

-- =============================================
-- PURCHASE ORDERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PurchaseOrders') AND name = 'ExpectedDate')
    ALTER TABLE PurchaseOrders ADD ExpectedDate DATETIME2 NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('PurchaseOrders') AND name = 'WarehouseId')
    ALTER TABLE PurchaseOrders ADD WarehouseId UNIQUEIDENTIFIER NULL;
GO

-- =============================================
-- BILL OF MATERIALS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BillOfMaterials') AND name = 'Code')
    ALTER TABLE BillOfMaterials ADD Code NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BillOfMaterials') AND name = 'Version')
    ALTER TABLE BillOfMaterials ADD Version NVARCHAR(20) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BillOfMaterials') AND name = 'EffectiveDate')
    ALTER TABLE BillOfMaterials ADD EffectiveDate DATETIME2 NULL;
GO

-- =============================================
-- BOM LINES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BomLines') AND name = 'BomId')
    ALTER TABLE BomLines ADD BomId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BomLines') AND name = 'ItemNumber')
    ALTER TABLE BomLines ADD ItemNumber INT NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('BomLines') AND name = 'Scrap')
    ALTER TABLE BomLines ADD Scrap DECIMAL(5,2) NOT NULL DEFAULT 0;
GO

-- =============================================
-- WORK ORDERS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'OrderNumber')
    ALTER TABLE WorkOrders ADD OrderNumber NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'BomId')
    ALTER TABLE WorkOrders ADD BomId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'Quantity')
    ALTER TABLE WorkOrders ADD Quantity DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'PlannedQuantity')
    ALTER TABLE WorkOrders ADD PlannedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'CompletedQuantity')
    ALTER TABLE WorkOrders ADD CompletedQuantity DECIMAL(18,4) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'PlannedStartDate')
    ALTER TABLE WorkOrders ADD PlannedStartDate DATETIME2 NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('WorkOrders') AND name = 'PlannedEndDate')
    ALTER TABLE WorkOrders ADD PlannedEndDate DATETIME2 NULL;
GO

-- =============================================
-- PROJECTS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'Code')
    ALTER TABLE Projects ADD Code NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'ClientId')
    ALTER TABLE Projects ADD ClientId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'Budget')
    ALTER TABLE Projects ADD Budget DECIMAL(18,2) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Projects') AND name = 'ProjectManagerId')
    ALTER TABLE Projects ADD ProjectManagerId UNIQUEIDENTIFIER NULL;
GO

-- =============================================
-- PROJECT TASKS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProjectTasks') AND name = 'StartDate')
    ALTER TABLE ProjectTasks ADD StartDate DATETIME2 NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProjectTasks') AND name = 'AssignedToId')
    ALTER TABLE ProjectTasks ADD AssignedToId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProjectTasks') AND name = 'TaskNumber')
    ALTER TABLE ProjectTasks ADD TaskNumber NVARCHAR(50) NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProjectTasks') AND name = 'EstimatedHours')
    ALTER TABLE ProjectTasks ADD EstimatedHours DECIMAL(10,2) NOT NULL DEFAULT 0;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('ProjectTasks') AND name = 'ActualHours')
    ALTER TABLE ProjectTasks ADD ActualHours DECIMAL(10,2) NOT NULL DEFAULT 0;
GO

-- =============================================
-- TIME ENTRIES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TimeEntries') AND name = 'TaskId')
    ALTER TABLE TimeEntries ADD TaskId UNIQUEIDENTIFIER NULL;
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('TimeEntries') AND name = 'EmployeeId')
    ALTER TABLE TimeEntries ADD EmployeeId UNIQUEIDENTIFIER NULL;
GO

-- =============================================
-- ACCOUNTS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Accounts') AND name = 'AccountNumber')
    ALTER TABLE Accounts ADD AccountNumber NVARCHAR(50) NULL;
GO

PRINT 'All schema fixes applied successfully';
GO
