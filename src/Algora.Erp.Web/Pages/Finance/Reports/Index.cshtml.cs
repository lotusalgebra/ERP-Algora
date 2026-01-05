using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class IndexModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public IndexModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public InvoiceSummaryReport InvoiceSummary { get; set; } = new();
    public PaymentSummaryReport PaymentSummary { get; set; } = new();
    public AgingReport Aging { get; set; } = new();
    public List<MonthlyRevenueTrend> RevenueTrend { get; set; } = new();
    public List<CustomerRevenueItem> TopCustomers { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string Period { get; set; } = "thismonth";

    public ReportDateRange DateRange { get; set; } = ReportDateRange.ThisMonth();

    public async Task OnGetAsync()
    {
        DateRange = Period?.ToLower() switch
        {
            "lastmonth" => ReportDateRange.LastMonth(),
            "thisquarter" => ReportDateRange.ThisQuarter(),
            "thisyear" => ReportDateRange.ThisYear(),
            "lastyear" => ReportDateRange.LastYear(),
            _ => ReportDateRange.ThisMonth()
        };

        InvoiceSummary = await _reportService.GetInvoiceSummaryAsync(DateRange);
        PaymentSummary = await _reportService.GetPaymentSummaryAsync(DateRange);
        Aging = await _reportService.GetAgingReportAsync();
        RevenueTrend = await _reportService.GetMonthlyRevenueTrendAsync(12);
        TopCustomers = await _reportService.GetRevenueByCustomerAsync(DateRange, 5);
    }
}
