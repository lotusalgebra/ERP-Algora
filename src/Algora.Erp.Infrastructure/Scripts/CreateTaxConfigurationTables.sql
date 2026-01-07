-- =============================================
-- Tax Configuration Tables for Multi-Country Support
-- Supports: GST (India, Australia, Singapore), VAT (UK, EU, UAE),
--           Sales Tax (USA), HST/PST (Canada), and Custom tax systems
-- =============================================

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Table: TaxConfigurations
-- Tenant-level tax configuration
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TaxConfigurations' AND xtype='U')
BEGIN
    CREATE TABLE TaxConfigurations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),

        -- Basic Info
        Name NVARCHAR(100) NOT NULL DEFAULT 'Default Tax Configuration',

        -- Country Settings
        CountryCode NVARCHAR(10) NOT NULL DEFAULT 'IN',
        CountryName NVARCHAR(100) NOT NULL DEFAULT 'India',

        -- Tax System
        TaxSystem INT NOT NULL DEFAULT 1, -- 0=None, 1=GST, 2=VAT, 3=SalesTax, 4=HST, 5=GST_PST, 6=Consumption, 99=Custom
        TaxSystemName NVARCHAR(100) NOT NULL DEFAULT 'GST',

        -- Tax ID Format
        TaxIdLabel NVARCHAR(50) NOT NULL DEFAULT 'GSTIN',
        TaxIdFormat NVARCHAR(500) NULL,
        TaxIdExample NVARCHAR(100) NULL,

        -- Regional Tax Settings
        HasRegionalTax BIT NOT NULL DEFAULT 1,
        RegionLabel NVARCHAR(50) NOT NULL DEFAULT 'State',
        HasInterRegionalTax BIT NOT NULL DEFAULT 1,

        -- Tax Labels
        CentralTaxLabel NVARCHAR(50) NULL DEFAULT 'CGST',
        RegionalTaxLabel NVARCHAR(50) NULL DEFAULT 'SGST',
        InterRegionalTaxLabel NVARCHAR(50) NULL DEFAULT 'IGST',
        CombinedTaxLabel NVARCHAR(50) NULL DEFAULT 'GST',

        -- Product/Service Code Labels
        ProductCodeLabel NVARCHAR(50) NULL DEFAULT 'HSN Code',
        ServiceCodeLabel NVARCHAR(50) NULL DEFAULT 'SAC Code',

        -- Calculation Settings
        CalculationMethod INT NOT NULL DEFAULT 0, -- 0=Exclusive, 1=Inclusive
        DecimalPlaces INT NOT NULL DEFAULT 2,
        RoundAtLineLevel BIT NOT NULL DEFAULT 1,

        -- Currency
        DefaultCurrencyCode NVARCHAR(10) NOT NULL DEFAULT 'INR',
        DefaultCurrencySymbol NVARCHAR(10) NOT NULL DEFAULT N'₹',

        -- Status
        IsDefault BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,

        -- Audit Fields
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    PRINT 'Created TaxConfigurations table';
END
GO

-- =============================================
-- Table: TaxSlabs
-- Tax rate slabs (replaces GstSlabs for multi-country support)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TaxSlabs' AND xtype='U')
BEGIN
    CREATE TABLE TaxSlabs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TaxConfigurationId UNIQUEIDENTIFIER NOT NULL,

        -- Basic Info
        Name NVARCHAR(100) NOT NULL,
        Code NVARCHAR(20) NULL,
        Description NVARCHAR(500) NULL,

        -- Tax Rates
        Rate DECIMAL(10, 4) NOT NULL DEFAULT 0,
        CentralRate DECIMAL(10, 4) NOT NULL DEFAULT 0,
        RegionalRate DECIMAL(10, 4) NOT NULL DEFAULT 0,
        InterRegionalRate DECIMAL(10, 4) NOT NULL DEFAULT 0,

        -- Product/Service Codes
        ApplicableCodes NVARCHAR(MAX) NULL,

        -- Flags
        IsDefault BIT NOT NULL DEFAULT 0,
        IsZeroRated BIT NOT NULL DEFAULT 0,
        IsExempt BIT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,

        -- Audit Fields
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_TaxSlabs_TaxConfiguration FOREIGN KEY (TaxConfigurationId)
            REFERENCES TaxConfigurations(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_TaxSlabs_TaxConfigurationId ON TaxSlabs(TaxConfigurationId);
    CREATE INDEX IX_TaxSlabs_IsActive ON TaxSlabs(IsActive) WHERE IsActive = 1 AND IsDeleted = 0;

    PRINT 'Created TaxSlabs table';
END
GO

-- =============================================
-- Table: TaxRegions
-- Tax regions (states, provinces, territories)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TaxRegions' AND xtype='U')
BEGIN
    CREATE TABLE TaxRegions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TaxConfigurationId UNIQUEIDENTIFIER NOT NULL,

        -- Basic Info
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        ShortName NVARCHAR(20) NULL,

        -- Region-Specific Tax Override
        RegionalTaxRate DECIMAL(10, 4) NULL,

        -- Additional Info
        IsUnionTerritory BIT NOT NULL DEFAULT 0,
        HasLocalTax BIT NOT NULL DEFAULT 0,
        LocalTaxRate DECIMAL(10, 4) NULL,

        -- Status
        IsActive BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,

        -- Audit Fields
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_TaxRegions_TaxConfiguration FOREIGN KEY (TaxConfigurationId)
            REFERENCES TaxConfigurations(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_TaxRegions_TaxConfigurationId ON TaxRegions(TaxConfigurationId);
    CREATE INDEX IX_TaxRegions_Code ON TaxRegions(Code);

    PRINT 'Created TaxRegions table';
END
GO

-- =============================================
-- Seed Default India GST Configuration
-- =============================================
DECLARE @ConfigId UNIQUEIDENTIFIER = NEWID();

-- Only insert if no configurations exist
IF NOT EXISTS (SELECT 1 FROM TaxConfigurations)
BEGIN
    INSERT INTO TaxConfigurations (
        Id, Name, CountryCode, CountryName, TaxSystem, TaxSystemName,
        TaxIdLabel, TaxIdFormat, TaxIdExample,
        HasRegionalTax, RegionLabel, HasInterRegionalTax,
        CentralTaxLabel, RegionalTaxLabel, InterRegionalTaxLabel, CombinedTaxLabel,
        ProductCodeLabel, ServiceCodeLabel,
        DefaultCurrencyCode, DefaultCurrencySymbol,
        IsDefault, IsActive
    )
    VALUES (
        @ConfigId, 'India GST', 'IN', 'India', 1, 'Goods and Services Tax',
        'GSTIN', '^\d{2}[A-Z]{5}\d{4}[A-Z]{1}[A-Z\d]{1}[Z]{1}[A-Z\d]{1}$', '27AABCU9603R1ZM',
        1, 'State', 1,
        'CGST', 'SGST', 'IGST', 'GST',
        'HSN Code', 'SAC Code',
        'INR', N'₹',
        1, 1
    );

    -- Insert GST Slabs
    INSERT INTO TaxSlabs (TaxConfigurationId, Name, Code, Rate, CentralRate, RegionalRate, InterRegionalRate, IsZeroRated, IsDefault, DisplayOrder)
    VALUES
        (@ConfigId, 'GST 0%', 'GST0', 0, 0, 0, 0, 1, 0, 1),
        (@ConfigId, 'GST 5%', 'GST5', 5, 2.5, 2.5, 5, 0, 0, 2),
        (@ConfigId, 'GST 12%', 'GST12', 12, 6, 6, 12, 0, 0, 3),
        (@ConfigId, 'GST 18%', 'GST18', 18, 9, 9, 18, 0, 1, 4),
        (@ConfigId, 'GST 28%', 'GST28', 28, 14, 14, 28, 0, 0, 5),
        (@ConfigId, 'Exempt', 'EXEMPT', 0, 0, 0, 0, 0, 0, 6);

    -- Insert Indian States as Tax Regions (from existing IndianStates table if available)
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'IndianStates')
    BEGIN
        INSERT INTO TaxRegions (TaxConfigurationId, Code, Name, ShortName, IsUnionTerritory, DisplayOrder)
        SELECT @ConfigId, Code, Name, ShortName, IsUnionTerritory, ROW_NUMBER() OVER (ORDER BY Name)
        FROM IndianStates
        WHERE IsActive = 1;
    END
    ELSE
    BEGIN
        -- Insert common Indian states manually
        INSERT INTO TaxRegions (TaxConfigurationId, Code, Name, ShortName, IsUnionTerritory, DisplayOrder)
        VALUES
            (@ConfigId, '01', 'Jammu and Kashmir', 'JK', 1, 1),
            (@ConfigId, '02', 'Himachal Pradesh', 'HP', 0, 2),
            (@ConfigId, '03', 'Punjab', 'PB', 0, 3),
            (@ConfigId, '04', 'Chandigarh', 'CH', 1, 4),
            (@ConfigId, '05', 'Uttarakhand', 'UK', 0, 5),
            (@ConfigId, '06', 'Haryana', 'HR', 0, 6),
            (@ConfigId, '07', 'Delhi', 'DL', 1, 7),
            (@ConfigId, '08', 'Rajasthan', 'RJ', 0, 8),
            (@ConfigId, '09', 'Uttar Pradesh', 'UP', 0, 9),
            (@ConfigId, '10', 'Bihar', 'BR', 0, 10),
            (@ConfigId, '11', 'Sikkim', 'SK', 0, 11),
            (@ConfigId, '12', 'Arunachal Pradesh', 'AR', 0, 12),
            (@ConfigId, '13', 'Nagaland', 'NL', 0, 13),
            (@ConfigId, '14', 'Manipur', 'MN', 0, 14),
            (@ConfigId, '15', 'Mizoram', 'MZ', 0, 15),
            (@ConfigId, '16', 'Tripura', 'TR', 0, 16),
            (@ConfigId, '17', 'Meghalaya', 'ML', 0, 17),
            (@ConfigId, '18', 'Assam', 'AS', 0, 18),
            (@ConfigId, '19', 'West Bengal', 'WB', 0, 19),
            (@ConfigId, '20', 'Jharkhand', 'JH', 0, 20),
            (@ConfigId, '21', 'Odisha', 'OD', 0, 21),
            (@ConfigId, '22', 'Chhattisgarh', 'CG', 0, 22),
            (@ConfigId, '23', 'Madhya Pradesh', 'MP', 0, 23),
            (@ConfigId, '24', 'Gujarat', 'GJ', 0, 24),
            (@ConfigId, '26', 'Dadra and Nagar Haveli and Daman and Diu', 'DD', 1, 25),
            (@ConfigId, '27', 'Maharashtra', 'MH', 0, 26),
            (@ConfigId, '29', 'Karnataka', 'KA', 0, 27),
            (@ConfigId, '30', 'Goa', 'GA', 0, 28),
            (@ConfigId, '31', 'Lakshadweep', 'LD', 1, 29),
            (@ConfigId, '32', 'Kerala', 'KL', 0, 30),
            (@ConfigId, '33', 'Tamil Nadu', 'TN', 0, 31),
            (@ConfigId, '34', 'Puducherry', 'PY', 1, 32),
            (@ConfigId, '35', 'Andaman and Nicobar Islands', 'AN', 1, 33),
            (@ConfigId, '36', 'Telangana', 'TS', 0, 34),
            (@ConfigId, '37', 'Andhra Pradesh', 'AP', 0, 35),
            (@ConfigId, '38', 'Ladakh', 'LA', 1, 36);
    END

    PRINT 'Seeded India GST configuration with tax slabs and regions';
END
GO

PRINT 'Tax Configuration tables created successfully';
GO
