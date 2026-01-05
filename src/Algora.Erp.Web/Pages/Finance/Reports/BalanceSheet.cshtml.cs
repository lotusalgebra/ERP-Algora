using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class BalanceSheetModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public BalanceSheetModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public BalanceSheetReport Report { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? AsOfDate { get; set; }

    public async Task OnGetAsync()
    {
        AsOfDate ??= DateTime.Today;
        Report = await _reportService.GetBalanceSheetAsync(AsOfDate.Value);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        AsOfDate ??= DateTime.Today;
        var report = await _reportService.GetBalanceSheetAsync(AsOfDate.Value);
        var pdfBytes = _reportService.ExportToPdf(report, "Balance Sheet");

        return File(pdfBytes, "application/pdf", $"BalanceSheet_{AsOfDate.Value:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        AsOfDate ??= DateTime.Today;
        var report = await _reportService.GetBalanceSheetAsync(AsOfDate.Value);

        // Combine all line items for CSV export
        var allItems = new List<BalanceSheetLineItem>();
        foreach (var section in report.AssetSections)
        {
            allItems.AddRange(section.Items);
        }
        foreach (var section in report.LiabilitySections)
        {
            allItems.AddRange(section.Items);
        }
        allItems.AddRange(report.EquityItems);

        var csvBytes = _reportService.ExportToCsv(allItems);

        return File(csvBytes, "text/csv", $"BalanceSheet_{AsOfDate.Value:yyyyMMdd}.csv");
    }
}
