-- =============================================
-- ALGORA ERP - PRODUCTION READY SCHEMA FIX & UAT SEED DATA
-- =============================================
SET QUOTED_IDENTIFIER ON;
SET NOCOUNT ON;
GO

-- =============================================
-- PART 1: FIX SCHEMA ISSUES
-- =============================================

-- Add missing soft delete columns to RecurringInvoices
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RecurringInvoices') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE RecurringInvoices ADD DeletedAt DATETIME2 NULL;
    ALTER TABLE RecurringInvoices ADD DeletedBy UNIQUEIDENTIFIER NULL;
    PRINT 'Added soft delete columns to RecurringInvoices';
END
GO

-- Add missing soft delete columns to RecurringInvoiceLines
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('RecurringInvoiceLines') AND name = 'DeletedAt')
BEGIN
    ALTER TABLE RecurringInvoiceLines ADD DeletedAt DATETIME2 NULL;
    ALTER TABLE RecurringInvoiceLines ADD DeletedBy UNIQUEIDENTIFIER NULL;
    PRINT 'Added soft delete columns to RecurringInvoiceLines';
END
GO

-- =============================================
-- PART 2: SEED SETTINGS DATA
-- =============================================

-- Seed Indian States (if not already present)
IF NOT EXISTS (SELECT 1 FROM IndianStates WHERE Code = 'MH')
BEGIN
    INSERT INTO IndianStates (Id, Code, Name, GstStateCode, IsUnionTerritory, IsActive, DisplayOrder, CreatedAt)
    VALUES
    (NEWID(), 'AN', 'Andaman and Nicobar Islands', '35', 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'AP', 'Andhra Pradesh', '37', 0, 1, 2, GETUTCDATE()),
    (NEWID(), 'AR', 'Arunachal Pradesh', '12', 0, 1, 3, GETUTCDATE()),
    (NEWID(), 'AS', 'Assam', '18', 0, 1, 4, GETUTCDATE()),
    (NEWID(), 'BR', 'Bihar', '10', 0, 1, 5, GETUTCDATE()),
    (NEWID(), 'CH', 'Chandigarh', '04', 1, 1, 6, GETUTCDATE()),
    (NEWID(), 'CT', 'Chhattisgarh', '22', 0, 1, 7, GETUTCDATE()),
    (NEWID(), 'DL', 'Delhi', '07', 1, 1, 8, GETUTCDATE()),
    (NEWID(), 'GA', 'Goa', '30', 0, 1, 9, GETUTCDATE()),
    (NEWID(), 'GJ', 'Gujarat', '24', 0, 1, 10, GETUTCDATE()),
    (NEWID(), 'HR', 'Haryana', '06', 0, 1, 11, GETUTCDATE()),
    (NEWID(), 'HP', 'Himachal Pradesh', '02', 0, 1, 12, GETUTCDATE()),
    (NEWID(), 'JK', 'Jammu and Kashmir', '01', 1, 1, 13, GETUTCDATE()),
    (NEWID(), 'JH', 'Jharkhand', '20', 0, 1, 14, GETUTCDATE()),
    (NEWID(), 'KA', 'Karnataka', '29', 0, 1, 15, GETUTCDATE()),
    (NEWID(), 'KL', 'Kerala', '32', 0, 1, 16, GETUTCDATE()),
    (NEWID(), 'LA', 'Ladakh', '38', 1, 1, 17, GETUTCDATE()),
    (NEWID(), 'MP', 'Madhya Pradesh', '23', 0, 1, 18, GETUTCDATE()),
    (NEWID(), 'MH', 'Maharashtra', '27', 0, 1, 19, GETUTCDATE()),
    (NEWID(), 'MN', 'Manipur', '14', 0, 1, 20, GETUTCDATE()),
    (NEWID(), 'ML', 'Meghalaya', '17', 0, 1, 21, GETUTCDATE()),
    (NEWID(), 'MZ', 'Mizoram', '15', 0, 1, 22, GETUTCDATE()),
    (NEWID(), 'NL', 'Nagaland', '13', 0, 1, 23, GETUTCDATE()),
    (NEWID(), 'OR', 'Odisha', '21', 0, 1, 24, GETUTCDATE()),
    (NEWID(), 'PY', 'Puducherry', '34', 1, 1, 25, GETUTCDATE()),
    (NEWID(), 'PB', 'Punjab', '03', 0, 1, 26, GETUTCDATE()),
    (NEWID(), 'RJ', 'Rajasthan', '08', 0, 1, 27, GETUTCDATE()),
    (NEWID(), 'SK', 'Sikkim', '11', 0, 1, 28, GETUTCDATE()),
    (NEWID(), 'TN', 'Tamil Nadu', '33', 0, 1, 29, GETUTCDATE()),
    (NEWID(), 'TG', 'Telangana', '36', 0, 1, 30, GETUTCDATE()),
    (NEWID(), 'TR', 'Tripura', '16', 0, 1, 31, GETUTCDATE()),
    (NEWID(), 'UP', 'Uttar Pradesh', '09', 0, 1, 32, GETUTCDATE()),
    (NEWID(), 'UK', 'Uttarakhand', '05', 0, 1, 33, GETUTCDATE()),
    (NEWID(), 'WB', 'West Bengal', '19', 0, 1, 34, GETUTCDATE());
    PRINT 'Seeded Indian States';
END
GO

-- Seed Currencies
IF NOT EXISTS (SELECT 1 FROM Currencies WHERE Code = 'INR')
BEGIN
    INSERT INTO Currencies (Id, Code, Name, Symbol, SymbolPosition, DecimalPlaces, DecimalSeparator, ThousandsSeparator, ExchangeRate, IsBaseCurrency, IsActive, DisplayOrder, CreatedAt)
    VALUES
    (NEWID(), 'INR', 'Indian Rupee', '₹', 'before', 2, '.', ',', 1.00, 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'USD', 'US Dollar', '$', 'before', 2, '.', ',', 83.50, 0, 1, 2, GETUTCDATE()),
    (NEWID(), 'EUR', 'Euro', '€', 'before', 2, '.', ',', 91.20, 0, 1, 3, GETUTCDATE()),
    (NEWID(), 'GBP', 'British Pound', '£', 'before', 2, '.', ',', 106.50, 0, 1, 4, GETUTCDATE()),
    (NEWID(), 'AED', 'UAE Dirham', 'د.إ', 'before', 2, '.', ',', 22.73, 0, 1, 5, GETUTCDATE());
    PRINT 'Seeded Currencies';
END
GO

-- Seed GST Slabs
IF NOT EXISTS (SELECT 1 FROM GstSlabs WHERE Rate = 18)
BEGIN
    INSERT INTO GstSlabs (Id, Name, Rate, CgstRate, SgstRate, IgstRate, HsnCodes, IsDefault, IsActive, DisplayOrder, CreatedAt)
    VALUES
    (NEWID(), 'Exempt', 0, 0, 0, 0, NULL, 0, 1, 1, GETUTCDATE()),
    (NEWID(), 'GST 5%', 5, 2.5, 2.5, 5, NULL, 0, 1, 2, GETUTCDATE()),
    (NEWID(), 'GST 12%', 12, 6, 6, 12, NULL, 0, 1, 3, GETUTCDATE()),
    (NEWID(), 'GST 18%', 18, 9, 9, 18, NULL, 1, 1, 4, GETUTCDATE()),
    (NEWID(), 'GST 28%', 28, 14, 14, 28, NULL, 0, 1, 5, GETUTCDATE());
    PRINT 'Seeded GST Slabs';
END
GO

-- Get Maharashtra State ID for Office Location
DECLARE @MaharashtraId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM IndianStates WHERE Code = 'MH');
DECLARE @INRId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Currencies WHERE Code = 'INR');

-- Seed Office Locations
IF NOT EXISTS (SELECT 1 FROM OfficeLocations WHERE Code = 'HO')
BEGIN
    INSERT INTO OfficeLocations (Id, Code, Name, Type, AddressLine1, AddressLine2, City, StateId, PostalCode, Country, GstNumber, IsGstRegistered, GstRegistrationType, DefaultCurrencyId, Phone, Email, ContactPerson, IsHeadOffice, IsActive, DisplayOrder, CreatedAt)
    VALUES
    (NEWID(), 'HO', 'Head Office', 0, '123 Business Park', 'Tower A, Floor 5', 'Mumbai', @MaharashtraId, '400001', 'India', '27AABCU9603R1ZM', 1, 0, @INRId, '+91-22-12345678', 'info@company.com', 'John Smith', 1, 1, 1, GETUTCDATE()),
    (NEWID(), 'BR1', 'Pune Branch', 1, '456 Tech Hub', 'Building B', 'Pune', @MaharashtraId, '411001', 'India', '27AABCU9603R1ZN', 1, 0, @INRId, '+91-20-98765432', 'pune@company.com', 'Jane Doe', 0, 1, 2, GETUTCDATE());
    PRINT 'Seeded Office Locations';
END
GO

PRINT '=== Settings Data Seeded ===';
GO

-- =============================================
-- PART 3: SEED HR DATA
-- =============================================

-- Seed Departments
IF NOT EXISTS (SELECT 1 FROM Departments WHERE Code = 'EXEC')
BEGIN
    INSERT INTO Departments (Id, Code, Name, Description, IsActive, CreatedAt)
    VALUES
    (NEWID(), 'EXEC', 'Executive', 'Executive Management', 1, GETUTCDATE()),
    (NEWID(), 'FIN', 'Finance', 'Finance & Accounting Department', 1, GETUTCDATE()),
    (NEWID(), 'HR', 'Human Resources', 'HR & Administration', 1, GETUTCDATE()),
    (NEWID(), 'IT', 'Information Technology', 'IT & Software Development', 1, GETUTCDATE()),
    (NEWID(), 'SALES', 'Sales', 'Sales & Business Development', 1, GETUTCDATE()),
    (NEWID(), 'MFG', 'Manufacturing', 'Production & Manufacturing', 1, GETUTCDATE()),
    (NEWID(), 'QA', 'Quality Assurance', 'Quality Control & Assurance', 1, GETUTCDATE()),
    (NEWID(), 'LOG', 'Logistics', 'Warehouse & Logistics', 1, GETUTCDATE()),
    (NEWID(), 'PROC', 'Procurement', 'Procurement & Purchasing', 1, GETUTCDATE());
    PRINT 'Seeded Departments';
END
GO

-- Seed Positions
DECLARE @ExecDeptId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'EXEC');
DECLARE @FinDeptId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'FIN');
DECLARE @HRDeptId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'HR');
DECLARE @ITDeptId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'IT');
DECLARE @SalesDeptId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'SALES');
DECLARE @MfgDeptId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'MFG');

IF NOT EXISTS (SELECT 1 FROM Positions WHERE Code = 'CEO')
BEGIN
    INSERT INTO Positions (Id, Code, Title, Description, DepartmentId, MinSalary, MaxSalary, IsActive, CreatedAt)
    VALUES
    (NEWID(), 'CEO', 'Chief Executive Officer', 'Company CEO', @ExecDeptId, 5000000, 10000000, 1, GETUTCDATE()),
    (NEWID(), 'CFO', 'Chief Financial Officer', 'Finance Head', @FinDeptId, 3000000, 6000000, 1, GETUTCDATE()),
    (NEWID(), 'CTO', 'Chief Technology Officer', 'Technology Head', @ITDeptId, 3000000, 6000000, 1, GETUTCDATE()),
    (NEWID(), 'HRMGR', 'HR Manager', 'Human Resources Manager', @HRDeptId, 1200000, 2000000, 1, GETUTCDATE()),
    (NEWID(), 'ACCT', 'Accountant', 'Senior Accountant', @FinDeptId, 600000, 1000000, 1, GETUTCDATE()),
    (NEWID(), 'SENG', 'Software Engineer', 'Software Developer', @ITDeptId, 800000, 1500000, 1, GETUTCDATE()),
    (NEWID(), 'SLMGR', 'Sales Manager', 'Sales Team Lead', @SalesDeptId, 1000000, 1800000, 1, GETUTCDATE()),
    (NEWID(), 'SLEXEC', 'Sales Executive', 'Sales Representative', @SalesDeptId, 400000, 700000, 1, GETUTCDATE()),
    (NEWID(), 'PRODMGR', 'Production Manager', 'Manufacturing Head', @MfgDeptId, 1000000, 1600000, 1, GETUTCDATE()),
    (NEWID(), 'OPER', 'Machine Operator', 'Production Operator', @MfgDeptId, 300000, 500000, 1, GETUTCDATE());
    PRINT 'Seeded Positions';
END
GO

-- Seed Employees
DECLARE @CEOPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'CEO');
DECLARE @CFOPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'CFO');
DECLARE @CTOPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'CTO');
DECLARE @HRMgrPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'HRMGR');
DECLARE @AcctPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'ACCT');
DECLARE @SEngPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'SENG');
DECLARE @SalesMgrPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'SLMGR');
DECLARE @SalesExecPosId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Positions WHERE Code = 'SLEXEC');

DECLARE @ExecDept UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'EXEC');
DECLARE @FinDept UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'FIN');
DECLARE @HRDept UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'HR');
DECLARE @ITDept UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'IT');
DECLARE @SalesDept UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Departments WHERE Code = 'SALES');

IF NOT EXISTS (SELECT 1 FROM Employees WHERE EmployeeCode = 'EMP001')
BEGIN
    INSERT INTO Employees (Id, EmployeeCode, FirstName, LastName, Email, Phone, Mobile, DateOfBirth, Gender, Address, City, State, Country, DepartmentId, PositionId, HireDate, EmploymentType, EmploymentStatus, BaseSalary, SalaryCurrency, CreatedAt)
    VALUES
    (NEWID(), 'EMP001', 'Rajesh', 'Kumar', 'rajesh.kumar@company.com', '+91-22-11111111', '+91-9876543210', '1975-03-15', 0, '101 Executive Tower', 'Mumbai', 'Maharashtra', 'India', @ExecDept, @CEOPosId, '2015-01-01', 0, 0, 7500000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP002', 'Priya', 'Sharma', 'priya.sharma@company.com', '+91-22-22222222', '+91-9876543211', '1980-07-22', 1, '202 Finance Block', 'Mumbai', 'Maharashtra', 'India', @FinDept, @CFOPosId, '2016-04-01', 0, 0, 4500000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP003', 'Amit', 'Patel', 'amit.patel@company.com', '+91-22-33333333', '+91-9876543212', '1982-11-10', 0, '303 Tech Park', 'Mumbai', 'Maharashtra', 'India', @ITDept, @CTOPosId, '2016-08-01', 0, 0, 4200000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP004', 'Neha', 'Gupta', 'neha.gupta@company.com', '+91-22-44444444', '+91-9876543213', '1985-05-25', 1, '404 HR Complex', 'Mumbai', 'Maharashtra', 'India', @HRDept, @HRMgrPosId, '2017-01-15', 0, 0, 1500000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP005', 'Vikram', 'Singh', 'vikram.singh@company.com', '+91-22-55555555', '+91-9876543214', '1988-09-08', 0, '505 Sales Tower', 'Mumbai', 'Maharashtra', 'India', @SalesDept, @SalesMgrPosId, '2018-03-01', 0, 0, 1400000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP006', 'Sunita', 'Reddy', 'sunita.reddy@company.com', '+91-22-66666666', '+91-9876543215', '1990-12-12', 1, '606 Account Block', 'Mumbai', 'Maharashtra', 'India', @FinDept, @AcctPosId, '2019-06-01', 0, 0, 800000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP007', 'Rahul', 'Mehta', 'rahul.mehta@company.com', '+91-22-77777777', '+91-9876543216', '1992-02-28', 0, '707 Tech Hub', 'Mumbai', 'Maharashtra', 'India', @ITDept, @SEngPosId, '2020-01-15', 0, 0, 1100000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP008', 'Anjali', 'Desai', 'anjali.desai@company.com', '+91-22-88888888', '+91-9876543217', '1993-06-18', 1, '808 Tech Hub', 'Mumbai', 'Maharashtra', 'India', @ITDept, @SEngPosId, '2020-07-01', 0, 0, 1000000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP009', 'Kiran', 'Joshi', 'kiran.joshi@company.com', '+91-22-99999999', '+91-9876543218', '1994-10-05', 0, '909 Sales Wing', 'Mumbai', 'Maharashtra', 'India', @SalesDept, @SalesExecPosId, '2021-02-01', 0, 0, 550000, 'INR', GETUTCDATE()),
    (NEWID(), 'EMP010', 'Meera', 'Nair', 'meera.nair@company.com', '+91-22-10101010', '+91-9876543219', '1995-04-22', 1, '1010 Sales Wing', 'Mumbai', 'Maharashtra', 'India', @SalesDept, @SalesExecPosId, '2021-08-01', 0, 0, 500000, 'INR', GETUTCDATE());
    PRINT 'Seeded Employees';
END
GO

PRINT '=== HR Data Seeded ===';
GO

-- =============================================
-- PART 4: SEED INVENTORY DATA
-- =============================================

-- Seed Product Categories
IF NOT EXISTS (SELECT 1 FROM ProductCategories WHERE Code = 'ELEC')
BEGIN
    INSERT INTO ProductCategories (Id, Code, Name, Description, IsActive, DisplayOrder, CreatedAt)
    VALUES
    (NEWID(), 'ELEC', 'Electronics', 'Electronic Items & Gadgets', 1, 1, GETUTCDATE()),
    (NEWID(), 'COMP', 'Computers', 'Computers & Accessories', 1, 2, GETUTCDATE()),
    (NEWID(), 'FURN', 'Furniture', 'Office & Home Furniture', 1, 3, GETUTCDATE()),
    (NEWID(), 'STAT', 'Stationery', 'Office Stationery & Supplies', 1, 4, GETUTCDATE()),
    (NEWID(), 'RAW', 'Raw Materials', 'Manufacturing Raw Materials', 1, 5, GETUTCDATE()),
    (NEWID(), 'PACK', 'Packaging', 'Packaging Materials', 1, 6, GETUTCDATE()),
    (NEWID(), 'SPARE', 'Spare Parts', 'Machine Spare Parts', 1, 7, GETUTCDATE());
    PRINT 'Seeded Product Categories';
END
GO

-- Seed Warehouses
IF NOT EXISTS (SELECT 1 FROM Warehouses WHERE Code = 'WH-MAIN')
BEGIN
    INSERT INTO Warehouses (Id, Code, Name, Description, Address, City, State, Country, PostalCode, ContactPerson, Phone, Email, IsActive, IsPrimary, CreatedAt)
    VALUES
    (NEWID(), 'WH-MAIN', 'Main Warehouse', 'Primary distribution warehouse', '100 Industrial Area', 'Mumbai', 'Maharashtra', 'India', '400078', 'Suresh Warehouse', '+91-22-12340001', 'warehouse@company.com', 1, 1, GETUTCDATE()),
    (NEWID(), 'WH-PUNE', 'Pune Warehouse', 'Pune regional warehouse', '200 MIDC', 'Pune', 'Maharashtra', 'India', '411026', 'Ramesh Warehouse', '+91-20-12340002', 'pune.warehouse@company.com', 1, 0, GETUTCDATE());
    PRINT 'Seeded Warehouses';
END
GO

-- Get category and warehouse IDs
DECLARE @ElecCatId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Code = 'ELEC');
DECLARE @CompCatId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Code = 'COMP');
DECLARE @FurnCatId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Code = 'FURN');
DECLARE @StatCatId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Code = 'STAT');
DECLARE @RawCatId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProductCategories WHERE Code = 'RAW');
DECLARE @GST18Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM GstSlabs WHERE Rate = 18);
DECLARE @GST12Id UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM GstSlabs WHERE Rate = 12);
DECLARE @MainWHId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Warehouses WHERE Code = 'WH-MAIN');

-- Seed Products
IF NOT EXISTS (SELECT 1 FROM Products WHERE Sku = 'LAPTOP-001')
BEGIN
    INSERT INTO Products (Id, Sku, Name, Description, CategoryId, ProductType, UnitOfMeasure, CostPrice, SellingPrice, TaxRate, ReorderLevel, MinimumStock, IsSellable, IsPurchasable, IsActive, TrackInventory, QuantityOnHand, QuantityReserved, IsTaxable, GstSlabId, HsnCode, CreatedAt)
    VALUES
    (NEWID(), 'LAPTOP-001', 'Dell Laptop i5', 'Dell Inspiron 15 Core i5 Laptop', @CompCatId, 0, 'PCS', 45000, 55000, 18, 10, 5, 1, 1, 1, 1, 50, 5, 1, @GST18Id, '8471', GETUTCDATE()),
    (NEWID(), 'LAPTOP-002', 'HP Laptop i7', 'HP Pavilion 15 Core i7 Laptop', @CompCatId, 0, 'PCS', 65000, 78000, 18, 10, 5, 1, 1, 1, 1, 35, 3, 1, @GST18Id, '8471', GETUTCDATE()),
    (NEWID(), 'MONITOR-001', 'Samsung 24" Monitor', 'Samsung 24 inch Full HD LED Monitor', @CompCatId, 0, 'PCS', 12000, 15000, 18, 20, 10, 1, 1, 1, 1, 100, 10, 1, @GST18Id, '8528', GETUTCDATE()),
    (NEWID(), 'KEYBOARD-001', 'Logitech Wireless Keyboard', 'Logitech K380 Wireless Keyboard', @CompCatId, 0, 'PCS', 2000, 2800, 18, 50, 25, 1, 1, 1, 1, 200, 20, 1, @GST18Id, '8471', GETUTCDATE()),
    (NEWID(), 'MOUSE-001', 'Logitech Wireless Mouse', 'Logitech M185 Wireless Mouse', @CompCatId, 0, 'PCS', 800, 1200, 18, 100, 50, 1, 1, 1, 1, 300, 30, 1, @GST18Id, '8471', GETUTCDATE()),
    (NEWID(), 'DESK-001', 'Office Desk Standard', 'Standard Office Desk 5x3 ft', @FurnCatId, 0, 'PCS', 8000, 12000, 18, 10, 5, 1, 1, 1, 1, 25, 2, 1, @GST18Id, '9403', GETUTCDATE()),
    (NEWID(), 'CHAIR-001', 'Executive Chair', 'High Back Executive Office Chair', @FurnCatId, 0, 'PCS', 6000, 9000, 18, 20, 10, 1, 1, 1, 1, 50, 5, 1, @GST18Id, '9401', GETUTCDATE()),
    (NEWID(), 'PAPER-A4', 'A4 Paper Ream', 'A4 Size Copier Paper 500 Sheets', @StatCatId, 0, 'REAM', 250, 350, 12, 100, 50, 1, 1, 1, 1, 500, 50, 1, @GST12Id, '4802', GETUTCDATE()),
    (NEWID(), 'PEN-BLUE', 'Blue Ball Pen Pack', 'Blue Ball Point Pen (Pack of 10)', @StatCatId, 0, 'PACK', 80, 120, 12, 200, 100, 1, 1, 1, 1, 1000, 100, 1, @GST12Id, '9608', GETUTCDATE()),
    (NEWID(), 'STEEL-001', 'Stainless Steel Sheet', 'SS 304 Grade Sheet 4x8 ft', @RawCatId, 0, 'SHT', 15000, 18000, 18, 50, 20, 0, 1, 1, 1, 100, 10, 1, @GST18Id, '7219', GETUTCDATE());
    PRINT 'Seeded Products';
END
GO

-- Seed Stock Levels
DECLARE @MainWH UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Warehouses WHERE Code = 'WH-MAIN');
DECLARE @PuneWH UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Warehouses WHERE Code = 'WH-PUNE');

IF NOT EXISTS (SELECT 1 FROM StockLevels)
BEGIN
    INSERT INTO StockLevels (Id, ProductId, WarehouseId, Quantity, ReservedQuantity, MinimumLevel, ReorderPoint, CreatedAt)
    SELECT NEWID(), p.Id, @MainWH, p.QuantityOnHand * 0.7, p.QuantityReserved * 0.7, p.MinimumStock, p.ReorderLevel, GETUTCDATE()
    FROM Products p WHERE p.IsDeleted = 0;

    INSERT INTO StockLevels (Id, ProductId, WarehouseId, Quantity, ReservedQuantity, MinimumLevel, ReorderPoint, CreatedAt)
    SELECT NEWID(), p.Id, @PuneWH, p.QuantityOnHand * 0.3, p.QuantityReserved * 0.3, p.MinimumStock * 0.5, p.ReorderLevel * 0.5, GETUTCDATE()
    FROM Products p WHERE p.IsDeleted = 0;
    PRINT 'Seeded Stock Levels';
END
GO

PRINT '=== Inventory Data Seeded ===';
GO

-- =============================================
-- PART 5: SEED SALES DATA
-- =============================================

DECLARE @MHState UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM IndianStates WHERE Code = 'MH');
DECLARE @GJState UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM IndianStates WHERE Code = 'GJ');
DECLARE @KAState UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM IndianStates WHERE Code = 'KA');
DECLARE @DLState UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM IndianStates WHERE Code = 'DL');

-- Seed Customers
IF NOT EXISTS (SELECT 1 FROM Customers WHERE Code = 'CUST001')
BEGIN
    INSERT INTO Customers (Id, Code, Name, Email, Phone, Mobile, TaxId, Address, City, State, PostalCode, Country, ContactPerson, ContactPhone, CreditLimit, PaymentTermDays, IsActive, CreatedAt)
    VALUES
    (NEWID(), 'CUST001', 'Tech Solutions Pvt Ltd', 'contact@techsolutions.com', '+91-22-12345001', '+91-9800000001', '27AABCT1234R1ZX', '100 Tech Park', 'Mumbai', 'Maharashtra', '400001', 'India', 'Rakesh Sharma', '+91-9800000001', 500000, 30, 1, GETUTCDATE()),
    (NEWID(), 'CUST002', 'Global Industries Ltd', 'sales@globalind.com', '+91-79-12345002', '+91-9800000002', '24AABCG5678R1ZY', '200 Industrial Estate', 'Ahmedabad', 'Gujarat', '380001', 'India', 'Mahesh Patel', '+91-9800000002', 1000000, 45, 1, GETUTCDATE()),
    (NEWID(), 'CUST003', 'Infosys Consulting', 'procurement@infosys.com', '+91-80-12345003', '+91-9800000003', '29AABCI9012R1ZZ', '300 IT Hub', 'Bangalore', 'Karnataka', '560001', 'India', 'Anand Kumar', '+91-9800000003', 2000000, 30, 1, GETUTCDATE()),
    (NEWID(), 'CUST004', 'Delhi Traders', 'orders@delhitraders.com', '+91-11-12345004', '+91-9800000004', '07AABCD3456R1ZA', '400 Karol Bagh', 'New Delhi', 'Delhi', '110001', 'India', 'Sunil Gupta', '+91-9800000004', 300000, 15, 1, GETUTCDATE()),
    (NEWID(), 'CUST005', 'Maharashtra Steel Works', 'purchase@mhsteel.com', '+91-22-12345005', '+91-9800000005', '27AABCM7890R1ZB', '500 MIDC Andheri', 'Mumbai', 'Maharashtra', '400069', 'India', 'Deepak Shah', '+91-9800000005', 750000, 30, 1, GETUTCDATE());
    PRINT 'Seeded Customers';
END
GO

-- Seed Leads
IF NOT EXISTS (SELECT 1 FROM Leads WHERE Code = 'LEAD001')
BEGIN
    INSERT INTO Leads (Id, Code, CompanyName, ContactName, Email, Phone, Source, Status, EstimatedValue, Notes, CreatedAt)
    VALUES
    (NEWID(), 'LEAD001', 'Startup India Inc', 'Vikrant Mehta', 'vikrant@startupindia.com', '+91-9900001111', 0, 0, 250000, 'Interested in bulk laptop purchase', GETUTCDATE()),
    (NEWID(), 'LEAD002', 'E-Commerce Giant', 'Priya Singh', 'priya@ecomgiant.com', '+91-9900002222', 1, 1, 500000, 'Looking for office furniture', GETUTCDATE()),
    (NEWID(), 'LEAD003', 'Manufacturing Pro', 'Ramesh Iyer', 'ramesh@mfgpro.com', '+91-9900003333', 2, 2, 1500000, 'Interested in raw material supply contract', GETUTCDATE()),
    (NEWID(), 'LEAD004', 'Retail Chain Ltd', 'Sneha Kapoor', 'sneha@retailchain.com', '+91-9900004444', 0, 0, 300000, 'Computer accessories for new stores', GETUTCDATE()),
    (NEWID(), 'LEAD005', 'Education Trust', 'Dr. Suresh Patil', 'suresh@edutrust.org', '+91-9900005555', 3, 1, 800000, 'Lab equipment for schools', GETUTCDATE());
    PRINT 'Seeded Leads';
END
GO

-- Seed Sales Orders
DECLARE @Cust1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST001');
DECLARE @Cust2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST002');
DECLARE @Cust3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST003');
DECLARE @SalesEmp UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP005');

IF NOT EXISTS (SELECT 1 FROM SalesOrders WHERE OrderNumber = 'SO-2024-0001')
BEGIN
    INSERT INTO SalesOrders (Id, OrderNumber, CustomerId, OrderDate, RequiredDate, Status, SubTotal, TaxAmount, TotalAmount, Currency, Reference, SalesPersonId, Notes, CreatedAt)
    VALUES
    (NEWID(), 'SO-2024-0001', @Cust1, '2024-01-15', '2024-01-25', 2, 275000, 49500, 324500, 'INR', 'PO-TS-001', @SalesEmp, 'Urgent delivery required', GETUTCDATE()),
    (NEWID(), 'SO-2024-0002', @Cust2, '2024-01-18', '2024-02-01', 1, 180000, 32400, 212400, 'INR', 'PO-GI-002', @SalesEmp, 'Regular order', GETUTCDATE()),
    (NEWID(), 'SO-2024-0003', @Cust3, '2024-01-20', '2024-02-05', 0, 450000, 81000, 531000, 'INR', 'PO-INF-003', @SalesEmp, 'Bulk order for new office', GETUTCDATE());
    PRINT 'Seeded Sales Orders';
END
GO

PRINT '=== Sales Data Seeded ===';
GO

-- =============================================
-- PART 6: SEED PROCUREMENT DATA
-- =============================================

-- Seed Suppliers
IF NOT EXISTS (SELECT 1 FROM Suppliers WHERE Code = 'SUP001')
BEGIN
    INSERT INTO Suppliers (Id, Code, Name, Email, Phone, Mobile, TaxId, Address, City, State, PostalCode, Country, ContactPerson, PaymentTermDays, IsActive, CreatedAt)
    VALUES
    (NEWID(), 'SUP001', 'Dell India Pvt Ltd', 'orders@dell.in', '+91-80-12340001', '+91-9700000001', '29AABCD1234R1ZX', '100 Electronic City', 'Bangalore', 'Karnataka', '560100', 'India', 'Sanjay Dell', 30, 1, GETUTCDATE()),
    (NEWID(), 'SUP002', 'HP India Ltd', 'supply@hp.in', '+91-80-12340002', '+91-9700000002', '29AABCH5678R1ZY', '200 IT Park', 'Bangalore', 'Karnataka', '560066', 'India', 'Ravi HP', 45, 1, GETUTCDATE()),
    (NEWID(), 'SUP003', 'Office Furniture Co', 'sales@officefurn.com', '+91-22-12340003', '+91-9700000003', '27AABCO9012R1ZZ', '300 Furniture Market', 'Mumbai', 'Maharashtra', '400037', 'India', 'Ashok Furniture', 15, 1, GETUTCDATE()),
    (NEWID(), 'SUP004', 'Paper Mills Ltd', 'orders@papermills.com', '+91-79-12340004', '+91-9700000004', '24AABCP3456R1ZA', '400 Paper Estate', 'Ahmedabad', 'Gujarat', '382210', 'India', 'Suresh Paper', 30, 1, GETUTCDATE()),
    (NEWID(), 'SUP005', 'Steel Corporation', 'purchase@steelcorp.com', '+91-22-12340005', '+91-9700000005', '27AABCS7890R1ZB', '500 Steel Market', 'Mumbai', 'Maharashtra', '400015', 'India', 'Manoj Steel', 60, 1, GETUTCDATE());
    PRINT 'Seeded Suppliers';
END
GO

-- Seed Purchase Orders
DECLARE @Sup1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Suppliers WHERE Code = 'SUP001');
DECLARE @Sup2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Suppliers WHERE Code = 'SUP002');
DECLARE @Sup3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Suppliers WHERE Code = 'SUP003');
DECLARE @ProcEmp UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP006');
DECLARE @MainWarehouse UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Warehouses WHERE Code = 'WH-MAIN');

IF NOT EXISTS (SELECT 1 FROM PurchaseOrders WHERE OrderNumber = 'PO-2024-0001')
BEGIN
    INSERT INTO PurchaseOrders (Id, OrderNumber, SupplierId, WarehouseId, OrderDate, ExpectedDate, Status, SubTotal, TaxAmount, TotalAmount, Currency, Reference, Notes, CreatedAt)
    VALUES
    (NEWID(), 'PO-2024-0001', @Sup1, @MainWarehouse, '2024-01-10', '2024-01-20', 2, 900000, 162000, 1062000, 'INR', 'Q-DELL-001', 'Laptop bulk order', GETUTCDATE()),
    (NEWID(), 'PO-2024-0002', @Sup2, @MainWarehouse, '2024-01-12', '2024-01-22', 1, 240000, 43200, 283200, 'INR', 'Q-HP-002', 'Monitor replenishment', GETUTCDATE()),
    (NEWID(), 'PO-2024-0003', @Sup3, @MainWarehouse, '2024-01-15', '2024-01-30', 0, 150000, 27000, 177000, 'INR', 'Q-FURN-003', 'Office expansion furniture', GETUTCDATE());
    PRINT 'Seeded Purchase Orders';
END
GO

PRINT '=== Procurement Data Seeded ===';
GO

-- =============================================
-- PART 7: SEED FINANCE DATA
-- =============================================

-- Get required IDs
DECLARE @Customer1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST001');
DECLARE @Customer2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST002');
DECLARE @Customer3 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST003');
DECLARE @GST18 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM GstSlabs WHERE Rate = 18);
DECLARE @HeadOffice UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM OfficeLocations WHERE Code = 'HO');

-- Seed Invoices
IF NOT EXISTS (SELECT 1 FROM Invoices WHERE InvoiceNumber = 'INV-2024-0001')
BEGIN
    INSERT INTO Invoices (Id, InvoiceNumber, Type, Status, CustomerId, InvoiceDate, DueDate, SubTotal, DiscountPercent, DiscountAmount, TaxAmount, CgstAmount, SgstAmount, IgstAmount, CgstRate, SgstRate, IgstRate, TotalAmount, PaidAmount, BalanceDue, Currency, PaymentTermDays, BillingName, BillingAddress, BillingCity, BillingState, BillingCountry, GstSlabId, FromLocationId, IsInterState, CreatedAt)
    VALUES
    (NEWID(), 'INV-2024-0001', 0, 1, @Customer1, '2024-01-20', '2024-02-19', 275000, 0, 0, 49500, 24750, 24750, 0, 9, 9, 0, 324500, 324500, 0, 'INR', 30, 'Tech Solutions Pvt Ltd', '100 Tech Park', 'Mumbai', 'Maharashtra', 'India', @GST18, @HeadOffice, 0, GETUTCDATE()),
    (NEWID(), 'INV-2024-0002', 0, 0, @Customer2, '2024-01-22', '2024-03-07', 180000, 5, 9000, 30780, 0, 0, 30780, 0, 0, 18, 201780, 100000, 101780, 'INR', 45, 'Global Industries Ltd', '200 Industrial Estate', 'Ahmedabad', 'Gujarat', 'India', @GST18, @HeadOffice, 1, GETUTCDATE()),
    (NEWID(), 'INV-2024-0003', 0, 0, @Customer3, '2024-01-25', '2024-02-24', 450000, 0, 0, 81000, 0, 0, 81000, 0, 0, 18, 531000, 0, 531000, 'INR', 30, 'Infosys Consulting', '300 IT Hub', 'Bangalore', 'Karnataka', 'India', @GST18, @HeadOffice, 1, GETUTCDATE());
    PRINT 'Seeded Invoices';
END
GO

-- Seed Invoice Payments
DECLARE @Inv1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = 'INV-2024-0001');
DECLARE @Inv2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Invoices WHERE InvoiceNumber = 'INV-2024-0002');
DECLARE @BankAccount UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Accounts WHERE Code = '1010' OR AccountNumber = '1010');

IF NOT EXISTS (SELECT 1 FROM InvoicePayments WHERE InvoiceId = @Inv1)
BEGIN
    INSERT INTO InvoicePayments (Id, InvoiceId, PaymentDate, Amount, PaymentMethod, Reference, Notes, CreatedAt)
    VALUES
    (NEWID(), @Inv1, '2024-02-10', 324500, 1, 'NEFT-001234', 'Full payment received', GETUTCDATE()),
    (NEWID(), @Inv2, '2024-02-15', 100000, 1, 'NEFT-001235', 'Partial payment received', GETUTCDATE());
    PRINT 'Seeded Invoice Payments';
END
GO

PRINT '=== Finance Data Seeded ===';
GO

-- =============================================
-- PART 8: SEED MANUFACTURING DATA
-- =============================================

DECLARE @LaptopProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'LAPTOP-001');
DECLARE @SteelProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'STEEL-001');
DECLARE @KeyboardProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'KEYBOARD-001');
DECLARE @MonitorProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'MONITOR-001');
DECLARE @MouseProduct UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'MOUSE-001');

-- Seed Bill of Materials
IF NOT EXISTS (SELECT 1 FROM BillOfMaterials WHERE Code = 'BOM-WORKSTATION')
BEGIN
    DECLARE @BomId UNIQUEIDENTIFIER = NEWID();
    INSERT INTO BillOfMaterials (Id, Code, Name, Description, ProductId, Version, Quantity, UnitOfMeasure, Status, EffectiveDate, IsActive, CreatedAt)
    VALUES
    (@BomId, 'BOM-WORKSTATION', 'Complete Workstation BOM', 'Bill of Materials for standard workstation setup', @LaptopProduct, '1.0', 1, 'SET', 1, '2024-01-01', 1, GETUTCDATE());

    INSERT INTO BomLines (Id, BomId, ItemNumber, ProductId, Quantity, UnitOfMeasure, Scrap, Notes, CreatedAt)
    VALUES
    (NEWID(), @BomId, 1, @LaptopProduct, 1, 'PCS', 0, 'Main laptop unit', GETUTCDATE()),
    (NEWID(), @BomId, 2, @MonitorProduct, 1, 'PCS', 0, 'External monitor', GETUTCDATE()),
    (NEWID(), @BomId, 3, @KeyboardProduct, 1, 'PCS', 0, 'Wireless keyboard', GETUTCDATE()),
    (NEWID(), @BomId, 4, @MouseProduct, 1, 'PCS', 0, 'Wireless mouse', GETUTCDATE());
    PRINT 'Seeded Bill of Materials';
END
GO

-- Seed Work Orders
DECLARE @BOM1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM BillOfMaterials WHERE Code = 'BOM-WORKSTATION');
DECLARE @Laptop UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Products WHERE Sku = 'LAPTOP-001');
DECLARE @ProdMgr UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE PositionId = (SELECT Id FROM Positions WHERE Code = 'PRODMGR'));

IF NOT EXISTS (SELECT 1 FROM WorkOrders WHERE OrderNumber = 'WO-2024-0001')
BEGIN
    INSERT INTO WorkOrders (Id, OrderNumber, BomId, ProductId, Quantity, PlannedQuantity, CompletedQuantity, Status, Priority, PlannedStartDate, PlannedEndDate, Notes, CreatedAt)
    VALUES
    (NEWID(), 'WO-2024-0001', @BOM1, @Laptop, 10, 10, 0, 0, 1, '2024-02-01', '2024-02-15', 'Workstation assembly for CUST001 order', GETUTCDATE()),
    (NEWID(), 'WO-2024-0002', @BOM1, @Laptop, 5, 5, 3, 1, 0, '2024-01-20', '2024-01-30', 'In progress - workstation batch', GETUTCDATE());
    PRINT 'Seeded Work Orders';
END
GO

PRINT '=== Manufacturing Data Seeded ===';
GO

-- =============================================
-- PART 9: SEED PROJECTS DATA
-- =============================================

DECLARE @PM UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP003');
DECLARE @Dev1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP007');
DECLARE @Dev2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Employees WHERE EmployeeCode = 'EMP008');
DECLARE @ProjectCust UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Customers WHERE Code = 'CUST003');

-- Seed Projects
IF NOT EXISTS (SELECT 1 FROM Projects WHERE Code = 'PROJ001')
BEGIN
    DECLARE @Proj1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Proj2 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO Projects (Id, Code, Name, Description, ClientId, ProjectManagerId, StartDate, EndDate, Budget, Status, Priority, IsActive, CreatedAt)
    VALUES
    (@Proj1, 'PROJ001', 'ERP Implementation', 'Complete ERP system implementation for client', @ProjectCust, @PM, '2024-01-01', '2024-06-30', 5000000, 1, 1, 1, GETUTCDATE()),
    (@Proj2, 'PROJ002', 'Website Redesign', 'Corporate website redesign project', @ProjectCust, @PM, '2024-02-01', '2024-04-30', 1500000, 0, 0, 1, GETUTCDATE());

    -- Seed Project Tasks
    INSERT INTO ProjectTasks (Id, ProjectId, TaskNumber, Title, Description, Status, Priority, StartDate, DueDate, EstimatedHours, ActualHours, AssignedToId, CreatedAt)
    VALUES
    (NEWID(), @Proj1, 'T001', 'Requirements Gathering', 'Gather and document all requirements', 2, 1, '2024-01-01', '2024-01-15', 40, 45, @PM, GETUTCDATE()),
    (NEWID(), @Proj1, 'T002', 'System Design', 'Design system architecture', 2, 1, '2024-01-16', '2024-01-31', 60, 55, @Dev1, GETUTCDATE()),
    (NEWID(), @Proj1, 'T003', 'Development Phase 1', 'Core module development', 1, 1, '2024-02-01', '2024-03-15', 200, 80, @Dev1, GETUTCDATE()),
    (NEWID(), @Proj1, 'T004', 'Development Phase 2', 'Additional modules', 0, 0, '2024-03-16', '2024-04-30', 200, 0, @Dev2, GETUTCDATE()),
    (NEWID(), @Proj2, 'T001', 'Design Mockups', 'Create UI/UX designs', 1, 1, '2024-02-01', '2024-02-15', 30, 15, @Dev2, GETUTCDATE());

    -- Seed Time Entries
    DECLARE @Task1 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProjectTasks WHERE TaskNumber = 'T001' AND ProjectId = @Proj1);
    DECLARE @Task2 UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM ProjectTasks WHERE TaskNumber = 'T002' AND ProjectId = @Proj1);

    INSERT INTO TimeEntries (Id, ProjectId, TaskId, EmployeeId, Date, Hours, Description, IsBillable, HourlyRate, Status, CreatedAt)
    VALUES
    (NEWID(), @Proj1, @Task1, @PM, '2024-01-05', 8, 'Client meeting - requirements discussion', 1, 2500, 1, GETUTCDATE()),
    (NEWID(), @Proj1, @Task1, @PM, '2024-01-08', 8, 'Document requirements', 1, 2500, 1, GETUTCDATE()),
    (NEWID(), @Proj1, @Task2, @Dev1, '2024-01-18', 8, 'Architecture design', 1, 2000, 1, GETUTCDATE()),
    (NEWID(), @Proj1, @Task2, @Dev1, '2024-01-20', 6, 'Database design', 1, 2000, 1, GETUTCDATE());

    PRINT 'Seeded Projects, Tasks, and Time Entries';
END
GO

PRINT '=== Projects Data Seeded ===';
GO

-- =============================================
-- PART 10: UPDATE USER ROLES
-- =============================================

-- Ensure admin user has Admin role with all permissions
DECLARE @AdminUserId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Users WHERE Email = 'admin@test.com');
DECLARE @AdminRoleId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Roles WHERE Name = 'Admin');

IF @AdminUserId IS NOT NULL AND @AdminRoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
    BEGIN
        INSERT INTO UserRoles (Id, UserId, RoleId) VALUES (NEWID(), @AdminUserId, @AdminRoleId);
    END
    PRINT 'Admin user role verified';
END
GO

PRINT '';
PRINT '=============================================';
PRINT 'UAT SEED DATA COMPLETE';
PRINT '=============================================';
PRINT 'Login: admin@test.com / admin123';
PRINT '=============================================';
GO
