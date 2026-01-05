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

    #endregion
}
