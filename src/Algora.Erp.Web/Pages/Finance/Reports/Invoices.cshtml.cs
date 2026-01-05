using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class InvoicesModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public InvoicesModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public InvoiceSummaryReport Report { get; set; } = new();
    public CollectionEfficiencyReport CollectionReport { get; set; } = new();

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

        Report = await _reportService.GetInvoiceSummaryAsync(DateRange);
        CollectionReport = await _reportService.GetCollectionEfficiencyAsync(DateRange);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var report = await _reportService.GetInvoiceSummaryAsync(DateRange);
        var pdfBytes = _reportService.ExportToPdf(report, "Invoice Summary Report");

        return File(pdfBytes, "application/pdf", $"InvoiceReport_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var report = await _reportService.GetInvoiceSummaryAsync(DateRange);
        var csvBytes = _reportService.ExportToCsv(report.ByStatus);

        return File(csvBytes, "text/csv", $"InvoiceStatusBreakdown_{DateTime.Now:yyyyMMdd}.csv");
    }
}
