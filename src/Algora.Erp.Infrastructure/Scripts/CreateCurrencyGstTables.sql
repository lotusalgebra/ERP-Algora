-- Currency, GST and Office Location Tables Migration
-- Creates tables and seeds data for multi-currency and GST support

SET QUOTED_IDENTIFIER ON;
GO

-- Currencies Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Currencies')
BEGIN
    CREATE TABLE Currencies (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(10) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Symbol NVARCHAR(10) NOT NULL,
        SymbolPosition NVARCHAR(10) NOT NULL DEFAULT 'before',
        DecimalPlaces INT NOT NULL DEFAULT 2,
        DecimalSeparator NVARCHAR(5) NOT NULL DEFAULT '.',
        ThousandsSeparator NVARCHAR(5) NOT NULL DEFAULT ',',
        ExchangeRate DECIMAL(18,6) NOT NULL DEFAULT 1.0,
        IsBaseCurrency BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    CREATE UNIQUE INDEX IX_Currencies_Code ON Currencies(Code) WHERE IsDeleted = 0;
END
GO

-- Indian States Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'IndianStates')
BEGIN
    CREATE TABLE IndianStates (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(5) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        ShortName NVARCHAR(10) NOT NULL,
        IsUnionTerritory BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    CREATE UNIQUE INDEX IX_IndianStates_Code ON IndianStates(Code) WHERE IsDeleted = 0;
END
GO

-- GST Slabs Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'GstSlabs')
BEGIN
    CREATE TABLE GstSlabs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Name NVARCHAR(100) NOT NULL,
        Rate DECIMAL(5,2) NOT NULL,
        CgstRate DECIMAL(5,2) NOT NULL,
        SgstRate DECIMAL(5,2) NOT NULL,
        IgstRate DECIMAL(5,2) NOT NULL,
        HsnCodes NVARCHAR(MAX) NULL,
        IsDefault BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );
END
GO

-- Office Locations Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'OfficeLocations')
BEGIN
    CREATE TABLE OfficeLocations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Type INT NOT NULL DEFAULT 1, -- 0=HeadOffice, 1=Branch, 2=Warehouse, 3=Factory, 4=RegisteredOffice
        AddressLine1 NVARCHAR(500) NOT NULL,
        AddressLine2 NVARCHAR(500) NULL,
        City NVARCHAR(100) NOT NULL,
        StateId UNIQUEIDENTIFIER NULL,
        PostalCode NVARCHAR(20) NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'India',
        GstNumber NVARCHAR(20) NULL,
        IsGstRegistered BIT NOT NULL DEFAULT 1,
        GstRegistrationType INT NOT NULL DEFAULT 0, -- 0=Regular, 1=Composition, etc.
        DefaultCurrencyId UNIQUEIDENTIFIER NULL,
        Phone NVARCHAR(50) NULL,
        Email NVARCHAR(200) NULL,
        ContactPerson NVARCHAR(200) NULL,
        IsHeadOffice BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_OfficeLocations_State FOREIGN KEY (StateId) REFERENCES IndianStates(Id),
        CONSTRAINT FK_OfficeLocations_Currency FOREIGN KEY (DefaultCurrencyId) REFERENCES Currencies(Id)
    );

    CREATE UNIQUE INDEX IX_OfficeLocations_Code ON OfficeLocations(Code) WHERE IsDeleted = 0;
END
GO

-- Seed Currencies
IF NOT EXISTS (SELECT 1 FROM Currencies WHERE Code = 'INR')
BEGIN
    INSERT INTO Currencies (Code, Name, Symbol, SymbolPosition, DecimalPlaces, DecimalSeparator, ThousandsSeparator, ExchangeRate, IsBaseCurrency, DisplayOrder)
    VALUES
        ('INR', 'Indian Rupee', N'₹', 'before', 2, '.', ',', 1.0, 1, 1),
        ('USD', 'US Dollar', '$', 'before', 2, '.', ',', 0.012, 0, 2),
        ('EUR', 'Euro', N'€', 'before', 2, ',', '.', 0.011, 0, 3),
        ('GBP', 'British Pound', N'£', 'before', 2, '.', ',', 0.0095, 0, 4),
        ('AED', 'UAE Dirham', 'AED', 'before', 2, '.', ',', 0.044, 0, 5),
        ('SGD', 'Singapore Dollar', 'S$', 'before', 2, '.', ',', 0.016, 0, 6),
        ('JPY', 'Japanese Yen', N'¥', 'before', 0, '.', ',', 1.79, 0, 7),
        ('CAD', 'Canadian Dollar', 'C$', 'before', 2, '.', ',', 0.016, 0, 8),
        ('AUD', 'Australian Dollar', 'A$', 'before', 2, '.', ',', 0.018, 0, 9);
END
GO

-- Seed Indian States (All 28 States + 8 Union Territories)
IF NOT EXISTS (SELECT 1 FROM IndianStates WHERE Code = '01')
BEGIN
    INSERT INTO IndianStates (Code, Name, ShortName, IsUnionTerritory)
    VALUES
        -- States
        ('01', 'Jammu and Kashmir', 'JK', 0),
        ('02', 'Himachal Pradesh', 'HP', 0),
        ('03', 'Punjab', 'PB', 0),
        ('04', 'Chandigarh', 'CH', 1),
        ('05', 'Uttarakhand', 'UK', 0),
        ('06', 'Haryana', 'HR', 0),
        ('07', 'Delhi', 'DL', 1),
        ('08', 'Rajasthan', 'RJ', 0),
        ('09', 'Uttar Pradesh', 'UP', 0),
        ('10', 'Bihar', 'BR', 0),
        ('11', 'Sikkim', 'SK', 0),
        ('12', 'Arunachal Pradesh', 'AR', 0),
        ('13', 'Nagaland', 'NL', 0),
        ('14', 'Manipur', 'MN', 0),
        ('15', 'Mizoram', 'MZ', 0),
        ('16', 'Tripura', 'TR', 0),
        ('17', 'Meghalaya', 'ML', 0),
        ('18', 'Assam', 'AS', 0),
        ('19', 'West Bengal', 'WB', 0),
        ('20', 'Jharkhand', 'JH', 0),
        ('21', 'Odisha', 'OD', 0),
        ('22', 'Chhattisgarh', 'CG', 0),
        ('23', 'Madhya Pradesh', 'MP', 0),
        ('24', 'Gujarat', 'GJ', 0),
        ('25', 'Daman and Diu', 'DD', 1),
        ('26', 'Dadra and Nagar Haveli', 'DN', 1),
        ('27', 'Maharashtra', 'MH', 0),
        ('29', 'Karnataka', 'KA', 0),
        ('30', 'Goa', 'GA', 0),
        ('31', 'Lakshadweep', 'LD', 1),
        ('32', 'Kerala', 'KL', 0),
        ('33', 'Tamil Nadu', 'TN', 0),
        ('34', 'Puducherry', 'PY', 1),
        ('35', 'Andaman and Nicobar Islands', 'AN', 1),
        ('36', 'Telangana', 'TG', 0),
        ('37', 'Andhra Pradesh', 'AP', 0),
        ('38', 'Ladakh', 'LA', 1);
END
GO

-- Seed GST Slabs
IF NOT EXISTS (SELECT 1 FROM GstSlabs WHERE Name = 'GST Exempt')
BEGIN
    INSERT INTO GstSlabs (Name, Rate, CgstRate, SgstRate, IgstRate, HsnCodes, IsDefault, DisplayOrder)
    VALUES
        ('GST Exempt', 0, 0, 0, 0, NULL, 0, 1),
        ('GST 0%', 0, 0, 0, 0, NULL, 0, 2),
        ('GST 0.25%', 0.25, 0.125, 0.125, 0.25, 'Rough diamonds', 0, 3),
        ('GST 3%', 3, 1.5, 1.5, 3, 'Gold, Silver', 0, 4),
        ('GST 5%', 5, 2.5, 2.5, 5, NULL, 0, 5),
        ('GST 12%', 12, 6, 6, 12, NULL, 0, 6),
        ('GST 18%', 18, 9, 9, 18, NULL, 1, 7),
        ('GST 28%', 28, 14, 14, 28, NULL, 0, 8);
END
GO

PRINT 'Currency, GST and Office Location tables created and seeded successfully';
GO
