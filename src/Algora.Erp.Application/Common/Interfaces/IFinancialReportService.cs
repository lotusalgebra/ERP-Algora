using Algora.Erp.Domain.Entities.Finance;

namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Service for generating financial reports
/// </summary>
public interface IFinancialReportService
{
    /// <summary>
    /// Gets invoice summary report
    /// </summary>
    Task<InvoiceSummaryReport> GetInvoiceSummaryAsync(ReportDateRange range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment summary report
    /// </summary>
    Task<PaymentSummaryReport> GetPaymentSummaryAsync(ReportDateRange range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets accounts receivable aging report
    /// </summary>
    Task<AgingReport> GetAgingReportAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets revenue by customer report
    /// </summary>
    Task<List<CustomerRevenueItem>> GetRevenueByCustomerAsync(ReportDateRange range, int topN = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets monthly revenue trend
    /// </summary>
    Task<List<MonthlyRevenueTrend>> GetMonthlyRevenueTrendAsync(int months = 12, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payment collection efficiency
    /// </summary>
    Task<CollectionEfficiencyReport> GetCollectionEfficiencyAsync(ReportDateRange range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cash flow report
    /// </summary>
    Task<CashFlowReport> GetCashFlowReportAsync(ReportDateRange range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets profit and loss statement
    /// </summary>
    Task<ProfitAndLossReport> GetProfitAndLossAsync(ReportDateRange range, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports report to PDF
    /// </summary>
    byte[] ExportToPdf<T>(T report, string reportTitle) where T : class;

    /// <summary>
    /// Exports report data to CSV
    /// </summary>
    byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class;
}

#region Report Models

public class ReportDateRange
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public static ReportDateRange ThisMonth()
    {
        var today = DateTime.Today;
        return new ReportDateRange
        {
            StartDate = new DateTime(today.Year, today.Month, 1),
            EndDate = today
        };
    }

    public static ReportDateRange LastMonth()
    {
        var today = DateTime.Today;
        var firstOfThisMonth = new DateTime(today.Year, today.Month, 1);
        return new ReportDateRange
        {
            StartDate = firstOfThisMonth.AddMonths(-1),
            EndDate = firstOfThisMonth.AddDays(-1)
        };
    }

    public static ReportDateRange ThisQuarter()
    {
        var today = DateTime.Today;
        var quarter = (today.Month - 1) / 3;
        var startMonth = quarter * 3 + 1;
        return new ReportDateRange
        {
            StartDate = new DateTime(today.Year, startMonth, 1),
            EndDate = today
        };
    }

    public static ReportDateRange ThisYear()
    {
        var today = DateTime.Today;
        return new ReportDateRange
        {
            StartDate = new DateTime(today.Year, 1, 1),
            EndDate = today
        };
    }

    public static ReportDateRange LastYear()
    {
        var today = DateTime.Today;
        return new ReportDateRange
        {
            StartDate = new DateTime(today.Year - 1, 1, 1),
            EndDate = new DateTime(today.Year - 1, 12, 31)
        };
    }

    public static ReportDateRange Custom(DateTime start, DateTime end) => new() { StartDate = start, EndDate = end };
}

public class InvoiceSummaryReport
{
    public ReportDateRange DateRange { get; set; } = new();
    public int TotalInvoices { get; set; }
    public int DraftInvoices { get; set; }
    public int SentInvoices { get; set; }
    public int PaidInvoices { get; set; }
    public int PartiallyPaidInvoices { get; set; }
    public int OverdueInvoices { get; set; }
    public int VoidedInvoices { get; set; }

    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalOverdue { get; set; }
    public decimal AverageInvoiceAmount { get; set; }

    public List<InvoiceStatusBreakdown> ByStatus { get; set; } = new();
    public List<DailyInvoiceSummary> DailyTrend { get; set; } = new();
}

public class InvoiceStatusBreakdown
{
    public InvoiceStatus Status { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class DailyInvoiceSummary
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class PaymentSummaryReport
{
    public ReportDateRange DateRange { get; set; } = new();
    public int TotalPayments { get; set; }
    public decimal TotalReceived { get; set; }
    public decimal AveragePayment { get; set; }
    public decimal LargestPayment { get; set; }
    public decimal SmallestPayment { get; set; }

    public List<PaymentMethodBreakdown> ByMethod { get; set; } = new();
    public List<DailyPaymentSummary> DailyTrend { get; set; } = new();
    public List<WeeklyPaymentSummary> WeeklyTrend { get; set; } = new();
}

public class PaymentMethodBreakdown
{
    public PaymentMethod Method { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class WeeklyPaymentSummary
{
    public DateTime WeekStart { get; set; }
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public class AgingReport
{
    public DateTime AsOfDate { get; set; } = DateTime.Today;
    public decimal TotalOutstanding { get; set; }

    public AgingBucket Current { get; set; } = new();
    public AgingBucket Days1To30 { get; set; } = new();
    public AgingBucket Days31To60 { get; set; } = new();
    public AgingBucket Days61To90 { get; set; } = new();
    public AgingBucket Over90Days { get; set; } = new();

    public List<CustomerAgingItem> ByCustomer { get; set; } = new();
}

public class AgingBucket
{
    public string Label { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
}

public class CustomerAgingItem
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Days1To30 { get; set; }
    public decimal Days31To60 { get; set; }
    public decimal Days61To90 { get; set; }
    public decimal Over90Days { get; set; }
    public decimal Total { get; set; }
}

public class CustomerRevenueItem
{
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public int InvoiceCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal Percentage { get; set; }
}

public class MonthlyRevenueTrend
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Invoiced { get; set; }
    public decimal Collected { get; set; }
    public int InvoiceCount { get; set; }
    public int PaymentCount { get; set; }
}

public class CollectionEfficiencyReport
{
    public ReportDateRange DateRange { get; set; } = new();
    public decimal TotalInvoiced { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal CollectionRate { get; set; }
    public double AverageDaysToPayment { get; set; }
    public int InvoicesPaidOnTime { get; set; }
    public int InvoicesPaidLate { get; set; }
    public decimal OnTimePaymentRate { get; set; }

    public List<CollectionByMonth> MonthlyTrend { get; set; } = new();
}

public class CollectionByMonth
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Invoiced { get; set; }
    public decimal Collected { get; set; }
    public decimal CollectionRate { get; set; }
}

public class CashFlowReport
{
    public ReportDateRange DateRange { get; set; } = new();
    public decimal TotalInflow { get; set; }
    public decimal ProjectedInflow { get; set; }
    public decimal OverdueReceivables { get; set; }

    public List<DailyCashFlow> DailyFlow { get; set; } = new();
    public List<WeeklyCashFlow> WeeklyFlow { get; set; } = new();
}

public class DailyCashFlow
{
    public DateTime Date { get; set; }
    public decimal Inflow { get; set; }
    public decimal CumulativeInflow { get; set; }
}

public class WeeklyCashFlow
{
    public DateTime WeekStart { get; set; }
    public decimal Inflow { get; set; }
    public decimal ProjectedInflow { get; set; }
}

public class ProfitAndLossReport
{
    public ReportDateRange DateRange { get; set; } = new();

    // Revenue Section
    public decimal TotalRevenue { get; set; }
    public List<PnLLineItem> RevenueItems { get; set; } = new();

    // Cost of Goods Sold Section
    public decimal TotalCOGS { get; set; }
    public List<PnLLineItem> COGSItems { get; set; } = new();

    // Gross Profit
    public decimal GrossProfit { get; set; }
    public decimal GrossProfitMargin { get; set; }

    // Operating Expenses Section
    public decimal TotalOperatingExpenses { get; set; }
    public List<PnLLineItem> OperatingExpenseItems { get; set; } = new();

    // Operating Income
    public decimal OperatingIncome { get; set; }
    public decimal OperatingMargin { get; set; }

    // Other Income/Expenses
    public decimal OtherIncome { get; set; }
    public decimal OtherExpenses { get; set; }
    public List<PnLLineItem> OtherIncomeItems { get; set; } = new();
    public List<PnLLineItem> OtherExpenseItems { get; set; } = new();

    // Net Income
    public decimal NetIncome { get; set; }
    public decimal NetProfitMargin { get; set; }

    // Comparative Data
    public PnLComparison? PreviousPeriod { get; set; }
    public List<MonthlyPnL> MonthlyTrend { get; set; } = new();
}

public class PnLLineItem
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public decimal? PreviousAmount { get; set; }
    public decimal? ChangePercent { get; set; }
}

public class PnLComparison
{
    public ReportDateRange DateRange { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal OperatingIncome { get; set; }
    public decimal NetIncome { get; set; }

    public decimal RevenueChange { get; set; }
    public decimal GrossProfitChange { get; set; }
    public decimal OperatingIncomeChange { get; set; }
    public decimal NetIncomeChange { get; set; }
}

public class MonthlyPnL
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthName { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal COGS { get; set; }
    public decimal GrossProfit { get; set; }
    public decimal OperatingExpenses { get; set; }
    public decimal NetIncome { get; set; }
}

#endregion
