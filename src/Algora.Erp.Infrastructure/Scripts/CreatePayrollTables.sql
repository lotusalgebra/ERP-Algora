SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- PAYROLL TABLES
-- Run this script in the tenant database
-- =============================================

-- Salary Components (earnings, deductions, taxes)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SalaryComponents' AND xtype='U')
BEGIN
    CREATE TABLE SalaryComponents (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        ComponentType INT NOT NULL DEFAULT 0, -- 0=Earning, 1=Deduction, 2=Reimbursement, 3=Tax
        CalculationType INT NOT NULL DEFAULT 0, -- 0=Fixed, 1=PercentOfBasic, 2=PercentOfGross, 3=Formula
        DefaultValue DECIMAL(18,2) DEFAULT 0,
        MinValue DECIMAL(18,2),
        MaxValue DECIMAL(18,2),
        IsTaxable BIT DEFAULT 1,
        IsRecurring BIT DEFAULT 1,
        IsActive BIT DEFAULT 1,
        SortOrder INT DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );

    CREATE UNIQUE INDEX IX_SalaryComponents_Code ON SalaryComponents(Code) WHERE IsDeleted = 0;
END
GO

-- Salary Structures (templates for salary packages)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SalaryStructures' AND xtype='U')
BEGIN
    CREATE TABLE SalaryStructures (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Code NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        BaseSalary DECIMAL(18,2) DEFAULT 0,
        Currency NVARCHAR(3) DEFAULT 'USD',
        PayFrequency INT NOT NULL DEFAULT 3, -- 0=Weekly, 1=BiWeekly, 2=SemiMonthly, 3=Monthly
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );

    CREATE UNIQUE INDEX IX_SalaryStructures_Code ON SalaryStructures(Code) WHERE IsDeleted = 0;
END
GO

-- Salary Structure Lines (components in a salary structure)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SalaryStructureLines' AND xtype='U')
BEGIN
    CREATE TABLE SalaryStructureLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        SalaryStructureId UNIQUEIDENTIFIER NOT NULL,
        SalaryComponentId UNIQUEIDENTIFIER NOT NULL,
        CalculationType INT NOT NULL DEFAULT 0,
        Value DECIMAL(18,4) DEFAULT 0,
        Amount DECIMAL(18,2) DEFAULT 0,
        SortOrder INT DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_SalaryStructureLines_Structure FOREIGN KEY (SalaryStructureId) REFERENCES SalaryStructures(Id) ON DELETE CASCADE,
        CONSTRAINT FK_SalaryStructureLines_Component FOREIGN KEY (SalaryComponentId) REFERENCES SalaryComponents(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_SalaryStructureLines_Unique ON SalaryStructureLines(SalaryStructureId, SalaryComponentId) WHERE IsDeleted = 0;
END
GO

-- Payroll Runs (payroll periods)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PayrollRuns' AND xtype='U')
BEGIN
    CREATE TABLE PayrollRuns (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        RunNumber NVARCHAR(20) NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        PeriodStart DATE NOT NULL,
        PeriodEnd DATE NOT NULL,
        PayDate DATE NOT NULL,
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Processing, 2=Processed, 3=Approved, 4=Paid, 5=Cancelled
        PayFrequency INT NOT NULL DEFAULT 3,
        Currency NVARCHAR(3) DEFAULT 'USD',
        EmployeeCount INT DEFAULT 0,
        TotalGrossPay DECIMAL(18,2) DEFAULT 0,
        TotalDeductions DECIMAL(18,2) DEFAULT 0,
        TotalNetPay DECIMAL(18,2) DEFAULT 0,
        TotalEmployerCosts DECIMAL(18,2) DEFAULT 0,
        ProcessedAt DATETIME2,
        ProcessedBy UNIQUEIDENTIFIER,
        ApprovedAt DATETIME2,
        ApprovedBy UNIQUEIDENTIFIER,
        Notes NVARCHAR(1000),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER
    );

    CREATE UNIQUE INDEX IX_PayrollRuns_RunNumber ON PayrollRuns(RunNumber) WHERE IsDeleted = 0;
    CREATE INDEX IX_PayrollRuns_Period ON PayrollRuns(PeriodStart, PeriodEnd);
END
GO

-- Payslips (individual employee payslips)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Payslips' AND xtype='U')
BEGIN
    CREATE TABLE Payslips (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        PayslipNumber NVARCHAR(20) NOT NULL,
        PayrollRunId UNIQUEIDENTIFIER NOT NULL,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        PeriodStart DATE NOT NULL,
        PeriodEnd DATE NOT NULL,
        PayDate DATE NOT NULL,
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Processed, 2=Approved, 3=Paid, 4=Cancelled
        WorkingDays DECIMAL(5,1) DEFAULT 0,
        DaysWorked DECIMAL(5,1) DEFAULT 0,
        LeavesTaken DECIMAL(5,1) DEFAULT 0,
        Overtime DECIMAL(5,1) DEFAULT 0,
        BasicSalary DECIMAL(18,2) DEFAULT 0,
        GrossPay DECIMAL(18,2) DEFAULT 0,
        TotalEarnings DECIMAL(18,2) DEFAULT 0,
        TotalDeductions DECIMAL(18,2) DEFAULT 0,
        TaxableAmount DECIMAL(18,2) DEFAULT 0,
        TaxAmount DECIMAL(18,2) DEFAULT 0,
        NetPay DECIMAL(18,2) DEFAULT 0,
        PaymentMethod NVARCHAR(50),
        BankAccountNumber NVARCHAR(50),
        BankName NVARCHAR(100),
        PaidAt DATETIME2,
        TransactionReference NVARCHAR(100),
        Notes NVARCHAR(1000),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_Payslips_PayrollRun FOREIGN KEY (PayrollRunId) REFERENCES PayrollRuns(Id) ON DELETE CASCADE,
        CONSTRAINT FK_Payslips_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE NO ACTION
    );

    CREATE UNIQUE INDEX IX_Payslips_PayslipNumber ON Payslips(PayslipNumber) WHERE IsDeleted = 0;
    CREATE UNIQUE INDEX IX_Payslips_RunEmployee ON Payslips(PayrollRunId, EmployeeId) WHERE IsDeleted = 0;
END
GO

-- Payslip Lines (earnings/deductions breakdown)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PayslipLines' AND xtype='U')
BEGIN
    CREATE TABLE PayslipLines (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        PayslipId UNIQUEIDENTIFIER NOT NULL,
        SalaryComponentId UNIQUEIDENTIFIER,
        ComponentCode NVARCHAR(20) NOT NULL,
        ComponentName NVARCHAR(100) NOT NULL,
        ComponentType INT NOT NULL DEFAULT 0,
        CalculationType INT NOT NULL DEFAULT 0,
        Value DECIMAL(18,4) DEFAULT 0,
        Amount DECIMAL(18,2) DEFAULT 0,
        IsTaxable BIT DEFAULT 1,
        SortOrder INT DEFAULT 0,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_PayslipLines_Payslip FOREIGN KEY (PayslipId) REFERENCES Payslips(Id) ON DELETE CASCADE,
        CONSTRAINT FK_PayslipLines_Component FOREIGN KEY (SalaryComponentId) REFERENCES SalaryComponents(Id) ON DELETE SET NULL
    );
END
GO

-- =============================================
-- SEED DEFAULT SALARY COMPONENTS
-- =============================================

-- Earnings
IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'BASIC')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, IsTaxable, IsRecurring, SortOrder)
    VALUES ('BASIC', 'Basic Salary', 'Base salary component', 0, 0, 1, 1, 1);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'HRA')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder)
    VALUES ('HRA', 'House Rent Allowance', 'Housing allowance', 0, 1, 40, 1, 1, 2);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'CONVEY')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder)
    VALUES ('CONVEY', 'Conveyance Allowance', 'Transportation allowance', 0, 0, 200, 0, 1, 3);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'MEDICAL')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder)
    VALUES ('MEDICAL', 'Medical Allowance', 'Health care allowance', 0, 0, 150, 0, 1, 4);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'MEAL')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder)
    VALUES ('MEAL', 'Meal Allowance', 'Food allowance', 0, 0, 100, 0, 1, 5);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'BONUS')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, IsTaxable, IsRecurring, SortOrder)
    VALUES ('BONUS', 'Bonus', 'Performance bonus', 0, 0, 1, 0, 6);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'OT')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, IsTaxable, IsRecurring, SortOrder)
    VALUES ('OT', 'Overtime Pay', 'Overtime compensation', 0, 0, 1, 0, 7);

-- Deductions
IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'PF')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder)
    VALUES ('PF', 'Provident Fund', 'Retirement savings', 1, 1, 12, 0, 1, 10);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'HEALTH')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder)
    VALUES ('HEALTH', 'Health Insurance', 'Medical insurance premium', 1, 0, 100, 0, 1, 11);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'PROF_TAX')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, DefaultValue, IsTaxable, IsRecurring, SortOrder)
    VALUES ('PROF_TAX', 'Professional Tax', 'State professional tax', 1, 0, 200, 0, 1, 12);

IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'LOAN')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, IsTaxable, IsRecurring, SortOrder)
    VALUES ('LOAN', 'Loan Deduction', 'Employee loan repayment', 1, 0, 0, 0, 13);

-- Tax
IF NOT EXISTS (SELECT 1 FROM SalaryComponents WHERE Code = 'INCOME_TAX')
    INSERT INTO SalaryComponents (Code, Name, Description, ComponentType, CalculationType, IsTaxable, IsRecurring, SortOrder)
    VALUES ('INCOME_TAX', 'Income Tax', 'Federal/State income tax', 3, 0, 0, 1, 20);

PRINT 'Payroll tables and default data created successfully';
GO
