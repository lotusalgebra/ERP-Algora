-- Create TenantSettings table for storing configurable tenant-level settings
-- Run this on each tenant database

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TenantSettings')
BEGIN
    CREATE TABLE TenantSettings (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),

        -- Company Information
        CompanyName NVARCHAR(200) NOT NULL DEFAULT 'My Company',
        CompanyLogo NVARCHAR(500) NULL,
        CompanyTagline NVARCHAR(500) NULL,
        CompanyWebsite NVARCHAR(200) NULL,
        CompanyEmail NVARCHAR(200) NULL,
        CompanyPhone NVARCHAR(50) NULL,

        -- Address
        AddressLine1 NVARCHAR(200) NULL,
        AddressLine2 NVARCHAR(200) NULL,
        City NVARCHAR(100) NULL,
        [State] NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,
        Country NVARCHAR(100) NOT NULL DEFAULT 'India',
        CountryCode NVARCHAR(10) NOT NULL DEFAULT 'IN',

        -- Regional Settings
        Currency NVARCHAR(10) NOT NULL DEFAULT 'INR',
        CurrencySymbol NVARCHAR(10) NOT NULL DEFAULT N'₹',
        CurrencyName NVARCHAR(50) NOT NULL DEFAULT 'Indian Rupee',
        CurrencyDecimalPlaces INT NOT NULL DEFAULT 2,
        [Language] NVARCHAR(20) NOT NULL DEFAULT 'en-IN',
        TimeZone NVARCHAR(50) NOT NULL DEFAULT 'Asia/Kolkata',
        DateFormat NVARCHAR(20) NOT NULL DEFAULT 'dd/MM/yyyy',
        TimeFormat NVARCHAR(20) NOT NULL DEFAULT 'HH:mm',
        DateTimeFormat NVARCHAR(50) NOT NULL DEFAULT 'dd/MM/yyyy HH:mm',

        -- Tax Settings
        TaxId NVARCHAR(50) NULL, -- GSTIN for India
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
        InvoiceHeaderText NVARCHAR(500) NULL,
        InvoiceFooterText NVARCHAR(500) DEFAULT 'Thank you for your business!',
        InvoiceTermsText NVARCHAR(MAX) NULL,
        ShowGstBreakdown BIT NOT NULL DEFAULT 1,
        ShowHsnCode BIT NOT NULL DEFAULT 1,

        -- Email Settings (SMTP)
        SmtpHost NVARCHAR(200) NULL,
        SmtpPort INT NOT NULL DEFAULT 587,
        SmtpUsername NVARCHAR(200) NULL,
        SmtpPassword NVARCHAR(500) NULL,
        SmtpEnableSsl BIT NOT NULL DEFAULT 1,
        EmailFromAddress NVARCHAR(200) NULL,
        EmailFromName NVARCHAR(200) NULL,

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

        -- Audit
        CreatedBy NVARCHAR(100) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ModifiedBy NVARCHAR(100) NULL,
        ModifiedAt DATETIME2 NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy NVARCHAR(100) NULL
    );

    PRINT 'Created TenantSettings table';

    -- Insert default settings
    INSERT INTO TenantSettings (
        Id, CompanyName, Country, CountryCode, Currency, CurrencySymbol, CurrencyName,
        [Language], TimeZone, DateFormat, TaxIdLabel, IsGstRegistered
    )
    VALUES (
        NEWID(), 'My Company', 'India', 'IN', 'INR', N'₹', 'Indian Rupee',
        'en-IN', 'Asia/Kolkata', 'dd/MM/yyyy', 'GSTIN', 1
    );

    PRINT 'Inserted default settings';
END
ELSE
BEGIN
    PRINT 'TenantSettings table already exists';
END
