-- Seed Financial Test Data for Algora ERP
-- This script creates Chart of Accounts and Journal Entries for testing financial reports
-- Run against the tenant database (e.g., AlgoraErp_Demo)

SET NOCOUNT ON;

-- =============================================
-- CHART OF ACCOUNTS
-- =============================================

-- Assets (AccountType = 0)
INSERT INTO Accounts (Id, Code, Name, AccountType, AccountSubType, Description, IsActive, OpeningBalance, CurrentBalance, CreatedAt, IsDeleted)
VALUES
-- Cash & Bank
(NEWID(), '1000', 'Cash on Hand', 0, 0, 'Petty cash and cash on hand', 1, 5000.00, 5000.00, GETUTCDATE(), 0),
(NEWID(), '1010', 'Operating Bank Account', 0, 1, 'Main operating bank account', 1, 50000.00, 50000.00, GETUTCDATE(), 0),
(NEWID(), '1020', 'Savings Account', 0, 1, 'Business savings account', 1, 25000.00, 25000.00, GETUTCDATE(), 0),

-- Receivables
(NEWID(), '1100', 'Accounts Receivable', 0, 2, 'Customer receivables', 1, 35000.00, 35000.00, GETUTCDATE(), 0),

-- Inventory
(NEWID(), '1200', 'Inventory - Raw Materials', 0, 3, 'Raw materials inventory', 1, 20000.00, 20000.00, GETUTCDATE(), 0),
(NEWID(), '1210', 'Inventory - Finished Goods', 0, 3, 'Finished goods inventory', 1, 45000.00, 45000.00, GETUTCDATE(), 0),

-- Prepaid & Other Current
(NEWID(), '1300', 'Prepaid Insurance', 0, 4, 'Prepaid insurance premiums', 1, 3600.00, 3600.00, GETUTCDATE(), 0),
(NEWID(), '1310', 'Prepaid Rent', 0, 4, 'Prepaid rent deposits', 1, 6000.00, 6000.00, GETUTCDATE(), 0),

-- Fixed Assets
(NEWID(), '1500', 'Furniture & Fixtures', 0, 5, 'Office furniture and fixtures', 1, 15000.00, 15000.00, GETUTCDATE(), 0),
(NEWID(), '1510', 'Computer Equipment', 0, 5, 'Computers and IT equipment', 1, 25000.00, 25000.00, GETUTCDATE(), 0),
(NEWID(), '1520', 'Machinery & Equipment', 0, 5, 'Manufacturing equipment', 1, 80000.00, 80000.00, GETUTCDATE(), 0),
(NEWID(), '1530', 'Vehicles', 0, 5, 'Company vehicles', 1, 35000.00, 35000.00, GETUTCDATE(), 0),
(NEWID(), '1599', 'Accumulated Depreciation', 0, 6, 'Accumulated depreciation on fixed assets', 1, -25000.00, -25000.00, GETUTCDATE(), 0);

-- Liabilities (AccountType = 1)
INSERT INTO Accounts (Id, Code, Name, AccountType, AccountSubType, Description, IsActive, OpeningBalance, CurrentBalance, CreatedAt, IsDeleted)
VALUES
-- Payables
(NEWID(), '2000', 'Accounts Payable', 1, 20, 'Supplier payables', 1, 28000.00, 28000.00, GETUTCDATE(), 0),
(NEWID(), '2010', 'Credit Card Payable', 1, 21, 'Business credit card balance', 1, 5500.00, 5500.00, GETUTCDATE(), 0),

-- Accrued Liabilities
(NEWID(), '2100', 'Accrued Salaries', 1, 22, 'Accrued wages and salaries', 1, 12000.00, 12000.00, GETUTCDATE(), 0),
(NEWID(), '2110', 'Accrued Expenses', 1, 22, 'Other accrued expenses', 1, 4500.00, 4500.00, GETUTCDATE(), 0),

-- Tax Liabilities
(NEWID(), '2200', 'Sales Tax Payable', 1, 23, 'Sales tax collected', 1, 3200.00, 3200.00, GETUTCDATE(), 0),
(NEWID(), '2210', 'Payroll Tax Payable', 1, 24, 'Payroll taxes withheld', 1, 8500.00, 8500.00, GETUTCDATE(), 0),

-- Debt
(NEWID(), '2300', 'Short-Term Bank Loan', 1, 25, 'Short-term credit line', 1, 15000.00, 15000.00, GETUTCDATE(), 0),
(NEWID(), '2400', 'Long-Term Bank Loan', 1, 26, 'Term loan payable', 1, 75000.00, 75000.00, GETUTCDATE(), 0),
(NEWID(), '2410', 'Equipment Financing', 1, 26, 'Equipment loan payable', 1, 40000.00, 40000.00, GETUTCDATE(), 0);

-- Equity (AccountType = 2)
INSERT INTO Accounts (Id, Code, Name, AccountType, AccountSubType, Description, IsActive, OpeningBalance, CurrentBalance, CreatedAt, IsDeleted)
VALUES
(NEWID(), '3000', 'Common Stock', 2, 42, 'Common stock issued', 1, 100000.00, 100000.00, GETUTCDATE(), 0),
(NEWID(), '3100', 'Additional Paid-in Capital', 2, 43, 'Capital in excess of par', 1, 50000.00, 50000.00, GETUTCDATE(), 0),
(NEWID(), '3200', 'Retained Earnings', 2, 41, 'Accumulated profits', 1, 45000.00, 45000.00, GETUTCDATE(), 0),
(NEWID(), '3300', 'Owner Drawings', 2, 44, 'Owner withdrawals', 1, 0.00, 0.00, GETUTCDATE(), 0);

-- Revenue (AccountType = 3)
INSERT INTO Accounts (Id, Code, Name, AccountType, AccountSubType, Description, IsActive, OpeningBalance, CurrentBalance, CreatedAt, IsDeleted)
VALUES
(NEWID(), '4000', 'Product Sales', 3, 60, 'Revenue from product sales', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '4100', 'Service Revenue', 3, 61, 'Revenue from services', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '4200', 'Interest Income', 3, 63, 'Interest earned on deposits', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '4900', 'Other Income', 3, 62, 'Miscellaneous income', 1, 0.00, 0.00, GETUTCDATE(), 0);

-- Expenses (AccountType = 4)
INSERT INTO Accounts (Id, Code, Name, AccountType, AccountSubType, Description, IsActive, OpeningBalance, CurrentBalance, CreatedAt, IsDeleted)
VALUES
(NEWID(), '5000', 'Cost of Goods Sold', 4, 80, 'Direct cost of products sold', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5100', 'Salaries & Wages', 4, 81, 'Employee compensation', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5110', 'Payroll Taxes', 4, 81, 'Employer payroll taxes', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5200', 'Rent Expense', 4, 82, 'Office and warehouse rent', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5300', 'Utilities', 4, 83, 'Electric, water, gas', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5400', 'Insurance Expense', 4, 84, 'Business insurance', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5500', 'Depreciation Expense', 4, 85, 'Asset depreciation', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5600', 'Marketing & Advertising', 4, 86, 'Marketing expenses', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5700', 'Office Supplies', 4, 87, 'Office supplies expense', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5800', 'Professional Fees', 4, 88, 'Legal and accounting fees', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '5900', 'Travel & Entertainment', 4, 89, 'Business travel expenses', 1, 0.00, 0.00, GETUTCDATE(), 0),
(NEWID(), '6000', 'Interest Expense', 4, 90, 'Loan interest', 1, 0.00, 0.00, GETUTCDATE(), 0);

PRINT 'Chart of Accounts created successfully.';

-- =============================================
-- JOURNAL ENTRIES
-- Now create sample journal entries for testing
-- =============================================

DECLARE @BankAccountId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1010');
DECLARE @CashId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1000');
DECLARE @ARId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1100');
DECLARE @InventoryRawId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1200');
DECLARE @InventoryFinishedId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1210');
DECLARE @EquipmentId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1520');
DECLARE @ComputerEquipmentId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1510');
DECLARE @AccumDeprecId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1599');
DECLARE @APId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2000');
DECLARE @AccruedSalariesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2100');
DECLARE @AccruedExpensesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2110');
DECLARE @ShortTermDebtId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2300');
DECLARE @LongTermDebtId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2400');
DECLARE @CommonStockId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '3000');
DECLARE @RetainedEarningsId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '3200');
DECLARE @ProductSalesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '4000');
DECLARE @ServiceRevenueId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '4100');
DECLARE @InterestIncomeId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '4200');
DECLARE @COGSId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5000');
DECLARE @SalariesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5100');
DECLARE @RentId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5200');
DECLARE @UtilitiesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5300');
DECLARE @InsuranceId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5400');
DECLARE @DepreciationId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5500');
DECLARE @MarketingId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5600');
DECLARE @OfficeSuppliesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5700');
DECLARE @ProfessionalFeesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5800');
DECLARE @InterestExpenseId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '6000');

-- Variables for journal entries
DECLARE @JournalId UNIQUEIDENTIFIER;
DECLARE @EntryNum INT = 1;

-- =============================================
-- OPERATING ACTIVITIES ENTRIES
-- =============================================

-- JE-001: Sales on account (creates A/R, increases revenue)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -60, GETUTCDATE()), 'Product sales on account - Acme Corp', 'INV-001', 1, 85000.00, 85000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ARId, 'A/R from Acme Corp', 85000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ProductSalesId, 'Product sales revenue', 0.00, 85000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-002: More sales on account
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -55, GETUTCDATE()), 'Product sales on account - Beta Inc', 'INV-002', 1, 62000.00, 62000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ARId, 'A/R from Beta Inc', 62000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ProductSalesId, 'Product sales revenue', 0.00, 62000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-003: Cash collection from customers (reduces A/R)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -45, GETUTCDATE()), 'Customer payment received - Acme Corp', 'PMT-001', 1, 50000.00, 50000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Cash received', 50000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ARId, 'Payment from Acme Corp', 0.00, 50000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-004: Service revenue cash sales
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -40, GETUTCDATE()), 'Service revenue - consulting fees', 'SRV-001', 1, 28000.00, 28000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Cash received for services', 28000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ServiceRevenueId, 'Consulting services', 0.00, 28000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-005: Cost of goods sold
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -50, GETUTCDATE()), 'Cost of goods sold for sales', 'COGS-001', 1, 45000.00, 45000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @COGSId, 'Cost of products sold', 45000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @InventoryFinishedId, 'Inventory reduction', 0.00, 45000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-006: Salary expense (accrued)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -30, GETUTCDATE()), 'Monthly salary expense', 'PAY-001', 1, 35000.00, 35000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @SalariesId, 'Employee salaries', 35000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @AccruedSalariesId, 'Accrued salaries payable', 0.00, 35000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-007: Salary payment (reduces accrued liability)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -25, GETUTCDATE()), 'Salary payment', 'PAY-002', 1, 35000.00, 35000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @AccruedSalariesId, 'Payment of accrued salaries', 35000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 35000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-008: Rent expense
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -28, GETUTCDATE()), 'Monthly rent payment', 'RENT-001', 1, 8000.00, 8000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @RentId, 'Office rent expense', 8000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Rent payment', 0.00, 8000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-009: Utilities expense
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -20, GETUTCDATE()), 'Utilities payment', 'UTIL-001', 1, 2500.00, 2500.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @UtilitiesId, 'Electric and water', 2500.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Utilities payment', 0.00, 2500.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-010: Depreciation expense (non-cash)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -15, GETUTCDATE()), 'Monthly depreciation', 'DEP-001', 1, 3500.00, 3500.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @DepreciationId, 'Depreciation expense', 3500.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @AccumDeprecId, 'Accumulated depreciation', 0.00, 3500.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-011: Marketing expense
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -18, GETUTCDATE()), 'Digital marketing campaign', 'MKT-001', 1, 5000.00, 5000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @MarketingId, 'Marketing expense', 5000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Marketing payment', 0.00, 5000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-012: Purchase inventory on account (increases A/P)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -35, GETUTCDATE()), 'Inventory purchase on account', 'PO-001', 1, 22000.00, 22000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @InventoryRawId, 'Raw materials purchase', 22000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @APId, 'A/P to supplier', 0.00, 22000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-013: Pay suppliers (reduces A/P)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -12, GETUTCDATE()), 'Payment to suppliers', 'PMT-002', 1, 18000.00, 18000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @APId, 'Pay supplier balance', 18000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 18000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-014: Interest income
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -10, GETUTCDATE()), 'Bank interest earned', 'INT-001', 1, 350.00, 350.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Interest received', 350.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @InterestIncomeId, 'Interest income', 0.00, 350.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- INVESTING ACTIVITIES ENTRIES
-- =============================================

-- JE-015: Purchase new equipment (fixed asset)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -42, GETUTCDATE()), 'Purchase manufacturing equipment', 'FA-001', 1, 45000.00, 45000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @EquipmentId, 'New CNC machine', 45000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Equipment purchase', 0.00, 45000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-016: Purchase computers
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -38, GETUTCDATE()), 'Purchase computer equipment', 'FA-002', 1, 12000.00, 12000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ComputerEquipmentId, 'New workstations', 12000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Computer purchase', 0.00, 12000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- FINANCING ACTIVITIES ENTRIES
-- =============================================

-- JE-017: Borrow from bank (short-term)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -50, GETUTCDATE()), 'Short-term bank loan received', 'LOAN-001', 1, 25000.00, 25000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Loan proceeds', 25000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ShortTermDebtId, 'Short-term loan payable', 0.00, 25000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-018: Repay short-term loan
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -8, GETUTCDATE()), 'Short-term loan repayment', 'LOAN-002', 1, 10000.00, 10000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ShortTermDebtId, 'Loan repayment', 10000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 10000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-019: Long-term loan repayment
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -5, GETUTCDATE()), 'Monthly loan payment', 'LOAN-003', 1, 5000.00, 5000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @LongTermDebtId, 'Principal payment', 5000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 5000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-020: Interest expense on loans
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -5, GETUTCDATE()), 'Loan interest payment', 'INT-002', 1, 1800.00, 1800.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @InterestExpenseId, 'Interest on loans', 1800.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Interest payment', 0.00, 1800.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-021: Owner investment (equity)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -55, GETUTCDATE()), 'Additional owner investment', 'EQ-001', 1, 50000.00, 50000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Owner contribution', 50000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @CommonStockId, 'Common stock issued', 0.00, 50000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-022: More recent sales
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -3, GETUTCDATE()), 'Product sales - Gamma LLC', 'INV-003', 1, 38000.00, 38000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ARId, 'A/R from Gamma LLC', 38000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ProductSalesId, 'Product sales revenue', 0.00, 38000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-023: COGS for recent sales
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -3, GETUTCDATE()), 'COGS for Gamma LLC order', 'COGS-002', 1, 19000.00, 19000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @COGSId, 'Cost of products sold', 19000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @InventoryFinishedId, 'Inventory reduction', 0.00, 19000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

PRINT 'Journal entries created successfully.';
PRINT 'Total journal entries: ' + CAST(@EntryNum - 1 AS VARCHAR);

-- Summary
SELECT
    'Accounts' AS Entity,
    COUNT(*) AS RecordCount
FROM Accounts
UNION ALL
SELECT
    'Journal Entries' AS Entity,
    COUNT(*) AS RecordCount
FROM JournalEntries
UNION ALL
SELECT
    'Journal Entry Lines' AS Entity,
    COUNT(*) AS RecordCount
FROM JournalEntryLines;
