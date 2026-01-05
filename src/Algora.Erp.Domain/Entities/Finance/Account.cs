using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Finance;

public class Account : AuditableEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public AccountType AccountType { get; set; }
    public AccountSubType? AccountSubType { get; set; }

    public Guid? ParentAccountId { get; set; }
    public Account? ParentAccount { get; set; }
    public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();

    public decimal OpeningBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public string Currency { get; set; } = "USD";

    public bool IsActive { get; set; } = true;
    public bool IsSystemAccount { get; set; }
    public bool AllowDirectPosting { get; set; } = true;

    public int DisplayOrder { get; set; }

    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}

public enum AccountType
{
    Asset = 0,
    Liability = 1,
    Equity = 2,
    Revenue = 3,
    Expense = 4
}

public enum AccountSubType
{
    // Assets
    Cash = 0,
    Bank = 1,
    AccountsReceivable = 2,
    Inventory = 3,
    PrepaidExpenses = 4,
    FixedAssets = 5,
    AccumulatedDepreciation = 6,
    OtherCurrentAssets = 7,
    OtherNonCurrentAssets = 8,

    // Liabilities
    AccountsPayable = 20,
    CreditCard = 21,
    AccruedLiabilities = 22,
    SalesTaxPayable = 23,
    PayrollLiabilities = 24,
    ShortTermDebt = 25,
    LongTermDebt = 26,
    OtherCurrentLiabilities = 27,
    OtherNonCurrentLiabilities = 28,

    // Equity
    OwnersEquity = 40,
    RetainedEarnings = 41,
    CommonStock = 42,
    AdditionalPaidInCapital = 43,
    Drawings = 44,

    // Revenue
    Sales = 60,
    ServiceRevenue = 61,
    OtherIncome = 62,
    InterestIncome = 63,
    DiscountsGiven = 64,

    // Expenses
    CostOfGoodsSold = 80,
    Salaries = 81,
    Rent = 82,
    Utilities = 83,
    Insurance = 84,
    Depreciation = 85,
    Marketing = 86,
    OfficeSupplies = 87,
    ProfessionalFees = 88,
    TravelExpenses = 89,
    InterestExpense = 90,
    TaxExpense = 91,
    OtherExpenses = 92
}
