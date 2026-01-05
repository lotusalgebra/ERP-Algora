using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class AgingModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public AgingModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public AgingReport Report { get; set; } = new();

    public async Task OnGetAsync()
    {
        Report = await _reportService.GetAgingReportAsync();
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        var report = await _reportService.GetAgingReportAsync();
        var pdfBytes = _reportService.ExportToPdf(report, "Accounts Receivable Aging Report");

        return File(pdfBytes, "application/pdf", $"AgingReport_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        var report = await _reportService.GetAgingReportAsync();
        var csvBytes = _reportService.ExportToCsv(report.ByCustomer);

        return File(csvBytes, "text/csv", $"AgingByCustomer_{DateTime.Now:yyyyMMdd}.csv");
    }
}
