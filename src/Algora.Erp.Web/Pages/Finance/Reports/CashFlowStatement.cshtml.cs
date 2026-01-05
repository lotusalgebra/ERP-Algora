using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class CashFlowStatementModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public CashFlowStatementModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public CashFlowStatementReport Report { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    public async Task OnGetAsync()
    {
        // Default to current fiscal year
        StartDate ??= new DateTime(DateTime.Today.Year, 1, 1);
        EndDate ??= DateTime.Today;

        var dateRange = new ReportDateRange
        {
            StartDate = StartDate.Value,
            EndDate = EndDate.Value
        };

        Report = await _reportService.GetCashFlowStatementAsync(dateRange);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        StartDate ??= new DateTime(DateTime.Today.Year, 1, 1);
        EndDate ??= DateTime.Today;

        var dateRange = new ReportDateRange
        {
            StartDate = StartDate.Value,
            EndDate = EndDate.Value
        };

        var report = await _reportService.GetCashFlowStatementAsync(dateRange);
        var pdfBytes = _reportService.ExportToPdf(report, "Cash Flow Statement");

        return File(pdfBytes, "application/pdf", $"CashFlowStatement_{StartDate.Value:yyyyMMdd}_{EndDate.Value:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        StartDate ??= new DateTime(DateTime.Today.Year, 1, 1);
        EndDate ??= DateTime.Today;

        var dateRange = new ReportDateRange
        {
            StartDate = StartDate.Value,
            EndDate = EndDate.Value
        };

        var report = await _reportService.GetCashFlowStatementAsync(dateRange);

        // Combine all line items for CSV export
        var allItems = new List<CashFlowLineItem>();
        allItems.AddRange(report.OperatingAdjustments);
        allItems.AddRange(report.WorkingCapitalChanges);
        allItems.AddRange(report.InvestingActivities);
        allItems.AddRange(report.FinancingActivities);

        var csvBytes = _reportService.ExportToCsv(allItems);

        return File(csvBytes, "text/csv", $"CashFlowStatement_{StartDate.Value:yyyyMMdd}_{EndDate.Value:yyyyMMdd}.csv");
    }
}
