using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class PaymentsModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public PaymentsModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public PaymentSummaryReport Report { get; set; } = new();
    public CashFlowReport CashFlow { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    public ReportDateRange DateRange { get; set; } = ReportDateRange.ThisMonth();

    public async Task OnGetAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }
        else
        {
            StartDate = DateRange.StartDate;
            EndDate = DateRange.EndDate;
        }

        Report = await _reportService.GetPaymentSummaryAsync(DateRange);
        CashFlow = await _reportService.GetCashFlowReportAsync(DateRange);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var report = await _reportService.GetPaymentSummaryAsync(DateRange);
        var pdfBytes = _reportService.ExportToPdf(report, "Payment Summary Report");

        return File(pdfBytes, "application/pdf", $"PaymentReport_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var report = await _reportService.GetPaymentSummaryAsync(DateRange);
        var csvBytes = _reportService.ExportToCsv(report.DailyTrend);

        return File(csvBytes, "text/csv", $"DailyPayments_{DateTime.Now:yyyyMMdd}.csv");
    }
}
