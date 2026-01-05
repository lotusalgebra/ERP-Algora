using System.Globalization;
using System.Reflection;
using System.Text;
using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Algora.Erp.Infrastructure.Services;

/// <summary>
/// Service for generating financial reports
/// </summary>
public class FinancialReportService : IFinancialReportService
{
    private readonly IApplicationDbContext _context;

    public FinancialReportService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<InvoiceSummaryReport> GetInvoiceSummaryAsync(ReportDateRange range, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= range.StartDate && i.InvoiceDate <= range.EndDate)
            .ToListAsync(cancellationToken);

        var report = new InvoiceSummaryReport
        {
            DateRange = range,
            TotalInvoices = invoices.Count,
            DraftInvoices = invoices.Count(i => i.Status == InvoiceStatus.Draft),
            SentInvoices = invoices.Count(i => i.Status == InvoiceStatus.Sent),
            PaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            PartiallyPaidInvoices = invoices.Count(i => i.Status == InvoiceStatus.PartiallyPaid),
            OverdueInvoices = invoices.Count(i => i.Status == InvoiceStatus.Overdue ||
                (i.DueDate < DateTime.Today && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void)),
            VoidedInvoices = invoices.Count(i => i.Status == InvoiceStatus.Void || i.Status == InvoiceStatus.Cancelled),
            TotalInvoiced = invoices.Where(i => i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled).Sum(i => i.TotalAmount),
            TotalPaid = invoices.Sum(i => i.PaidAmount),
            TotalOutstanding = invoices.Where(i => i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void).Sum(i => i.BalanceDue),
            TotalOverdue = invoices.Where(i => i.DueDate < DateTime.Today && i.Status != InvoiceStatus.Paid && i.Status != InvoiceStatus.Void).Sum(i => i.BalanceDue)
        };

        report.AverageInvoiceAmount = report.TotalInvoices > 0 ? report.TotalInvoiced / report.TotalInvoices : 0;

        // Status breakdown
        report.ByStatus = Enum.GetValues<InvoiceStatus>()
            .Select(status =>
            {
                var statusInvoices = invoices.Where(i => i.Status == status).ToList();
                var amount = statusInvoices.Sum(i => i.TotalAmount);
                return new InvoiceStatusBreakdown
                {
                    Status = status,
                    Count = statusInvoices.Count,
                    Amount = amount,
                    Percentage = report.TotalInvoiced > 0 ? (amount / report.TotalInvoiced) * 100 : 0
                };
            })
            .Where(b => b.Count > 0)
            .OrderByDescending(b => b.Amount)
            .ToList();

        // Daily trend
        report.DailyTrend = invoices
            .GroupBy(i => i.InvoiceDate.Date)
            .Select(g => new DailyInvoiceSummary
            {
                Date = g.Key,
                Count = g.Count(),
                Amount = g.Sum(i => i.TotalAmount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        return report;
    }

    public async Task<PaymentSummaryReport> GetPaymentSummaryAsync(ReportDateRange range, CancellationToken cancellationToken = default)
    {
        var payments = await _context.InvoicePayments
            .Include(p => p.Invoice)
            .Where(p => p.PaymentDate >= range.StartDate && p.PaymentDate <= range.EndDate)
            .ToListAsync(cancellationToken);

        var report = new PaymentSummaryReport
        {
            DateRange = range,
            TotalPayments = payments.Count,
            TotalReceived = payments.Sum(p => p.Amount),
            AveragePayment = payments.Count > 0 ? payments.Average(p => p.Amount) : 0,
            LargestPayment = payments.Count > 0 ? payments.Max(p => p.Amount) : 0,
            SmallestPayment = payments.Count > 0 ? payments.Min(p => p.Amount) : 0
        };

        // By payment method
        report.ByMethod = Enum.GetValues<PaymentMethod>()
            .Select(method =>
            {
                var methodPayments = payments.Where(p => p.PaymentMethod == method).ToList();
                var amount = methodPayments.Sum(p => p.Amount);
                return new PaymentMethodBreakdown
                {
                    Method = method,
                    Count = methodPayments.Count,
                    Amount = amount,
                    Percentage = report.TotalReceived > 0 ? (amount / report.TotalReceived) * 100 : 0
                };
            })
            .Where(b => b.Count > 0)
            .OrderByDescending(b => b.Amount)
            .ToList();

        // Daily trend
        report.DailyTrend = payments
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new DailyPaymentSummary
            {
                Date = g.Key,
                Count = g.Count(),
                Amount = g.Sum(p => p.Amount)
            })
            .OrderBy(d => d.Date)
            .ToList();

        // Weekly trend
        report.WeeklyTrend = payments
            .GroupBy(p => GetWeekStart(p.PaymentDate))
            .Select(g => new WeeklyPaymentSummary
            {
                WeekStart = g.Key,
                Count = g.Count(),
                Amount = g.Sum(p => p.Amount)
            })
            .OrderBy(w => w.WeekStart)
            .ToList();

        return report;
    }

    public async Task<AgingReport> GetAgingReportAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var outstandingInvoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.BalanceDue > 0 && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var report = new AgingReport
        {
            AsOfDate = today,
            TotalOutstanding = outstandingInvoices.Sum(i => i.BalanceDue)
        };

        // Categorize invoices by age
        var current = outstandingInvoices.Where(i => i.DueDate >= today).ToList();
        var days1To30 = outstandingInvoices.Where(i => i.DueDate < today && i.DueDate >= today.AddDays(-30)).ToList();
        var days31To60 = outstandingInvoices.Where(i => i.DueDate < today.AddDays(-30) && i.DueDate >= today.AddDays(-60)).ToList();
        var days61To90 = outstandingInvoices.Where(i => i.DueDate < today.AddDays(-60) && i.DueDate >= today.AddDays(-90)).ToList();
        var over90Days = outstandingInvoices.Where(i => i.DueDate < today.AddDays(-90)).ToList();

        report.Current = CreateAgingBucket("Current", current, report.TotalOutstanding);
        report.Days1To30 = CreateAgingBucket("1-30 Days", days1To30, report.TotalOutstanding);
        report.Days31To60 = CreateAgingBucket("31-60 Days", days31To60, report.TotalOutstanding);
        report.Days61To90 = CreateAgingBucket("61-90 Days", days61To90, report.TotalOutstanding);
        report.Over90Days = CreateAgingBucket("Over 90 Days", over90Days, report.TotalOutstanding);

        // By customer
        report.ByCustomer = outstandingInvoices
            .Where(i => i.Customer != null)
            .GroupBy(i => new { i.CustomerId, i.Customer!.Name })
            .Select(g =>
            {
                var customerInvoices = g.ToList();
                return new CustomerAgingItem
                {
                    CustomerId = g.Key.CustomerId ?? Guid.Empty,
                    CustomerName = g.Key.Name,
                    Current = customerInvoices.Where(i => i.DueDate >= today).Sum(i => i.BalanceDue),
                    Days1To30 = customerInvoices.Where(i => i.DueDate < today && i.DueDate >= today.AddDays(-30)).Sum(i => i.BalanceDue),
                    Days31To60 = customerInvoices.Where(i => i.DueDate < today.AddDays(-30) && i.DueDate >= today.AddDays(-60)).Sum(i => i.BalanceDue),
                    Days61To90 = customerInvoices.Where(i => i.DueDate < today.AddDays(-60) && i.DueDate >= today.AddDays(-90)).Sum(i => i.BalanceDue),
                    Over90Days = customerInvoices.Where(i => i.DueDate < today.AddDays(-90)).Sum(i => i.BalanceDue),
                    Total = customerInvoices.Sum(i => i.BalanceDue)
                };
            })
            .OrderByDescending(c => c.Total)
            .ToList();

        return report;
    }

    public async Task<List<CustomerRevenueItem>> GetRevenueByCustomerAsync(ReportDateRange range, int topN = 10, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Customer)
            .Where(i => i.InvoiceDate >= range.StartDate && i.InvoiceDate <= range.EndDate
                && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled
                && i.CustomerId != null)
            .ToListAsync(cancellationToken);

        var totalRevenue = invoices.Sum(i => i.TotalAmount);

        return invoices
            .GroupBy(i => new { i.CustomerId, i.Customer!.Name, i.Customer.Code })
            .Select(g =>
            {
                var customerInvoices = g.ToList();
                var revenue = customerInvoices.Sum(i => i.TotalAmount);
                return new CustomerRevenueItem
                {
                    CustomerId = g.Key.CustomerId ?? Guid.Empty,
                    CustomerName = g.Key.Name,
                    CustomerCode = g.Key.Code,
                    InvoiceCount = customerInvoices.Count,
                    TotalRevenue = revenue,
                    PaidAmount = customerInvoices.Sum(i => i.PaidAmount),
                    OutstandingAmount = customerInvoices.Sum(i => i.BalanceDue),
                    Percentage = totalRevenue > 0 ? (revenue / totalRevenue) * 100 : 0
                };
            })
            .OrderByDescending(c => c.TotalRevenue)
            .Take(topN)
            .ToList();
    }

    public async Task<List<MonthlyRevenueTrend>> GetMonthlyRevenueTrendAsync(int months = 12, CancellationToken cancellationToken = default)
    {
        var startDate = DateTime.Today.AddMonths(-months + 1);
        startDate = new DateTime(startDate.Year, startDate.Month, 1);

        var invoices = await _context.Invoices
            .Where(i => i.InvoiceDate >= startDate && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var payments = await _context.InvoicePayments
            .Where(p => p.PaymentDate >= startDate)
            .ToListAsync(cancellationToken);

        var result = new List<MonthlyRevenueTrend>();

        for (var i = 0; i < months; i++)
        {
            var monthStart = startDate.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthInvoices = invoices.Where(inv => inv.InvoiceDate >= monthStart && inv.InvoiceDate <= monthEnd).ToList();
            var monthPayments = payments.Where(p => p.PaymentDate >= monthStart && p.PaymentDate <= monthEnd).ToList();

            result.Add(new MonthlyRevenueTrend
            {
                Year = monthStart.Year,
                Month = monthStart.Month,
                MonthName = monthStart.ToString("MMM yyyy"),
                Invoiced = monthInvoices.Sum(inv => inv.TotalAmount),
                Collected = monthPayments.Sum(p => p.Amount),
                InvoiceCount = monthInvoices.Count,
                PaymentCount = monthPayments.Count
            });
        }

        return result;
    }

    public async Task<CollectionEfficiencyReport> GetCollectionEfficiencyAsync(ReportDateRange range, CancellationToken cancellationToken = default)
    {
        var invoices = await _context.Invoices
            .Include(i => i.Payments)
            .Where(i => i.InvoiceDate >= range.StartDate && i.InvoiceDate <= range.EndDate
                && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var paidInvoices = invoices.Where(i => i.Status == InvoiceStatus.Paid && i.PaidDate.HasValue).ToList();

        // Calculate days to payment
        var daysToPayment = paidInvoices
            .Where(i => i.PaidDate.HasValue)
            .Select(i => (i.PaidDate!.Value - i.InvoiceDate).TotalDays)
            .ToList();

        var onTime = paidInvoices.Count(i => i.PaidDate <= i.DueDate);
        var late = paidInvoices.Count(i => i.PaidDate > i.DueDate);

        var report = new CollectionEfficiencyReport
        {
            DateRange = range,
            TotalInvoiced = invoices.Sum(i => i.TotalAmount),
            TotalCollected = invoices.Sum(i => i.PaidAmount),
            AverageDaysToPayment = daysToPayment.Count > 0 ? daysToPayment.Average() : 0,
            InvoicesPaidOnTime = onTime,
            InvoicesPaidLate = late,
            OnTimePaymentRate = paidInvoices.Count > 0 ? ((decimal)onTime / paidInvoices.Count) * 100 : 0
        };

        report.CollectionRate = report.TotalInvoiced > 0 ? (report.TotalCollected / report.TotalInvoiced) * 100 : 0;

        // Monthly trend
        report.MonthlyTrend = invoices
            .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
            .Select(g =>
            {
                var monthInvoiced = g.Sum(i => i.TotalAmount);
                var monthCollected = g.Sum(i => i.PaidAmount);
                return new CollectionByMonth
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                    Invoiced = monthInvoiced,
                    Collected = monthCollected,
                    CollectionRate = monthInvoiced > 0 ? (monthCollected / monthInvoiced) * 100 : 0
                };
            })
            .OrderBy(m => m.Year).ThenBy(m => m.Month)
            .ToList();

        return report;
    }

    public async Task<CashFlowReport> GetCashFlowReportAsync(ReportDateRange range, CancellationToken cancellationToken = default)
    {
        var payments = await _context.InvoicePayments
            .Where(p => p.PaymentDate >= range.StartDate && p.PaymentDate <= range.EndDate)
            .OrderBy(p => p.PaymentDate)
            .ToListAsync(cancellationToken);

        var outstandingInvoices = await _context.Invoices
            .Where(i => i.BalanceDue > 0 && i.DueDate >= range.StartDate && i.DueDate <= range.EndDate
                && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var overdueInvoices = await _context.Invoices
            .Where(i => i.BalanceDue > 0 && i.DueDate < DateTime.Today
                && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var report = new CashFlowReport
        {
            DateRange = range,
            TotalInflow = payments.Sum(p => p.Amount),
            ProjectedInflow = outstandingInvoices.Sum(i => i.BalanceDue),
            OverdueReceivables = overdueInvoices.Sum(i => i.BalanceDue)
        };

        // Daily flow with cumulative
        decimal cumulative = 0;
        report.DailyFlow = payments
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new { Date = g.Key, Amount = g.Sum(p => p.Amount) })
            .OrderBy(d => d.Date)
            .Select(d =>
            {
                cumulative += d.Amount;
                return new DailyCashFlow
                {
                    Date = d.Date,
                    Inflow = d.Amount,
                    CumulativeInflow = cumulative
                };
            })
            .ToList();

        // Weekly flow with projected
        report.WeeklyFlow = payments
            .GroupBy(p => GetWeekStart(p.PaymentDate))
            .Select(g =>
            {
                var weekStart = g.Key;
                var weekEnd = weekStart.AddDays(6);
                var projected = outstandingInvoices
                    .Where(i => i.DueDate >= weekStart && i.DueDate <= weekEnd)
                    .Sum(i => i.BalanceDue);

                return new WeeklyCashFlow
                {
                    WeekStart = weekStart,
                    Inflow = g.Sum(p => p.Amount),
                    ProjectedInflow = projected
                };
            })
            .OrderBy(w => w.WeekStart)
            .ToList();

        return report;
    }

    public async Task<ProfitAndLossReport> GetProfitAndLossAsync(ReportDateRange range, CancellationToken cancellationToken = default)
    {
        // Get all journal entries within the date range
        var journalEntries = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate >= range.StartDate && j.EntryDate <= range.EndDate && j.Status == JournalEntryStatus.Posted)
            .ToListAsync(cancellationToken);

        var allLines = journalEntries.SelectMany(j => j.Lines).ToList();

        // Get invoices for revenue data (if journal entries are limited)
        var invoices = await _context.Invoices
            .Where(i => i.InvoiceDate >= range.StartDate && i.InvoiceDate <= range.EndDate
                && i.Status != InvoiceStatus.Void && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync(cancellationToken);

        var report = new ProfitAndLossReport
        {
            DateRange = range
        };

        // Revenue items from accounts
        var revenueAccounts = allLines
            .Where(l => l.Account.AccountType == AccountType.Revenue)
            .GroupBy(l => l.Account)
            .Select(g => new PnLLineItem
            {
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Category = "Revenue",
                Amount = g.Sum(l => l.CreditAmount - l.DebitAmount) // Revenue increases with credits
            })
            .Where(i => i.Amount != 0)
            .OrderByDescending(i => i.Amount)
            .ToList();

        // If no journal entries, use invoice data for revenue
        if (!revenueAccounts.Any() && invoices.Any())
        {
            revenueAccounts.Add(new PnLLineItem
            {
                AccountCode = "4000",
                AccountName = "Sales Revenue",
                Category = "Revenue",
                Amount = invoices.Sum(i => i.TotalAmount)
            });
        }

        report.RevenueItems = revenueAccounts;
        report.TotalRevenue = revenueAccounts.Sum(i => i.Amount);

        // COGS items
        var cogsAccounts = allLines
            .Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountSubType == AccountSubType.CostOfGoodsSold)
            .GroupBy(l => l.Account)
            .Select(g => new PnLLineItem
            {
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Category = "COGS",
                Amount = g.Sum(l => l.DebitAmount - l.CreditAmount) // Expenses increase with debits
            })
            .Where(i => i.Amount != 0)
            .OrderByDescending(i => i.Amount)
            .ToList();

        report.COGSItems = cogsAccounts;
        report.TotalCOGS = cogsAccounts.Sum(i => i.Amount);

        // Gross Profit
        report.GrossProfit = report.TotalRevenue - report.TotalCOGS;
        report.GrossProfitMargin = report.TotalRevenue != 0 ? (report.GrossProfit / report.TotalRevenue) * 100 : 0;

        // Operating Expenses (all expense accounts except COGS)
        var operatingExpenseAccounts = allLines
            .Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountSubType != AccountSubType.CostOfGoodsSold)
            .GroupBy(l => l.Account)
            .Select(g => new PnLLineItem
            {
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Category = GetExpenseCategory(g.Key.AccountSubType),
                Amount = g.Sum(l => l.DebitAmount - l.CreditAmount)
            })
            .Where(i => i.Amount != 0)
            .OrderByDescending(i => i.Amount)
            .ToList();

        report.OperatingExpenseItems = operatingExpenseAccounts;
        report.TotalOperatingExpenses = operatingExpenseAccounts.Sum(i => i.Amount);

        // Operating Income
        report.OperatingIncome = report.GrossProfit - report.TotalOperatingExpenses;
        report.OperatingMargin = report.TotalRevenue != 0 ? (report.OperatingIncome / report.TotalRevenue) * 100 : 0;

        // Other Income (e.g., interest income, other income accounts)
        var otherIncomeAccounts = allLines
            .Where(l => l.Account.AccountType == AccountType.Revenue &&
                (l.Account.AccountSubType == AccountSubType.OtherIncome || l.Account.AccountSubType == AccountSubType.InterestIncome))
            .GroupBy(l => l.Account)
            .Select(g => new PnLLineItem
            {
                AccountCode = g.Key.Code,
                AccountName = g.Key.Name,
                Category = "Other Income",
                Amount = g.Sum(l => l.CreditAmount - l.DebitAmount)
            })
            .Where(i => i.Amount != 0)
            .ToList();

        report.OtherIncomeItems = otherIncomeAccounts;
        report.OtherIncome = otherIncomeAccounts.Sum(i => i.Amount);

        // Net Income
        report.NetIncome = report.OperatingIncome + report.OtherIncome - report.OtherExpenses;
        report.NetProfitMargin = report.TotalRevenue != 0 ? (report.NetIncome / report.TotalRevenue) * 100 : 0;

        // Calculate percentages for line items
        foreach (var item in report.RevenueItems)
        {
            item.Percentage = report.TotalRevenue != 0 ? (item.Amount / report.TotalRevenue) * 100 : 0;
        }
        foreach (var item in report.OperatingExpenseItems)
        {
            item.Percentage = report.TotalOperatingExpenses != 0 ? (item.Amount / report.TotalOperatingExpenses) * 100 : 0;
        }

        // Monthly trend
        report.MonthlyTrend = await GetMonthlyPnLTrendAsync(range, cancellationToken);

        // Previous period comparison
        var periodLength = (range.EndDate - range.StartDate).Days;
        var previousRange = new ReportDateRange
        {
            StartDate = range.StartDate.AddDays(-periodLength - 1),
            EndDate = range.StartDate.AddDays(-1)
        };

        var previousReport = await GetPreviousPeriodDataAsync(previousRange, cancellationToken);
        if (previousReport.HasValue)
        {
            var prev = previousReport.Value;
            report.PreviousPeriod = new PnLComparison
            {
                DateRange = previousRange,
                TotalRevenue = prev.TotalRevenue,
                GrossProfit = prev.GrossProfit,
                OperatingIncome = prev.OperatingIncome,
                NetIncome = prev.NetIncome,
                RevenueChange = prev.TotalRevenue != 0
                    ? ((report.TotalRevenue - prev.TotalRevenue) / prev.TotalRevenue) * 100
                    : 0,
                GrossProfitChange = prev.GrossProfit != 0
                    ? ((report.GrossProfit - prev.GrossProfit) / Math.Abs(prev.GrossProfit)) * 100
                    : 0,
                OperatingIncomeChange = prev.OperatingIncome != 0
                    ? ((report.OperatingIncome - prev.OperatingIncome) / Math.Abs(prev.OperatingIncome)) * 100
                    : 0,
                NetIncomeChange = prev.NetIncome != 0
                    ? ((report.NetIncome - prev.NetIncome) / Math.Abs(prev.NetIncome)) * 100
                    : 0
            };
        }

        return report;
    }

    public async Task<BalanceSheetReport> GetBalanceSheetAsync(DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        // Get all accounts with their current balances from posted journal entries up to the as-of date
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate <= asOfDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        // Get all accounts
        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        var report = new BalanceSheetReport
        {
            AsOfDate = asOfDate
        };

        // Calculate account balances
        var accountBalances = accounts.Select(account =>
        {
            var lines = journalLines.Where(l => l.AccountId == account.Id).ToList();
            var balance = account.AccountType switch
            {
                // Assets and Expenses increase with debits
                AccountType.Asset or AccountType.Expense =>
                    account.OpeningBalance + lines.Sum(l => l.DebitAmount - l.CreditAmount),
                // Liabilities, Equity, and Revenue increase with credits
                _ => account.OpeningBalance + lines.Sum(l => l.CreditAmount - l.DebitAmount)
            };

            return new
            {
                Account = account,
                Balance = balance
            };
        }).Where(a => a.Balance != 0).ToList();

        // ===== ASSETS =====
        var assetAccounts = accountBalances
            .Where(a => a.Account.AccountType == AccountType.Asset)
            .ToList();

        // Current Assets
        var currentAssetSubTypes = new[]
        {
            AccountSubType.Cash,
            AccountSubType.Bank,
            AccountSubType.AccountsReceivable,
            AccountSubType.Inventory,
            AccountSubType.PrepaidExpenses,
            AccountSubType.OtherCurrentAssets
        };

        var currentAssets = assetAccounts
            .Where(a => currentAssetSubTypes.Contains(a.Account.AccountSubType ?? AccountSubType.OtherCurrentAssets))
            .Select(a => new BalanceSheetLineItem
            {
                AccountCode = a.Account.Code,
                AccountName = a.Account.Name,
                SubType = a.Account.AccountSubType,
                Balance = a.Balance
            })
            .OrderBy(a => a.AccountCode)
            .ToList();

        var currentAssetsSection = new BalanceSheetSection
        {
            SectionName = "Current Assets",
            Items = currentAssets,
            Total = currentAssets.Sum(a => a.Balance)
        };

        // Non-Current Assets
        var nonCurrentAssets = assetAccounts
            .Where(a => !currentAssetSubTypes.Contains(a.Account.AccountSubType ?? AccountSubType.OtherCurrentAssets))
            .Select(a => new BalanceSheetLineItem
            {
                AccountCode = a.Account.Code,
                AccountName = a.Account.Name,
                SubType = a.Account.AccountSubType,
                Balance = a.Balance
            })
            .OrderBy(a => a.AccountCode)
            .ToList();

        var nonCurrentAssetsSection = new BalanceSheetSection
        {
            SectionName = "Non-Current Assets",
            Items = nonCurrentAssets,
            Total = nonCurrentAssets.Sum(a => a.Balance)
        };

        report.AssetSections = new List<BalanceSheetSection> { currentAssetsSection, nonCurrentAssetsSection };
        report.TotalCurrentAssets = currentAssetsSection.Total;
        report.TotalNonCurrentAssets = nonCurrentAssetsSection.Total;
        report.TotalAssets = report.TotalCurrentAssets + report.TotalNonCurrentAssets;

        // ===== LIABILITIES =====
        var liabilityAccounts = accountBalances
            .Where(a => a.Account.AccountType == AccountType.Liability)
            .ToList();

        // Current Liabilities
        var currentLiabilitySubTypes = new[]
        {
            AccountSubType.AccountsPayable,
            AccountSubType.CreditCard,
            AccountSubType.AccruedLiabilities,
            AccountSubType.SalesTaxPayable,
            AccountSubType.PayrollLiabilities,
            AccountSubType.ShortTermDebt,
            AccountSubType.OtherCurrentLiabilities
        };

        var currentLiabilities = liabilityAccounts
            .Where(a => currentLiabilitySubTypes.Contains(a.Account.AccountSubType ?? AccountSubType.OtherCurrentLiabilities))
            .Select(a => new BalanceSheetLineItem
            {
                AccountCode = a.Account.Code,
                AccountName = a.Account.Name,
                SubType = a.Account.AccountSubType,
                Balance = a.Balance
            })
            .OrderBy(a => a.AccountCode)
            .ToList();

        var currentLiabilitiesSection = new BalanceSheetSection
        {
            SectionName = "Current Liabilities",
            Items = currentLiabilities,
            Total = currentLiabilities.Sum(a => a.Balance)
        };

        // Non-Current Liabilities
        var nonCurrentLiabilities = liabilityAccounts
            .Where(a => !currentLiabilitySubTypes.Contains(a.Account.AccountSubType ?? AccountSubType.OtherCurrentLiabilities))
            .Select(a => new BalanceSheetLineItem
            {
                AccountCode = a.Account.Code,
                AccountName = a.Account.Name,
                SubType = a.Account.AccountSubType,
                Balance = a.Balance
            })
            .OrderBy(a => a.AccountCode)
            .ToList();

        var nonCurrentLiabilitiesSection = new BalanceSheetSection
        {
            SectionName = "Non-Current Liabilities",
            Items = nonCurrentLiabilities,
            Total = nonCurrentLiabilities.Sum(a => a.Balance)
        };

        report.LiabilitySections = new List<BalanceSheetSection> { currentLiabilitiesSection, nonCurrentLiabilitiesSection };
        report.TotalCurrentLiabilities = currentLiabilitiesSection.Total;
        report.TotalNonCurrentLiabilities = nonCurrentLiabilitiesSection.Total;
        report.TotalLiabilities = report.TotalCurrentLiabilities + report.TotalNonCurrentLiabilities;

        // ===== EQUITY =====
        var equityAccounts = accountBalances
            .Where(a => a.Account.AccountType == AccountType.Equity)
            .ToList();

        report.EquityItems = equityAccounts
            .Select(a => new BalanceSheetLineItem
            {
                AccountCode = a.Account.Code,
                AccountName = a.Account.Name,
                SubType = a.Account.AccountSubType,
                Balance = a.Balance
            })
            .OrderBy(a => a.AccountCode)
            .ToList();

        // Calculate retained earnings from revenue and expenses
        var revenueTotal = accountBalances
            .Where(a => a.Account.AccountType == AccountType.Revenue)
            .Sum(a => a.Balance);

        var expenseTotal = accountBalances
            .Where(a => a.Account.AccountType == AccountType.Expense)
            .Sum(a => a.Balance);

        var netIncome = revenueTotal - expenseTotal;

        // Add net income to retained earnings
        var retainedEarningsItem = report.EquityItems.FirstOrDefault(e => e.SubType == AccountSubType.RetainedEarnings);
        if (retainedEarningsItem != null)
        {
            retainedEarningsItem.Balance += netIncome;
        }
        else if (netIncome != 0)
        {
            report.EquityItems.Add(new BalanceSheetLineItem
            {
                AccountCode = "3900",
                AccountName = "Retained Earnings (Current Period)",
                SubType = AccountSubType.RetainedEarnings,
                Balance = netIncome
            });
        }

        report.TotalEquity = report.EquityItems.Sum(e => e.Balance);
        report.TotalLiabilitiesAndEquity = report.TotalLiabilities + report.TotalEquity;

        // ===== KEY RATIOS =====
        // Current Ratio = Current Assets / Current Liabilities
        report.CurrentRatio = report.TotalCurrentLiabilities != 0
            ? report.TotalCurrentAssets / report.TotalCurrentLiabilities
            : 0;

        // Quick Ratio = (Current Assets - Inventory) / Current Liabilities
        var inventoryBalance = currentAssets.Where(a => a.SubType == AccountSubType.Inventory).Sum(a => a.Balance);
        report.QuickRatio = report.TotalCurrentLiabilities != 0
            ? (report.TotalCurrentAssets - inventoryBalance) / report.TotalCurrentLiabilities
            : 0;

        // Debt to Equity = Total Liabilities / Total Equity
        report.DebtToEquityRatio = report.TotalEquity != 0
            ? report.TotalLiabilities / report.TotalEquity
            : 0;

        // Working Capital = Current Assets - Current Liabilities
        report.WorkingCapital = report.TotalCurrentAssets - report.TotalCurrentLiabilities;

        // ===== PREVIOUS PERIOD COMPARISON =====
        var previousDate = asOfDate.AddYears(-1);
        var previousBalances = await GetPreviousBalanceSheetDataAsync(previousDate, cancellationToken);
        if (previousBalances.HasValue)
        {
            var prev = previousBalances.Value;
            report.PreviousPeriod = new BalanceSheetComparison
            {
                AsOfDate = previousDate,
                TotalAssets = prev.TotalAssets,
                TotalLiabilities = prev.TotalLiabilities,
                TotalEquity = prev.TotalEquity,
                AssetsChange = prev.TotalAssets != 0
                    ? ((report.TotalAssets - prev.TotalAssets) / prev.TotalAssets) * 100
                    : 0,
                LiabilitiesChange = prev.TotalLiabilities != 0
                    ? ((report.TotalLiabilities - prev.TotalLiabilities) / prev.TotalLiabilities) * 100
                    : 0,
                EquityChange = prev.TotalEquity != 0
                    ? ((report.TotalEquity - prev.TotalEquity) / Math.Abs(prev.TotalEquity)) * 100
                    : 0
            };
        }

        return report;
    }

    public async Task<CashFlowStatementReport> GetCashFlowStatementAsync(ReportDateRange range, CancellationToken cancellationToken = default)
    {
        var report = new CashFlowStatementReport
        {
            DateRange = range
        };

        // Get journal entries for the period
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate >= range.StartDate && j.EntryDate <= range.EndDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        // Get all accounts
        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        // Calculate Net Income (Revenue - Expenses)
        var revenueTotal = journalLines
            .Where(l => l.Account.AccountType == AccountType.Revenue)
            .Sum(l => l.CreditAmount - l.DebitAmount);

        var expenseTotal = journalLines
            .Where(l => l.Account.AccountType == AccountType.Expense)
            .Sum(l => l.DebitAmount - l.CreditAmount);

        report.NetIncome = revenueTotal - expenseTotal;

        // ===== OPERATING ACTIVITIES =====
        // Non-cash adjustments (depreciation, amortization)
        var depreciation = journalLines
            .Where(l => l.Account.AccountSubType == AccountSubType.Depreciation)
            .Sum(l => l.DebitAmount - l.CreditAmount);

        if (depreciation != 0)
        {
            report.OperatingAdjustments.Add(new CashFlowLineItem
            {
                Description = "Depreciation & Amortization",
                Category = CashFlowCategory.OperatingAdjustment,
                Amount = depreciation // Add back (non-cash expense)
            });
        }

        // Changes in working capital
        // Get beginning and ending balances for working capital accounts
        var beginningDate = range.StartDate.AddDays(-1);
        var beginningLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate <= beginningDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        var endingLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate <= range.EndDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        // Accounts Receivable change (increase = cash outflow)
        var arAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.AccountsReceivable).ToList();
        foreach (var arAccount in arAccounts)
        {
            var beginningBalance = arAccount.OpeningBalance + beginningLines.Where(l => l.AccountId == arAccount.Id).Sum(l => l.DebitAmount - l.CreditAmount);
            var endingBalance = arAccount.OpeningBalance + endingLines.Where(l => l.AccountId == arAccount.Id).Sum(l => l.DebitAmount - l.CreditAmount);
            var change = endingBalance - beginningBalance;

            if (change != 0)
            {
                report.WorkingCapitalChanges.Add(new CashFlowLineItem
                {
                    Description = $"Change in {arAccount.Name}",
                    AccountCode = arAccount.Code,
                    Category = CashFlowCategory.WorkingCapitalChange,
                    Amount = -change // Increase in AR = decrease in cash
                });
            }
        }

        // Inventory change (increase = cash outflow)
        var inventoryAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.Inventory).ToList();
        foreach (var invAccount in inventoryAccounts)
        {
            var beginningBalance = invAccount.OpeningBalance + beginningLines.Where(l => l.AccountId == invAccount.Id).Sum(l => l.DebitAmount - l.CreditAmount);
            var endingBalance = invAccount.OpeningBalance + endingLines.Where(l => l.AccountId == invAccount.Id).Sum(l => l.DebitAmount - l.CreditAmount);
            var change = endingBalance - beginningBalance;

            if (change != 0)
            {
                report.WorkingCapitalChanges.Add(new CashFlowLineItem
                {
                    Description = $"Change in {invAccount.Name}",
                    AccountCode = invAccount.Code,
                    Category = CashFlowCategory.WorkingCapitalChange,
                    Amount = -change // Increase in inventory = decrease in cash
                });
            }
        }

        // Accounts Payable change (increase = cash inflow)
        var apAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.AccountsPayable).ToList();
        foreach (var apAccount in apAccounts)
        {
            var beginningBalance = apAccount.OpeningBalance + beginningLines.Where(l => l.AccountId == apAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var endingBalance = apAccount.OpeningBalance + endingLines.Where(l => l.AccountId == apAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var change = endingBalance - beginningBalance;

            if (change != 0)
            {
                report.WorkingCapitalChanges.Add(new CashFlowLineItem
                {
                    Description = $"Change in {apAccount.Name}",
                    AccountCode = apAccount.Code,
                    Category = CashFlowCategory.WorkingCapitalChange,
                    Amount = change // Increase in AP = increase in cash
                });
            }
        }

        // Accrued Liabilities change
        var accruedAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.AccruedLiabilities || a.AccountSubType == AccountSubType.PayrollLiabilities).ToList();
        foreach (var accAccount in accruedAccounts)
        {
            var beginningBalance = accAccount.OpeningBalance + beginningLines.Where(l => l.AccountId == accAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var endingBalance = accAccount.OpeningBalance + endingLines.Where(l => l.AccountId == accAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var change = endingBalance - beginningBalance;

            if (change != 0)
            {
                report.WorkingCapitalChanges.Add(new CashFlowLineItem
                {
                    Description = $"Change in {accAccount.Name}",
                    AccountCode = accAccount.Code,
                    Category = CashFlowCategory.WorkingCapitalChange,
                    Amount = change
                });
            }
        }

        report.NetCashFromOperating = report.NetIncome
            + report.OperatingAdjustments.Sum(a => a.Amount)
            + report.WorkingCapitalChanges.Sum(a => a.Amount);

        // ===== INVESTING ACTIVITIES =====
        // Fixed Assets purchases/sales
        var fixedAssetAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.FixedAssets).ToList();
        foreach (var faAccount in fixedAssetAccounts)
        {
            var beginningBalance = faAccount.OpeningBalance + beginningLines.Where(l => l.AccountId == faAccount.Id).Sum(l => l.DebitAmount - l.CreditAmount);
            var endingBalance = faAccount.OpeningBalance + endingLines.Where(l => l.AccountId == faAccount.Id).Sum(l => l.DebitAmount - l.CreditAmount);
            var change = endingBalance - beginningBalance;

            if (change != 0)
            {
                report.InvestingActivities.Add(new CashFlowLineItem
                {
                    Description = change > 0 ? $"Purchase of {faAccount.Name}" : $"Sale of {faAccount.Name}",
                    AccountCode = faAccount.Code,
                    Category = CashFlowCategory.Investing,
                    Amount = -change // Purchase = cash outflow (negative)
                });
            }
        }

        report.NetCashFromInvesting = report.InvestingActivities.Sum(a => a.Amount);

        // ===== FINANCING ACTIVITIES =====
        // Long-term debt changes
        var debtAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.LongTermDebt || a.AccountSubType == AccountSubType.ShortTermDebt).ToList();
        foreach (var debtAccount in debtAccounts)
        {
            var beginningBalance = debtAccount.OpeningBalance + beginningLines.Where(l => l.AccountId == debtAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var endingBalance = debtAccount.OpeningBalance + endingLines.Where(l => l.AccountId == debtAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var change = endingBalance - beginningBalance;

            if (change != 0)
            {
                report.FinancingActivities.Add(new CashFlowLineItem
                {
                    Description = change > 0 ? $"Proceeds from {debtAccount.Name}" : $"Repayment of {debtAccount.Name}",
                    AccountCode = debtAccount.Code,
                    Category = CashFlowCategory.Financing,
                    Amount = change
                });
            }
        }

        // Equity changes (capital contributions, dividends)
        var equityAccounts = accounts.Where(a => a.AccountType == AccountType.Equity && a.AccountSubType != AccountSubType.RetainedEarnings).ToList();
        foreach (var eqAccount in equityAccounts)
        {
            var beginningBalance = eqAccount.OpeningBalance + beginningLines.Where(l => l.AccountId == eqAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var endingBalance = eqAccount.OpeningBalance + endingLines.Where(l => l.AccountId == eqAccount.Id).Sum(l => l.CreditAmount - l.DebitAmount);
            var change = endingBalance - beginningBalance;

            if (change != 0)
            {
                // Drawings reduce cash (negative), contributions increase cash (positive)
                var isDrawings = eqAccount.AccountSubType == AccountSubType.Drawings;
                report.FinancingActivities.Add(new CashFlowLineItem
                {
                    Description = isDrawings ? "Owner Drawings" : $"Change in {eqAccount.Name}",
                    AccountCode = eqAccount.Code,
                    Category = CashFlowCategory.Financing,
                    Amount = isDrawings ? -Math.Abs(change) : change
                });
            }
        }

        report.NetCashFromFinancing = report.FinancingActivities.Sum(a => a.Amount);

        // ===== SUMMARY =====
        report.NetChangeInCash = report.NetCashFromOperating + report.NetCashFromInvesting + report.NetCashFromFinancing;

        // Calculate beginning and ending cash balances
        var cashAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.Cash || a.AccountSubType == AccountSubType.Bank).ToList();
        report.BeginningCashBalance = cashAccounts.Sum(ca =>
            ca.OpeningBalance + beginningLines.Where(l => l.AccountId == ca.Id).Sum(l => l.DebitAmount - l.CreditAmount));
        report.EndingCashBalance = cashAccounts.Sum(ca =>
            ca.OpeningBalance + endingLines.Where(l => l.AccountId == ca.Id).Sum(l => l.DebitAmount - l.CreditAmount));

        // ===== KEY METRICS =====
        // Operating Cash Flow Ratio = Operating Cash Flow / Current Liabilities
        var currentLiabilities = accounts
            .Where(a => a.AccountType == AccountType.Liability &&
                (a.AccountSubType == AccountSubType.AccountsPayable ||
                 a.AccountSubType == AccountSubType.AccruedLiabilities ||
                 a.AccountSubType == AccountSubType.PayrollLiabilities ||
                 a.AccountSubType == AccountSubType.ShortTermDebt ||
                 a.AccountSubType == AccountSubType.OtherCurrentLiabilities))
            .Sum(a => a.OpeningBalance + endingLines.Where(l => l.AccountId == a.Id).Sum(l => l.CreditAmount - l.DebitAmount));

        report.OperatingCashFlowRatio = currentLiabilities != 0 ? report.NetCashFromOperating / currentLiabilities : 0;

        // Free Cash Flow = Operating Cash Flow - Capital Expenditures
        var capex = report.InvestingActivities.Where(i => i.Amount < 0).Sum(i => i.Amount);
        report.FreeCashFlow = report.NetCashFromOperating + capex; // capex is negative

        // Cash Flow to Debt Ratio = Operating Cash Flow / Total Debt
        var totalDebt = accounts
            .Where(a => a.AccountSubType == AccountSubType.ShortTermDebt || a.AccountSubType == AccountSubType.LongTermDebt)
            .Sum(a => a.OpeningBalance + endingLines.Where(l => l.AccountId == a.Id).Sum(l => l.CreditAmount - l.DebitAmount));

        report.CashFlowToDebtRatio = totalDebt != 0 ? report.NetCashFromOperating / totalDebt : 0;

        // ===== MONTHLY TREND =====
        report.MonthlyTrend = await GetMonthlyCashFlowTrendAsync(range, cancellationToken);

        // ===== PREVIOUS PERIOD COMPARISON =====
        var periodLength = (range.EndDate - range.StartDate).Days;
        var previousRange = new ReportDateRange
        {
            StartDate = range.StartDate.AddDays(-periodLength - 1),
            EndDate = range.StartDate.AddDays(-1)
        };

        var previousData = await GetPreviousCashFlowDataAsync(previousRange, cancellationToken);
        if (previousData.HasValue)
        {
            var prev = previousData.Value;
            report.PreviousPeriod = new CashFlowComparison
            {
                DateRange = previousRange,
                NetCashFromOperating = prev.Operating,
                NetCashFromInvesting = prev.Investing,
                NetCashFromFinancing = prev.Financing,
                NetChangeInCash = prev.Operating + prev.Investing + prev.Financing,
                OperatingChange = prev.Operating != 0
                    ? ((report.NetCashFromOperating - prev.Operating) / Math.Abs(prev.Operating)) * 100
                    : 0,
                InvestingChange = prev.Investing != 0
                    ? ((report.NetCashFromInvesting - prev.Investing) / Math.Abs(prev.Investing)) * 100
                    : 0,
                FinancingChange = prev.Financing != 0
                    ? ((report.NetCashFromFinancing - prev.Financing) / Math.Abs(prev.Financing)) * 100
                    : 0,
                TotalChange = (prev.Operating + prev.Investing + prev.Financing) != 0
                    ? ((report.NetChangeInCash - (prev.Operating + prev.Investing + prev.Financing)) / Math.Abs(prev.Operating + prev.Investing + prev.Financing)) * 100
                    : 0
            };
        }

        return report;
    }

    public async Task<TrialBalanceReport> GetTrialBalanceAsync(DateTime asOfDate, CancellationToken cancellationToken = default)
    {
        // Get all accounts with their current balances from posted journal entries up to the as-of date
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate <= asOfDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        // Get all accounts
        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .ToListAsync(cancellationToken);

        var report = new TrialBalanceReport
        {
            AsOfDate = asOfDate,
            TotalAccounts = accounts.Count
        };

        // Calculate account balances
        var accountItems = new List<TrialBalanceLineItem>();

        foreach (var account in accounts)
        {
            var lines = journalLines.Where(l => l.AccountId == account.Id).ToList();
            decimal balance;
            decimal debit = 0;
            decimal credit = 0;

            // Calculate balance based on account type (normal balance rules)
            if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense)
            {
                // Assets and Expenses have normal debit balances
                balance = account.OpeningBalance + lines.Sum(l => l.DebitAmount - l.CreditAmount);
                if (balance >= 0)
                    debit = balance;
                else
                    credit = Math.Abs(balance);
            }
            else
            {
                // Liabilities, Equity, and Revenue have normal credit balances
                balance = account.OpeningBalance + lines.Sum(l => l.CreditAmount - l.DebitAmount);
                if (balance >= 0)
                    credit = balance;
                else
                    debit = Math.Abs(balance);
            }

            accountItems.Add(new TrialBalanceLineItem
            {
                AccountId = account.Id,
                AccountCode = account.Code,
                AccountName = account.Name,
                AccountType = account.AccountType,
                SubType = account.AccountSubType,
                Debit = debit,
                Credit = credit,
                Balance = balance
            });
        }

        // Filter to only accounts with balances (or show all based on requirement)
        report.Accounts = accountItems.Where(a => a.Debit != 0 || a.Credit != 0).OrderBy(a => a.AccountCode).ToList();
        report.AccountsWithActivity = report.Accounts.Count;
        report.ZeroBalanceAccounts = accountItems.Count - report.AccountsWithActivity;

        // Calculate totals
        report.TotalDebits = report.Accounts.Sum(a => a.Debit);
        report.TotalCredits = report.Accounts.Sum(a => a.Credit);
        report.Difference = report.TotalDebits - report.TotalCredits;

        // Group by account type
        report.Sections = report.Accounts
            .GroupBy(a => a.AccountType)
            .Select(g => new TrialBalanceSection
            {
                AccountType = g.Key,
                SectionName = g.Key.ToString(),
                TotalDebits = g.Sum(a => a.Debit),
                TotalCredits = g.Sum(a => a.Credit),
                AccountCount = g.Count(),
                Accounts = g.OrderBy(a => a.AccountCode).ToList()
            })
            .OrderBy(s => s.AccountType)
            .ToList();

        // Previous period comparison (same day last year)
        var previousDate = asOfDate.AddYears(-1);
        var previousData = await GetPreviousTrialBalanceDataAsync(previousDate, cancellationToken);
        if (previousData.HasValue)
        {
            var prev = previousData.Value;
            report.PreviousPeriod = new TrialBalanceComparison
            {
                AsOfDate = previousDate,
                TotalDebits = prev.TotalDebits,
                TotalCredits = prev.TotalCredits,
                DebitsChange = prev.TotalDebits != 0
                    ? ((report.TotalDebits - prev.TotalDebits) / prev.TotalDebits) * 100
                    : 0,
                CreditsChange = prev.TotalCredits != 0
                    ? ((report.TotalCredits - prev.TotalCredits) / prev.TotalCredits) * 100
                    : 0
            };

            // Add previous balances to line items
            var previousLines = await GetPreviousTrialBalanceLinesAsync(previousDate, cancellationToken);
            foreach (var item in report.Accounts)
            {
                var prevItem = previousLines.FirstOrDefault(p => p.AccountId == item.AccountId);
                if (prevItem != null)
                {
                    item.PreviousBalance = prevItem.Balance;
                    item.ChangeAmount = item.Balance - prevItem.Balance;
                    item.ChangePercent = prevItem.Balance != 0
                        ? ((item.Balance - prevItem.Balance) / Math.Abs(prevItem.Balance)) * 100
                        : 0;
                }
            }
        }

        return report;
    }

    private async Task<(decimal TotalDebits, decimal TotalCredits)?> GetPreviousTrialBalanceDataAsync(
        DateTime asOfDate, CancellationToken cancellationToken)
    {
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate <= asOfDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        if (!accounts.Any())
            return null;

        decimal totalDebits = 0;
        decimal totalCredits = 0;

        foreach (var account in accounts)
        {
            var lines = journalLines.Where(l => l.AccountId == account.Id).ToList();
            decimal balance;

            if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense)
            {
                balance = account.OpeningBalance + lines.Sum(l => l.DebitAmount - l.CreditAmount);
                if (balance > 0) totalDebits += balance;
                else totalCredits += Math.Abs(balance);
            }
            else
            {
                balance = account.OpeningBalance + lines.Sum(l => l.CreditAmount - l.DebitAmount);
                if (balance > 0) totalCredits += balance;
                else totalDebits += Math.Abs(balance);
            }
        }

        return (totalDebits, totalCredits);
    }

    private async Task<List<TrialBalanceLineItem>> GetPreviousTrialBalanceLinesAsync(
        DateTime asOfDate, CancellationToken cancellationToken)
    {
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate <= asOfDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        var result = new List<TrialBalanceLineItem>();

        foreach (var account in accounts)
        {
            var lines = journalLines.Where(l => l.AccountId == account.Id).ToList();
            decimal balance;
            decimal debit = 0;
            decimal credit = 0;

            if (account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense)
            {
                balance = account.OpeningBalance + lines.Sum(l => l.DebitAmount - l.CreditAmount);
                if (balance >= 0) debit = balance;
                else credit = Math.Abs(balance);
            }
            else
            {
                balance = account.OpeningBalance + lines.Sum(l => l.CreditAmount - l.DebitAmount);
                if (balance >= 0) credit = balance;
                else debit = Math.Abs(balance);
            }

            if (debit != 0 || credit != 0)
            {
                result.Add(new TrialBalanceLineItem
                {
                    AccountId = account.Id,
                    AccountCode = account.Code,
                    AccountName = account.Name,
                    AccountType = account.AccountType,
                    SubType = account.AccountSubType,
                    Debit = debit,
                    Credit = credit,
                    Balance = balance
                });
            }
        }

        return result;
    }

    private async Task<List<MonthlyCashFlow>> GetMonthlyCashFlowTrendAsync(ReportDateRange range, CancellationToken cancellationToken)
    {
        var result = new List<MonthlyCashFlow>();
        var accounts = await _context.Accounts.Where(a => a.IsActive).ToListAsync(cancellationToken);
        var cashAccounts = accounts.Where(a => a.AccountSubType == AccountSubType.Cash || a.AccountSubType == AccountSubType.Bank).ToList();

        var currentDate = new DateTime(range.StartDate.Year, range.StartDate.Month, 1);
        var endDate = range.EndDate;
        decimal runningCashBalance = 0;

        // Get initial cash balance before the period
        var initialLines = await _context.JournalEntries
            .Where(j => j.EntryDate < currentDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        runningCashBalance = cashAccounts.Sum(ca =>
            ca.OpeningBalance + initialLines.Where(l => l.AccountId == ca.Id).Sum(l => l.DebitAmount - l.CreditAmount));

        while (currentDate <= endDate)
        {
            var monthStart = currentDate;
            var monthEnd = currentDate.AddMonths(1).AddDays(-1);
            if (monthEnd > endDate) monthEnd = endDate;

            var monthLines = await _context.JournalEntries
                .Include(j => j.Lines)
                    .ThenInclude(l => l.Account)
                .Where(j => j.EntryDate >= monthStart && j.EntryDate <= monthEnd && j.Status == JournalEntryStatus.Posted)
                .SelectMany(j => j.Lines)
                .ToListAsync(cancellationToken);

            // Calculate operating, investing, financing for the month (simplified)
            var revenue = monthLines.Where(l => l.Account.AccountType == AccountType.Revenue).Sum(l => l.CreditAmount - l.DebitAmount);
            var expenses = monthLines.Where(l => l.Account.AccountType == AccountType.Expense).Sum(l => l.DebitAmount - l.CreditAmount);
            var operating = revenue - expenses;

            var investing = -monthLines.Where(l => l.Account.AccountSubType == AccountSubType.FixedAssets).Sum(l => l.DebitAmount - l.CreditAmount);
            var financing = monthLines.Where(l => l.Account.AccountSubType == AccountSubType.LongTermDebt || l.Account.AccountSubType == AccountSubType.ShortTermDebt)
                .Sum(l => l.CreditAmount - l.DebitAmount);

            var netChange = operating + investing + financing;
            runningCashBalance += netChange;

            result.Add(new MonthlyCashFlow
            {
                Year = currentDate.Year,
                Month = currentDate.Month,
                MonthName = currentDate.ToString("MMM yyyy"),
                Operating = operating,
                Investing = investing,
                Financing = financing,
                NetChange = netChange,
                EndingBalance = runningCashBalance
            });

            currentDate = currentDate.AddMonths(1);
        }

        return result;
    }

    private async Task<(decimal Operating, decimal Investing, decimal Financing)?> GetPreviousCashFlowDataAsync(
        ReportDateRange range, CancellationToken cancellationToken)
    {
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate >= range.StartDate && j.EntryDate <= range.EndDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        if (!journalLines.Any())
            return null;

        var revenue = journalLines.Where(l => l.Account.AccountType == AccountType.Revenue).Sum(l => l.CreditAmount - l.DebitAmount);
        var expenses = journalLines.Where(l => l.Account.AccountType == AccountType.Expense).Sum(l => l.DebitAmount - l.CreditAmount);
        var operating = revenue - expenses;

        var investing = -journalLines.Where(l => l.Account.AccountSubType == AccountSubType.FixedAssets).Sum(l => l.DebitAmount - l.CreditAmount);
        var financing = journalLines.Where(l => l.Account.AccountSubType == AccountSubType.LongTermDebt || l.Account.AccountSubType == AccountSubType.ShortTermDebt)
            .Sum(l => l.CreditAmount - l.DebitAmount);

        return (operating, investing, financing);
    }

    private async Task<(decimal TotalAssets, decimal TotalLiabilities, decimal TotalEquity)?> GetPreviousBalanceSheetDataAsync(
        DateTime asOfDate, CancellationToken cancellationToken)
    {
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate <= asOfDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .ToListAsync(cancellationToken);

        if (!accounts.Any())
            return null;

        var accountBalances = accounts.Select(account =>
        {
            var lines = journalLines.Where(l => l.AccountId == account.Id).ToList();
            var balance = account.AccountType switch
            {
                AccountType.Asset or AccountType.Expense =>
                    account.OpeningBalance + lines.Sum(l => l.DebitAmount - l.CreditAmount),
                _ => account.OpeningBalance + lines.Sum(l => l.CreditAmount - l.DebitAmount)
            };

            return new { Account = account, Balance = balance };
        }).ToList();

        var totalAssets = accountBalances.Where(a => a.Account.AccountType == AccountType.Asset).Sum(a => a.Balance);
        var totalLiabilities = accountBalances.Where(a => a.Account.AccountType == AccountType.Liability).Sum(a => a.Balance);

        var equityBalance = accountBalances.Where(a => a.Account.AccountType == AccountType.Equity).Sum(a => a.Balance);
        var revenueTotal = accountBalances.Where(a => a.Account.AccountType == AccountType.Revenue).Sum(a => a.Balance);
        var expenseTotal = accountBalances.Where(a => a.Account.AccountType == AccountType.Expense).Sum(a => a.Balance);
        var totalEquity = equityBalance + (revenueTotal - expenseTotal);

        return (totalAssets, totalLiabilities, totalEquity);
    }

    private async Task<List<MonthlyPnL>> GetMonthlyPnLTrendAsync(ReportDateRange range, CancellationToken cancellationToken)
    {
        var result = new List<MonthlyPnL>();
        var currentDate = new DateTime(range.StartDate.Year, range.StartDate.Month, 1);
        var endDate = range.EndDate;

        while (currentDate <= endDate)
        {
            var monthStart = currentDate;
            var monthEnd = currentDate.AddMonths(1).AddDays(-1);
            if (monthEnd > endDate) monthEnd = endDate;

            var journalLines = await _context.JournalEntries
                .Include(j => j.Lines)
                    .ThenInclude(l => l.Account)
                .Where(j => j.EntryDate >= monthStart && j.EntryDate <= monthEnd && j.Status == JournalEntryStatus.Posted)
                .SelectMany(j => j.Lines)
                .ToListAsync(cancellationToken);

            var revenue = journalLines
                .Where(l => l.Account.AccountType == AccountType.Revenue)
                .Sum(l => l.CreditAmount - l.DebitAmount);

            var cogs = journalLines
                .Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountSubType == AccountSubType.CostOfGoodsSold)
                .Sum(l => l.DebitAmount - l.CreditAmount);

            var opex = journalLines
                .Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountSubType != AccountSubType.CostOfGoodsSold)
                .Sum(l => l.DebitAmount - l.CreditAmount);

            result.Add(new MonthlyPnL
            {
                Year = currentDate.Year,
                Month = currentDate.Month,
                MonthName = currentDate.ToString("MMM yyyy"),
                Revenue = revenue,
                COGS = cogs,
                GrossProfit = revenue - cogs,
                OperatingExpenses = opex,
                NetIncome = revenue - cogs - opex
            });

            currentDate = currentDate.AddMonths(1);
        }

        return result;
    }

    private async Task<(decimal TotalRevenue, decimal GrossProfit, decimal OperatingIncome, decimal NetIncome)?> GetPreviousPeriodDataAsync(
        ReportDateRange range, CancellationToken cancellationToken)
    {
        var journalLines = await _context.JournalEntries
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Where(j => j.EntryDate >= range.StartDate && j.EntryDate <= range.EndDate && j.Status == JournalEntryStatus.Posted)
            .SelectMany(j => j.Lines)
            .ToListAsync(cancellationToken);

        if (!journalLines.Any())
            return null;

        var revenue = journalLines
            .Where(l => l.Account.AccountType == AccountType.Revenue)
            .Sum(l => l.CreditAmount - l.DebitAmount);

        var cogs = journalLines
            .Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountSubType == AccountSubType.CostOfGoodsSold)
            .Sum(l => l.DebitAmount - l.CreditAmount);

        var opex = journalLines
            .Where(l => l.Account.AccountType == AccountType.Expense && l.Account.AccountSubType != AccountSubType.CostOfGoodsSold)
            .Sum(l => l.DebitAmount - l.CreditAmount);

        var grossProfit = revenue - cogs;
        var operatingIncome = grossProfit - opex;
        var netIncome = operatingIncome;

        return (revenue, grossProfit, operatingIncome, netIncome);
    }

    private static string GetExpenseCategory(AccountSubType? subType)
    {
        return subType switch
        {
            AccountSubType.Salaries => "Payroll",
            AccountSubType.Rent => "Facilities",
            AccountSubType.Utilities => "Facilities",
            AccountSubType.Insurance => "Insurance",
            AccountSubType.Depreciation => "Depreciation",
            AccountSubType.Marketing => "Marketing",
            AccountSubType.TravelExpenses => "Travel",
            AccountSubType.OfficeSupplies => "Office",
            AccountSubType.ProfessionalFees => "Professional Services",
            _ => "Other"
        };
    }

    public byte[] ExportToPdf<T>(T report, string reportTitle) where T : class
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Item().Text("Algora ERP").Bold().FontSize(16);
                    column.Item().Text(reportTitle).Bold().FontSize(14);
                    column.Item().Text($"Generated: {DateTime.Now:MMMM dd, yyyy HH:mm}").FontSize(9).FontColor(Colors.Grey.Medium);
                    column.Item().Height(10);
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingVertical(20).Column(column =>
                {
                    // Render report content based on type
                    if (report is InvoiceSummaryReport invoiceReport)
                    {
                        RenderInvoiceSummaryPdf(column, invoiceReport);
                    }
                    else if (report is PaymentSummaryReport paymentReport)
                    {
                        RenderPaymentSummaryPdf(column, paymentReport);
                    }
                    else if (report is AgingReport agingReport)
                    {
                        RenderAgingReportPdf(column, agingReport);
                    }
                    else if (report is CollectionEfficiencyReport collectionReport)
                    {
                        RenderCollectionReportPdf(column, collectionReport);
                    }
                    else if (report is ProfitAndLossReport pnlReport)
                    {
                        RenderProfitAndLossPdf(column, pnlReport);
                    }
                    else if (report is BalanceSheetReport balanceSheetReport)
                    {
                        RenderBalanceSheetPdf(column, balanceSheetReport);
                    }
                    else if (report is CashFlowStatementReport cashFlowReport)
                    {
                        RenderCashFlowStatementPdf(column, cashFlowReport);
                    }
                    else if (report is TrialBalanceReport trialBalanceReport)
                    {
                        RenderTrialBalancePdf(column, trialBalanceReport);
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Page ").FontSize(9);
                    text.CurrentPageNumber().FontSize(9);
                    text.Span(" of ").FontSize(9);
                    text.TotalPages().FontSize(9);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] ExportToCsv<T>(IEnumerable<T> data) where T : class
    {
        var sb = new StringBuilder();
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Header
        sb.AppendLine(string.Join(",", properties.Select(p => $"\"{p.Name}\"")));

        // Data rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var value = p.GetValue(item);
                if (value == null) return "\"\"";
                if (value is DateTime dt) return $"\"{dt:yyyy-MM-dd}\"";
                if (value is decimal d) return d.ToString(CultureInfo.InvariantCulture);
                return $"\"{value.ToString()?.Replace("\"", "\"\"")}\"";
            });
            sb.AppendLine(string.Join(",", values));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    #region Helper Methods

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    private static AgingBucket CreateAgingBucket(string label, List<Invoice> invoices, decimal totalOutstanding)
    {
        var amount = invoices.Sum(i => i.BalanceDue);
        return new AgingBucket
        {
            Label = label,
            InvoiceCount = invoices.Count,
            Amount = amount,
            Percentage = totalOutstanding > 0 ? (amount / totalOutstanding) * 100 : 0
        };
    }

    private void RenderInvoiceSummaryPdf(ColumnDescriptor column, InvoiceSummaryReport report)
    {
        column.Item().Text($"Period: {report.DateRange.StartDate:MMM dd, yyyy} - {report.DateRange.EndDate:MMM dd, yyyy}").FontSize(11);
        column.Item().Height(15);

        // Summary stats
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Total Invoiced").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Total Paid").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Outstanding").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Overdue").Bold();

            table.Cell().Padding(8).Text(report.TotalInvoiced.ToString("C2"));
            table.Cell().Padding(8).Text(report.TotalPaid.ToString("C2"));
            table.Cell().Padding(8).Text(report.TotalOutstanding.ToString("C2"));
            table.Cell().Padding(8).Text(report.TotalOverdue.ToString("C2")).FontColor(Colors.Red.Medium);
        });

        column.Item().Height(20);
        column.Item().Text("Status Breakdown").Bold().FontSize(12);
        column.Item().Height(10);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Status").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Count").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Amount").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("%").Bold();

            foreach (var status in report.ByStatus)
            {
                table.Cell().Padding(5).Text(status.Status.ToString());
                table.Cell().Padding(5).Text(status.Count.ToString());
                table.Cell().Padding(5).Text(status.Amount.ToString("C2"));
                table.Cell().Padding(5).Text($"{status.Percentage:F1}%");
            }
        });
    }

    private void RenderPaymentSummaryPdf(ColumnDescriptor column, PaymentSummaryReport report)
    {
        column.Item().Text($"Period: {report.DateRange.StartDate:MMM dd, yyyy} - {report.DateRange.EndDate:MMM dd, yyyy}").FontSize(11);
        column.Item().Height(15);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Total Received").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Payments").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Average").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Largest").Bold();

            table.Cell().Padding(8).Text(report.TotalReceived.ToString("C2"));
            table.Cell().Padding(8).Text(report.TotalPayments.ToString());
            table.Cell().Padding(8).Text(report.AveragePayment.ToString("C2"));
            table.Cell().Padding(8).Text(report.LargestPayment.ToString("C2"));
        });

        column.Item().Height(20);
        column.Item().Text("By Payment Method").Bold().FontSize(12);
        column.Item().Height(10);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Method").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Count").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Amount").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("%").Bold();

            foreach (var method in report.ByMethod)
            {
                table.Cell().Padding(5).Text(method.Method.ToString());
                table.Cell().Padding(5).Text(method.Count.ToString());
                table.Cell().Padding(5).Text(method.Amount.ToString("C2"));
                table.Cell().Padding(5).Text($"{method.Percentage:F1}%");
            }
        });
    }

    private void RenderAgingReportPdf(ColumnDescriptor column, AgingReport report)
    {
        column.Item().Text($"As of: {report.AsOfDate:MMMM dd, yyyy}").FontSize(11);
        column.Item().Text($"Total Outstanding: {report.TotalOutstanding:C2}").Bold().FontSize(12);
        column.Item().Height(15);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(2);
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Aging Bucket").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Invoices").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Amount").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("%").Bold();

            var buckets = new[] { report.Current, report.Days1To30, report.Days31To60, report.Days61To90, report.Over90Days };
            foreach (var bucket in buckets)
            {
                table.Cell().Padding(5).Text(bucket.Label);
                table.Cell().Padding(5).Text(bucket.InvoiceCount.ToString());
                table.Cell().Padding(5).Text(bucket.Amount.ToString("C2"));
                table.Cell().Padding(5).Text($"{bucket.Percentage:F1}%");
            }
        });

        if (report.ByCustomer.Any())
        {
            column.Item().Height(20);
            column.Item().Text("By Customer").Bold().FontSize(12);
            column.Item().Height(10);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Customer").Bold().FontSize(8);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Current").Bold().FontSize(8);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("1-30").Bold().FontSize(8);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("31-60").Bold().FontSize(8);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("61-90").Bold().FontSize(8);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text(">90").Bold().FontSize(8);
                table.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Total").Bold().FontSize(8);

                foreach (var customer in report.ByCustomer.Take(20))
                {
                    table.Cell().Padding(3).Text(customer.CustomerName).FontSize(8);
                    table.Cell().Padding(3).Text(customer.Current.ToString("C0")).FontSize(8);
                    table.Cell().Padding(3).Text(customer.Days1To30.ToString("C0")).FontSize(8);
                    table.Cell().Padding(3).Text(customer.Days31To60.ToString("C0")).FontSize(8);
                    table.Cell().Padding(3).Text(customer.Days61To90.ToString("C0")).FontSize(8);
                    table.Cell().Padding(3).Text(customer.Over90Days.ToString("C0")).FontSize(8);
                    table.Cell().Padding(3).Text(customer.Total.ToString("C0")).Bold().FontSize(8);
                }
            });
        }
    }

    private void RenderCollectionReportPdf(ColumnDescriptor column, CollectionEfficiencyReport report)
    {
        column.Item().Text($"Period: {report.DateRange.StartDate:MMM dd, yyyy} - {report.DateRange.EndDate:MMM dd, yyyy}").FontSize(11);
        column.Item().Height(15);

        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Padding(8).Text("Total Invoiced:").Bold();
            table.Cell().Padding(8).Text(report.TotalInvoiced.ToString("C2"));

            table.Cell().Padding(8).Text("Total Collected:").Bold();
            table.Cell().Padding(8).Text(report.TotalCollected.ToString("C2"));

            table.Cell().Padding(8).Text("Collection Rate:").Bold();
            table.Cell().Padding(8).Text($"{report.CollectionRate:F1}%");

            table.Cell().Padding(8).Text("Avg Days to Payment:").Bold();
            table.Cell().Padding(8).Text($"{report.AverageDaysToPayment:F1} days");

            table.Cell().Padding(8).Text("On-Time Payments:").Bold();
            table.Cell().Padding(8).Text($"{report.InvoicesPaidOnTime} ({report.OnTimePaymentRate:F1}%)");

            table.Cell().Padding(8).Text("Late Payments:").Bold();
            table.Cell().Padding(8).Text(report.InvoicesPaidLate.ToString());
        });
    }

    private void RenderProfitAndLossPdf(ColumnDescriptor column, ProfitAndLossReport report)
    {
        column.Item().Text($"Period: {report.DateRange.StartDate:MMM dd, yyyy} - {report.DateRange.EndDate:MMM dd, yyyy}").FontSize(11);
        column.Item().Height(15);

        // Revenue Section
        column.Item().Text("REVENUE").Bold().FontSize(12);
        column.Item().Height(5);
        foreach (var item in report.RevenueItems)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text($"  {item.AccountName}");
                row.RelativeItem(1).AlignRight().Text(item.Amount.ToString("C2"));
            });
        }
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Total Revenue").Bold();
            row.RelativeItem(1).AlignRight().Text(report.TotalRevenue.ToString("C2")).Bold();
        });
        column.Item().Height(10);

        // COGS Section
        column.Item().Text("COST OF GOODS SOLD").Bold().FontSize(12);
        column.Item().Height(5);
        foreach (var item in report.COGSItems)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text($"  {item.AccountName}");
                row.RelativeItem(1).AlignRight().Text(item.Amount.ToString("C2"));
            });
        }
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Total COGS").Bold();
            row.RelativeItem(1).AlignRight().Text(report.TotalCOGS.ToString("C2")).Bold();
        });
        column.Item().Height(10);

        // Gross Profit
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("GROSS PROFIT").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.GrossProfit.ToString("C2")).Bold().FontSize(12);
        });
        column.Item().Text($"Gross Margin: {report.GrossProfitMargin:F1}%").FontSize(10);
        column.Item().Height(10);

        // Operating Expenses
        column.Item().Text("OPERATING EXPENSES").Bold().FontSize(12);
        column.Item().Height(5);
        foreach (var item in report.OperatingExpenseItems)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text($"  {item.AccountName}");
                row.RelativeItem(1).AlignRight().Text(item.Amount.ToString("C2"));
            });
        }
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Total Operating Expenses").Bold();
            row.RelativeItem(1).AlignRight().Text(report.TotalOperatingExpenses.ToString("C2")).Bold();
        });
        column.Item().Height(10);

        // Operating Income
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("OPERATING INCOME").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.OperatingIncome.ToString("C2")).Bold().FontSize(12);
        });
        column.Item().Height(10);

        // Net Income
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("NET INCOME").Bold().FontSize(14);
            row.RelativeItem(1).AlignRight().Text(report.NetIncome.ToString("C2")).Bold().FontSize(14);
        });
        column.Item().Text($"Net Profit Margin: {report.NetProfitMargin:F1}%").FontSize(10);
    }

    private void RenderBalanceSheetPdf(ColumnDescriptor column, BalanceSheetReport report)
    {
        column.Item().Text($"As of: {report.AsOfDate:MMMM dd, yyyy}").FontSize(11);
        column.Item().Height(15);

        // ===== ASSETS =====
        column.Item().Text("ASSETS").Bold().FontSize(14);
        column.Item().Height(10);

        foreach (var section in report.AssetSections)
        {
            if (section.Items.Any())
            {
                column.Item().Text(section.SectionName).Bold().FontSize(11);
                column.Item().Height(5);

                foreach (var item in section.Items)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text($"  {item.AccountName}");
                        row.RelativeItem(1).AlignRight().Text(item.Balance.ToString("C2"));
                    });
                }

                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Text($"Total {section.SectionName}").Bold();
                    row.RelativeItem(1).AlignRight().Text(section.Total.ToString("C2")).Bold();
                });
                column.Item().Height(8);
            }
        }

        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("TOTAL ASSETS").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.TotalAssets.ToString("C2")).Bold().FontSize(12);
        });
        column.Item().Height(20);

        // ===== LIABILITIES =====
        column.Item().Text("LIABILITIES").Bold().FontSize(14);
        column.Item().Height(10);

        foreach (var section in report.LiabilitySections)
        {
            if (section.Items.Any())
            {
                column.Item().Text(section.SectionName).Bold().FontSize(11);
                column.Item().Height(5);

                foreach (var item in section.Items)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem(3).Text($"  {item.AccountName}");
                        row.RelativeItem(1).AlignRight().Text(item.Balance.ToString("C2"));
                    });
                }

                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Text($"Total {section.SectionName}").Bold();
                    row.RelativeItem(1).AlignRight().Text(section.Total.ToString("C2")).Bold();
                });
                column.Item().Height(8);
            }
        }

        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("TOTAL LIABILITIES").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.TotalLiabilities.ToString("C2")).Bold().FontSize(12);
        });
        column.Item().Height(20);

        // ===== EQUITY =====
        column.Item().Text("EQUITY").Bold().FontSize(14);
        column.Item().Height(10);

        foreach (var item in report.EquityItems)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text($"  {item.AccountName}");
                row.RelativeItem(1).AlignRight().Text(item.Balance.ToString("C2"));
            });
        }

        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("TOTAL EQUITY").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.TotalEquity.ToString("C2")).Bold().FontSize(12);
        });
        column.Item().Height(15);

        // ===== TOTAL LIABILITIES & EQUITY =====
        column.Item().LineHorizontal(2).LineColor(Colors.Black);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("TOTAL LIABILITIES & EQUITY").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.TotalLiabilitiesAndEquity.ToString("C2")).Bold().FontSize(12);
        });
        column.Item().Height(20);

        // ===== KEY RATIOS =====
        column.Item().Text("KEY RATIOS").Bold().FontSize(12);
        column.Item().Height(10);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Padding(5).Text("Current Ratio").Bold();
            table.Cell().Padding(5).Text("Quick Ratio").Bold();
            table.Cell().Padding(5).Text("Debt/Equity").Bold();
            table.Cell().Padding(5).Text("Working Capital").Bold();

            table.Cell().Padding(5).Text($"{report.CurrentRatio:F2}");
            table.Cell().Padding(5).Text($"{report.QuickRatio:F2}");
            table.Cell().Padding(5).Text($"{report.DebtToEquityRatio:F2}");
            table.Cell().Padding(5).Text(report.WorkingCapital.ToString("C2"));
        });
    }

    private void RenderCashFlowStatementPdf(ColumnDescriptor column, CashFlowStatementReport report)
    {
        column.Item().Text($"Period: {report.DateRange.StartDate:MMM dd, yyyy} - {report.DateRange.EndDate:MMM dd, yyyy}").FontSize(11);
        column.Item().Height(15);

        // ===== OPERATING ACTIVITIES =====
        column.Item().Text("CASH FLOWS FROM OPERATING ACTIVITIES").Bold().FontSize(12);
        column.Item().Height(5);

        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("  Net Income");
            row.RelativeItem(1).AlignRight().Text(report.NetIncome.ToString("C2"));
        });

        column.Item().Height(5);
        column.Item().Text("  Adjustments to reconcile net income:").FontSize(9);

        foreach (var item in report.OperatingAdjustments)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text($"    {item.Description}");
                row.RelativeItem(1).AlignRight().Text(item.Amount.ToString("C2"));
            });
        }

        column.Item().Height(5);
        column.Item().Text("  Changes in operating assets and liabilities:").FontSize(9);

        foreach (var item in report.WorkingCapitalChanges)
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text($"    {item.Description}");
                row.RelativeItem(1).AlignRight().Text(item.Amount.ToString("C2"));
            });
        }

        column.Item().Height(5);
        column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Net Cash from Operating Activities").Bold();
            row.RelativeItem(1).AlignRight().Text(report.NetCashFromOperating.ToString("C2")).Bold();
        });
        column.Item().Height(15);

        // ===== INVESTING ACTIVITIES =====
        column.Item().Text("CASH FLOWS FROM INVESTING ACTIVITIES").Bold().FontSize(12);
        column.Item().Height(5);

        if (report.InvestingActivities.Any())
        {
            foreach (var item in report.InvestingActivities)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Text($"  {item.Description}");
                    row.RelativeItem(1).AlignRight().Text(item.Amount.ToString("C2"));
                });
            }
        }
        else
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text("  (No investing activities)").FontColor(Colors.Grey.Medium);
                row.RelativeItem(1).AlignRight().Text("$0.00").FontColor(Colors.Grey.Medium);
            });
        }

        column.Item().Height(5);
        column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Net Cash from Investing Activities").Bold();
            row.RelativeItem(1).AlignRight().Text(report.NetCashFromInvesting.ToString("C2")).Bold();
        });
        column.Item().Height(15);

        // ===== FINANCING ACTIVITIES =====
        column.Item().Text("CASH FLOWS FROM FINANCING ACTIVITIES").Bold().FontSize(12);
        column.Item().Height(5);

        if (report.FinancingActivities.Any())
        {
            foreach (var item in report.FinancingActivities)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem(3).Text($"  {item.Description}");
                    row.RelativeItem(1).AlignRight().Text(item.Amount.ToString("C2"));
                });
            }
        }
        else
        {
            column.Item().Row(row =>
            {
                row.RelativeItem(3).Text("  (No financing activities)").FontColor(Colors.Grey.Medium);
                row.RelativeItem(1).AlignRight().Text("$0.00").FontColor(Colors.Grey.Medium);
            });
        }

        column.Item().Height(5);
        column.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Medium);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Net Cash from Financing Activities").Bold();
            row.RelativeItem(1).AlignRight().Text(report.NetCashFromFinancing.ToString("C2")).Bold();
        });
        column.Item().Height(20);

        // ===== SUMMARY =====
        column.Item().LineHorizontal(2).LineColor(Colors.Black);
        column.Item().Height(5);

        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Net Change in Cash").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.NetChangeInCash.ToString("C2")).Bold().FontSize(12);
        });

        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Beginning Cash Balance");
            row.RelativeItem(1).AlignRight().Text(report.BeginningCashBalance.ToString("C2"));
        });

        column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
        column.Item().Row(row =>
        {
            row.RelativeItem(3).Text("Ending Cash Balance").Bold().FontSize(12);
            row.RelativeItem(1).AlignRight().Text(report.EndingCashBalance.ToString("C2")).Bold().FontSize(12);
        });
        column.Item().Height(20);

        // ===== KEY METRICS =====
        column.Item().Text("KEY CASH FLOW METRICS").Bold().FontSize(12);
        column.Item().Height(10);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Padding(5).Text("Operating CF Ratio").Bold();
            table.Cell().Padding(5).Text("Free Cash Flow").Bold();
            table.Cell().Padding(5).Text("CF to Debt Ratio").Bold();

            table.Cell().Padding(5).Text($"{report.OperatingCashFlowRatio:F2}");
            table.Cell().Padding(5).Text(report.FreeCashFlow.ToString("C2"));
            table.Cell().Padding(5).Text($"{report.CashFlowToDebtRatio:F2}");
        });
    }

    private void RenderTrialBalancePdf(ColumnDescriptor column, TrialBalanceReport report)
    {
        column.Item().Text($"As of: {report.AsOfDate:MMMM dd, yyyy}").FontSize(11);
        column.Item().Height(15);

        // Balance Status
        column.Item().Row(row =>
        {
            row.RelativeItem().Text("Balance Status:").Bold();
            if (report.IsBalanced)
            {
                row.RelativeItem().Text("BALANCED").FontColor(Colors.Green.Medium).Bold();
            }
            else
            {
                row.RelativeItem().Text($"OUT OF BALANCE (Difference: {report.Difference:C2})").FontColor(Colors.Red.Medium).Bold();
            }
        });
        column.Item().Height(15);

        // Summary Totals
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Total Debits").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Total Credits").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Accounts w/ Activity").Bold();
            table.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Zero Balance Accounts").Bold();

            table.Cell().Padding(8).Text(report.TotalDebits.ToString("C2"));
            table.Cell().Padding(8).Text(report.TotalCredits.ToString("C2"));
            table.Cell().Padding(8).Text(report.AccountsWithActivity.ToString());
            table.Cell().Padding(8).Text(report.ZeroBalanceAccounts.ToString());
        });

        column.Item().Height(20);

        // Account Details by Section
        foreach (var section in report.Sections)
        {
            column.Item().Text(section.SectionName.ToUpper()).Bold().FontSize(12);
            column.Item().Height(5);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);  // Account Code
                    columns.RelativeColumn(3);   // Account Name
                    columns.RelativeColumn();    // Debit
                    columns.RelativeColumn();    // Credit
                });

                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Code").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Account Name").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Debit").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Credit").Bold();

                foreach (var account in section.Accounts)
                {
                    table.Cell().Padding(5).Text(account.AccountCode);
                    table.Cell().Padding(5).Text(account.AccountName);
                    table.Cell().Padding(5).AlignRight().Text(account.Debit > 0 ? account.Debit.ToString("C2") : "-");
                    table.Cell().Padding(5).AlignRight().Text(account.Credit > 0 ? account.Credit.ToString("C2") : "-");
                }

                // Section subtotals
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("");
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text($"{section.SectionName} Total").Bold();
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text(section.TotalDebits.ToString("C2")).Bold();
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).AlignRight().Text(section.TotalCredits.ToString("C2")).Bold();
            });

            column.Item().Height(15);
        }

        // Grand Totals
        column.Item().LineHorizontal(2).LineColor(Colors.Black);
        column.Item().Height(5);
        column.Item().Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(80);
                columns.RelativeColumn(3);
                columns.RelativeColumn();
                columns.RelativeColumn();
            });

            table.Cell().Padding(5).Text("");
            table.Cell().Padding(5).Text("GRAND TOTAL").Bold().FontSize(12);
            table.Cell().Padding(5).AlignRight().Text(report.TotalDebits.ToString("C2")).Bold().FontSize(12);
            table.Cell().Padding(5).AlignRight().Text(report.TotalCredits.ToString("C2")).Bold().FontSize(12);
        });

        // Previous Period Comparison
        if (report.PreviousPeriod != null)
        {
            column.Item().Height(20);
            column.Item().Text("YEAR-OVER-YEAR COMPARISON").Bold().FontSize(12);
            column.Item().Height(10);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text($"Current ({report.AsOfDate:MMM yyyy})").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text($"Prior ({report.PreviousPeriod.AsOfDate:MMM yyyy})").Bold();
                table.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Change %").Bold();

                table.Cell().Padding(5).Text("Total Debits");
                table.Cell().Padding(5).Text(report.TotalDebits.ToString("C2"));
                table.Cell().Padding(5).Text(report.PreviousPeriod.TotalDebits.ToString("C2"));
                table.Cell().Padding(5).Text($"{report.PreviousPeriod.DebitsChange:F1}%");

                table.Cell().Padding(5).Text("Total Credits");
                table.Cell().Padding(5).Text(report.TotalCredits.ToString("C2"));
                table.Cell().Padding(5).Text(report.PreviousPeriod.TotalCredits.ToString("C2"));
                table.Cell().Padding(5).Text($"{report.PreviousPeriod.CreditsChange:F1}%");
            });
        }
    }

    #endregion
}
