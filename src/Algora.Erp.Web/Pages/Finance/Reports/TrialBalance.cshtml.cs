using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class TrialBalanceModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public TrialBalanceModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public TrialBalanceReport Report { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? AsOfDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool ShowZeroBalances { get; set; } = false;

    public async Task OnGetAsync()
    {
        AsOfDate ??= DateTime.Today;
        Report = await _reportService.GetTrialBalanceAsync(AsOfDate.Value);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        AsOfDate ??= DateTime.Today;
        var report = await _reportService.GetTrialBalanceAsync(AsOfDate.Value);
        var pdfBytes = _reportService.ExportToPdf(report, "Trial Balance");

        return File(pdfBytes, "application/pdf", $"TrialBalance_{AsOfDate.Value:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        AsOfDate ??= DateTime.Today;
        var report = await _reportService.GetTrialBalanceAsync(AsOfDate.Value);
        var csvBytes = _reportService.ExportToCsv(report.Accounts);

        return File(csvBytes, "text/csv", $"TrialBalance_{AsOfDate.Value:yyyyMMdd}.csv");
    }
}
