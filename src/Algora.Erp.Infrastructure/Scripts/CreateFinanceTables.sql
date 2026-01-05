-- Algora ERP Finance Tables
-- Run this script to add Finance tables to tenant database

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- FINANCE TABLES
-- =============================================

-- Accounts table (Chart of Accounts)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Accounts' AND xtype='U')
BEGIN
    CREATE TABLE Accounts (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        [Description] NVARCHAR(500) NULL,
        AccountType INT NOT NULL DEFAULT 0, -- 0=Asset, 1=Liability, 2=Equity, 3=Revenue, 4=Expense
        AccountSubType INT NULL,
        ParentAccountId UNIQUEIDENTIFIER NULL,
        OpeningBalance DECIMAL(18, 2) NOT NULL DEFAULT 0,
        CurrentBalance DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
        IsActive BIT NOT NULL DEFAULT 1,
        IsSystemAccount BIT NOT NULL DEFAULT 0,
        AllowDirectPosting BIT NOT NULL DEFAULT 1,
        DisplayOrder INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_Accounts_Code UNIQUE (Code),
        CONSTRAINT FK_Accounts_ParentAccount FOREIGN KEY (ParentAccountId) REFERENCES Accounts(Id)
    );

    CREATE INDEX IX_Accounts_Code ON Accounts(Code);
    CREATE INDEX IX_Accounts_AccountType ON Accounts(AccountType);
    CREATE INDEX IX_Accounts_IsDeleted ON Accounts(IsDeleted);

    PRINT 'Created Accounts table';
END
GO

-- JournalEntries table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='JournalEntries' AND xtype='U')
BEGIN
    CREATE TABLE JournalEntries (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        EntryNumber NVARCHAR(20) NOT NULL,
        EntryDate DATE NOT NULL,
        Reference NVARCHAR(100) NULL,
        [Description] NVARCHAR(500) NOT NULL,
        EntryType INT NOT NULL DEFAULT 0, -- 0=General, 1=Sales, 2=Purchase, 3=CashReceipt, 4=CashPayment, 5=Payroll, 6=Adjustment, 7=Closing, 8=Opening
        [Status] INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Pending, 2=Posted, 3=Reversed, 4=Void
        TotalDebit DECIMAL(18, 2) NOT NULL DEFAULT 0,
        TotalCredit DECIMAL(18, 2) NOT NULL DEFAULT 0,
        Currency NVARCHAR(3) NOT NULL DEFAULT 'USD',
        IsAdjusting BIT NOT NULL DEFAULT 0,
        IsClosing BIT NOT NULL DEFAULT 0,
        IsReversing BIT NOT NULL DEFAULT 0,
        ReversingEntryId UNIQUEIDENTIFIER NULL,
        PostedBy UNIQUEIDENTIFIER NULL,
        PostedAt DATETIME2 NULL,
        Notes NVARCHAR(1000) NULL,
        AttachmentUrl NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT UQ_JournalEntries_EntryNumber UNIQUE (EntryNumber)
    );

    CREATE INDEX IX_JournalEntries_EntryNumber ON JournalEntries(EntryNumber);
    CREATE INDEX IX_JournalEntries_EntryDate ON JournalEntries(EntryDate);
    CREATE INDEX IX_JournalEntries_Status ON JournalEntries([Status]);
    CREATE INDEX IX_JournalEntries_IsDeleted ON JournalEntries(IsDeleted);

    PRINT 'Created JournalEntries table';
END
GO

-- JournalEntryLines table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='JournalEntryLines' AND xtype='U')
BEGIN
    CREATE TABLE JournalEntryLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWSEQUENTIALID(),
        JournalEntryId UNIQUEIDENTIFIER NOT NULL,
        AccountId UNIQUEIDENTIFIER NOT NULL,
        [Description] NVARCHAR(500) NULL,
        DebitAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
        CreditAmount DECIMAL(18, 2) NOT NULL DEFAULT 0,
        LineNumber INT NOT NULL DEFAULT 0,
        SourceDocumentType NVARCHAR(100) NULL,
        SourceDocumentId UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        IsDeleted BIT NOT NULL DEFAULT 0,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL,
        CONSTRAINT FK_JournalEntryLines_JournalEntries FOREIGN KEY (JournalEntryId) REFERENCES JournalEntries(Id) ON DELETE CASCADE,
        CONSTRAINT FK_JournalEntryLines_Accounts FOREIGN KEY (AccountId) REFERENCES Accounts(Id)
    );

    CREATE INDEX IX_JournalEntryLines_JournalEntryId ON JournalEntryLines(JournalEntryId);
    CREATE INDEX IX_JournalEntryLines_AccountId ON JournalEntryLines(AccountId);
    CREATE INDEX IX_JournalEntryLines_IsDeleted ON JournalEntryLines(IsDeleted);

    PRINT 'Created JournalEntryLines table';
END
GO

-- =============================================
-- SEED DEFAULT CHART OF ACCOUNTS
-- =============================================

IF NOT EXISTS (SELECT 1 FROM Accounts WHERE Code = '1000')
BEGIN
    -- ASSETS (1xxx)
    INSERT INTO Accounts (Code, Name, AccountType, AccountSubType, IsSystemAccount, DisplayOrder)
    VALUES
        ('1000', 'Assets', 0, NULL, 1, 1),
        ('1100', 'Cash and Cash Equivalents', 0, 0, 1, 2),
        ('1110', 'Petty Cash', 0, 0, 0, 3),
        ('1120', 'Operating Bank Account', 0, 1, 0, 4),
        ('1130', 'Savings Account', 0, 1, 0, 5),
        ('1200', 'Accounts Receivable', 0, 2, 1, 6),
        ('1300', 'Inventory', 0, 3, 1, 7),
        ('1400', 'Prepaid Expenses', 0, 4, 0, 8),
        ('1500', 'Fixed Assets', 0, 5, 1, 9),
        ('1510', 'Equipment', 0, 5, 0, 10),
        ('1520', 'Furniture & Fixtures', 0, 5, 0, 11),
        ('1530', 'Vehicles', 0, 5, 0, 12),
        ('1590', 'Accumulated Depreciation', 0, 6, 1, 13);

    -- LIABILITIES (2xxx)
    INSERT INTO Accounts (Code, Name, AccountType, AccountSubType, IsSystemAccount, DisplayOrder)
    VALUES
        ('2000', 'Liabilities', 1, NULL, 1, 20),
        ('2100', 'Accounts Payable', 1, 20, 1, 21),
        ('2200', 'Credit Cards Payable', 1, 21, 0, 22),
        ('2300', 'Accrued Liabilities', 1, 22, 0, 23),
        ('2400', 'Sales Tax Payable', 1, 23, 1, 24),
        ('2500', 'Payroll Liabilities', 1, 24, 1, 25),
        ('2600', 'Short-term Loans', 1, 25, 0, 26),
        ('2700', 'Long-term Loans', 1, 26, 0, 27);

    -- EQUITY (3xxx)
    INSERT INTO Accounts (Code, Name, AccountType, AccountSubType, IsSystemAccount, DisplayOrder)
    VALUES
        ('3000', 'Equity', 2, NULL, 1, 30),
        ('3100', 'Owner''s Equity', 2, 40, 1, 31),
        ('3200', 'Retained Earnings', 2, 41, 1, 32),
        ('3300', 'Owner''s Drawings', 2, 44, 0, 33);

    -- REVENUE (4xxx)
    INSERT INTO Accounts (Code, Name, AccountType, AccountSubType, IsSystemAccount, DisplayOrder)
    VALUES
        ('4000', 'Revenue', 3, NULL, 1, 40),
        ('4100', 'Sales Revenue', 3, 60, 1, 41),
        ('4200', 'Service Revenue', 3, 61, 0, 42),
        ('4300', 'Other Income', 3, 62, 0, 43),
        ('4400', 'Interest Income', 3, 63, 0, 44),
        ('4900', 'Discounts Given', 3, 64, 0, 45);

    -- EXPENSES (5xxx-6xxx)
    INSERT INTO Accounts (Code, Name, AccountType, AccountSubType, IsSystemAccount, DisplayOrder)
    VALUES
        ('5000', 'Cost of Goods Sold', 4, 80, 1, 50),
        ('6000', 'Operating Expenses', 4, NULL, 1, 60),
        ('6100', 'Salaries & Wages', 4, 81, 1, 61),
        ('6200', 'Rent Expense', 4, 82, 0, 62),
        ('6300', 'Utilities', 4, 83, 0, 63),
        ('6400', 'Insurance', 4, 84, 0, 64),
        ('6500', 'Depreciation', 4, 85, 1, 65),
        ('6600', 'Marketing & Advertising', 4, 86, 0, 66),
        ('6700', 'Office Supplies', 4, 87, 0, 67),
        ('6800', 'Professional Fees', 4, 88, 0, 68),
        ('6900', 'Travel Expenses', 4, 89, 0, 69),
        ('7000', 'Interest Expense', 4, 90, 0, 70),
        ('7100', 'Tax Expense', 4, 91, 0, 71),
        ('7900', 'Other Expenses', 4, 92, 0, 72);

    -- Set parent accounts
    UPDATE Accounts SET ParentAccountId = (SELECT Id FROM Accounts WHERE Code = '1000') WHERE Code LIKE '1_00' AND Code != '1000';
    UPDATE Accounts SET ParentAccountId = (SELECT Id FROM Accounts WHERE Code = '1100') WHERE Code IN ('1110', '1120', '1130');
    UPDATE Accounts SET ParentAccountId = (SELECT Id FROM Accounts WHERE Code = '1500') WHERE Code IN ('1510', '1520', '1530', '1590');
    UPDATE Accounts SET ParentAccountId = (SELECT Id FROM Accounts WHERE Code = '2000') WHERE Code LIKE '2_00' AND Code != '2000';
    UPDATE Accounts SET ParentAccountId = (SELECT Id FROM Accounts WHERE Code = '3000') WHERE Code LIKE '3_00' AND Code != '3000';
    UPDATE Accounts SET ParentAccountId = (SELECT Id FROM Accounts WHERE Code = '4000') WHERE Code LIKE '4_00' AND Code != '4000';
    UPDATE Accounts SET ParentAccountId = (SELECT Id FROM Accounts WHERE Code = '6000') WHERE Code LIKE '6_00' AND Code != '6000';

    PRINT 'Seeded default Chart of Accounts';
END
GO

PRINT 'Finance tables created successfully';
GO
