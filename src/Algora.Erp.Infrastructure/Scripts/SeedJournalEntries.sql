-- Seed Journal Entry Test Data for Algora ERP
-- Uses existing Chart of Accounts
-- Run against AlgoraErp_Dev database

SET NOCOUNT ON;

-- Delete existing journal entries first
DELETE FROM JournalEntryLines;
DELETE FROM JournalEntries;
PRINT 'Cleared existing journal entries.';

-- Get Account IDs
DECLARE @BankAccountId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1120');
DECLARE @CashId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1110');
DECLARE @ARId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1200');
DECLARE @InventoryId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1300');
DECLARE @EquipmentId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1510');
DECLARE @AccumDeprecId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '1590');
DECLARE @APId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2100');
DECLARE @AccruedLiabilitiesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2300');
DECLARE @ShortTermDebtId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2600');
DECLARE @LongTermDebtId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '2700');
DECLARE @OwnersEquityId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '3100');
DECLARE @RetainedEarningsId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '3200');
DECLARE @SalesRevenueId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '4100');
DECLARE @ServiceRevenueId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '4200');
DECLARE @InterestIncomeId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '4400');
DECLARE @COGSId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '5000');
DECLARE @SalariesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '6100');
DECLARE @RentId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '6200');
DECLARE @UtilitiesId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '6300');
DECLARE @DepreciationId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '6500');
DECLARE @MarketingId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '6600');
DECLARE @InterestExpenseId UNIQUEIDENTIFIER = (SELECT Id FROM Accounts WHERE Code = '7000');

-- Verify accounts found
IF @BankAccountId IS NULL PRINT 'WARNING: Bank Account not found!';
IF @ARId IS NULL PRINT 'WARNING: AR Account not found!';
IF @SalesRevenueId IS NULL PRINT 'WARNING: Sales Revenue not found!';

DECLARE @JournalId UNIQUEIDENTIFIER;
DECLARE @EntryNum INT = 1;

-- =============================================
-- OPERATING ACTIVITIES - Revenue & Collections
-- =============================================

-- JE-001: Sales on account - Customer A ($85,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -60, GETUTCDATE()), 'Product sales on account - Customer A', 'INV-001', 1, 85000.00, 85000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ARId, 'Accounts Receivable - Customer A', 85000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @SalesRevenueId, 'Product sales revenue', 0.00, 85000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-002: Sales on account - Customer B ($62,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -55, GETUTCDATE()), 'Product sales on account - Customer B', 'INV-002', 1, 62000.00, 62000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ARId, 'Accounts Receivable - Customer B', 62000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @SalesRevenueId, 'Product sales revenue', 0.00, 62000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-003: Sales on account - Customer C ($48,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -45, GETUTCDATE()), 'Product sales on account - Customer C', 'INV-003', 1, 48000.00, 48000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ARId, 'Accounts Receivable - Customer C', 48000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @SalesRevenueId, 'Product sales revenue', 0.00, 48000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-004: Cash collection from Customer A ($50,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -40, GETUTCDATE()), 'Customer payment - Customer A', 'PMT-001', 1, 50000.00, 50000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Cash received', 50000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ARId, 'Payment from Customer A', 0.00, 50000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-005: Cash collection from Customer B ($62,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -30, GETUTCDATE()), 'Customer payment - Customer B', 'PMT-002', 1, 62000.00, 62000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Cash received', 62000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ARId, 'Payment from Customer B', 0.00, 62000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-006: Service revenue - cash sales ($28,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -35, GETUTCDATE()), 'Service revenue - consulting', 'SRV-001', 1, 28000.00, 28000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Cash received for services', 28000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ServiceRevenueId, 'Consulting services', 0.00, 28000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- OPERATING ACTIVITIES - Cost of Goods Sold
-- =============================================

-- JE-007: COGS for sales ($95,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -50, GETUTCDATE()), 'Cost of goods sold', 'COGS-001', 1, 95000.00, 95000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @COGSId, 'Cost of products sold', 95000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @InventoryId, 'Inventory reduction', 0.00, 95000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- OPERATING ACTIVITIES - Expenses
-- =============================================

-- JE-008: Salary expense ($35,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -25, GETUTCDATE()), 'Monthly salary expense', 'PAY-001', 1, 35000.00, 35000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @SalariesId, 'Employee salaries', 35000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @AccruedLiabilitiesId, 'Accrued salaries payable', 0.00, 35000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-009: Pay salaries ($35,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -20, GETUTCDATE()), 'Salary payment', 'PAY-002', 1, 35000.00, 35000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @AccruedLiabilitiesId, 'Pay accrued salaries', 35000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 35000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-010: Rent expense ($8,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -28, GETUTCDATE()), 'Monthly rent payment', 'RENT-001', 1, 8000.00, 8000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @RentId, 'Office rent expense', 8000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Rent payment', 0.00, 8000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-011: Utilities ($2,500)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -18, GETUTCDATE()), 'Utilities payment', 'UTIL-001', 1, 2500.00, 2500.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @UtilitiesId, 'Electric and water', 2500.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Utilities payment', 0.00, 2500.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-012: Marketing ($5,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -15, GETUTCDATE()), 'Marketing campaign', 'MKT-001', 1, 5000.00, 5000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @MarketingId, 'Digital marketing', 5000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Marketing payment', 0.00, 5000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-013: Depreciation (NON-CASH) ($4,500)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -10, GETUTCDATE()), 'Monthly depreciation', 'DEP-001', 1, 4500.00, 4500.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @DepreciationId, 'Depreciation expense', 4500.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @AccumDeprecId, 'Accumulated depreciation', 0.00, 4500.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- OPERATING ACTIVITIES - Inventory & Payables
-- =============================================

-- JE-014: Buy inventory on account ($28,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -42, GETUTCDATE()), 'Inventory purchase on account', 'PO-001', 1, 28000.00, 28000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @InventoryId, 'Inventory purchase', 28000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @APId, 'Accounts Payable', 0.00, 28000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-015: Pay suppliers ($20,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -12, GETUTCDATE()), 'Payment to suppliers', 'PMT-003', 1, 20000.00, 20000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @APId, 'Pay supplier', 20000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 20000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-016: Interest income ($450)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -8, GETUTCDATE()), 'Bank interest earned', 'INT-001', 1, 450.00, 450.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Interest received', 450.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @InterestIncomeId, 'Interest income', 0.00, 450.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- INVESTING ACTIVITIES
-- =============================================

-- JE-017: Purchase equipment ($45,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -48, GETUTCDATE()), 'Purchase manufacturing equipment', 'FA-001', 1, 45000.00, 45000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @EquipmentId, 'New CNC machine', 45000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Equipment purchase', 0.00, 45000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-018: Purchase computers ($12,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -38, GETUTCDATE()), 'Purchase computer equipment', 'FA-002', 1, 12000.00, 12000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @EquipmentId, 'New workstations', 12000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Computer purchase', 0.00, 12000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- FINANCING ACTIVITIES
-- =============================================

-- JE-019: Borrow short-term ($30,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -52, GETUTCDATE()), 'Short-term bank loan', 'LOAN-001', 1, 30000.00, 30000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Loan proceeds', 30000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @ShortTermDebtId, 'Short-term loan payable', 0.00, 30000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-020: Repay short-term loan ($10,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -6, GETUTCDATE()), 'Loan repayment', 'LOAN-002', 1, 10000.00, 10000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ShortTermDebtId, 'Loan repayment', 10000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 10000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-021: Long-term loan repayment ($5,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -5, GETUTCDATE()), 'Long-term loan payment', 'LOAN-003', 1, 5000.00, 5000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @LongTermDebtId, 'Principal payment', 5000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Cash payment', 0.00, 5000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-022: Interest expense ($2,100)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -5, GETUTCDATE()), 'Loan interest payment', 'INT-002', 1, 2100.00, 2100.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @InterestExpenseId, 'Interest expense', 2100.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @BankAccountId, 'Interest payment', 0.00, 2100.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-023: Owner investment ($60,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -58, GETUTCDATE()), 'Owner capital contribution', 'EQ-001', 1, 60000.00, 60000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @BankAccountId, 'Owner investment', 60000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @OwnersEquityId, 'Owner equity contribution', 0.00, 60000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- =============================================
-- Recent Activity for Current Period
-- =============================================

-- JE-024: Recent sales ($42,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -2, GETUTCDATE()), 'Product sales - Customer D', 'INV-004', 1, 42000.00, 42000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @ARId, 'Accounts Receivable - Customer D', 42000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @SalesRevenueId, 'Product sales revenue', 0.00, 42000.00, 2, GETUTCDATE(), 0);
SET @EntryNum = @EntryNum + 1;

-- JE-025: COGS for recent sales ($21,000)
SET @JournalId = NEWID();
INSERT INTO JournalEntries (Id, EntryNumber, EntryDate, Description, Reference, Status, TotalDebit, TotalCredit, CreatedAt, IsDeleted)
VALUES (@JournalId, 'JE-' + RIGHT('000' + CAST(@EntryNum AS VARCHAR), 4), DATEADD(DAY, -2, GETUTCDATE()), 'COGS for Customer D', 'COGS-002', 1, 21000.00, 21000.00, GETUTCDATE(), 0);
INSERT INTO JournalEntryLines (Id, JournalEntryId, AccountId, Description, DebitAmount, CreditAmount, LineNumber, CreatedAt, IsDeleted)
VALUES
(NEWID(), @JournalId, @COGSId, 'Cost of goods sold', 21000.00, 0.00, 1, GETUTCDATE(), 0),
(NEWID(), @JournalId, @InventoryId, 'Inventory reduction', 0.00, 21000.00, 2, GETUTCDATE(), 0);

PRINT 'Journal entries created successfully!';
PRINT 'Total entries: ' + CAST(@EntryNum AS VARCHAR);

-- Summary
SELECT 'Summary' AS [Report];
SELECT
    (SELECT COUNT(*) FROM JournalEntries) AS [Journal Entries],
    (SELECT COUNT(*) FROM JournalEntryLines) AS [Journal Lines],
    (SELECT SUM(TotalDebit) FROM JournalEntries) AS [Total Debits],
    (SELECT SUM(TotalCredit) FROM JournalEntries) AS [Total Credits];
