using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class ProfitLossModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public ProfitLossModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public ProfitAndLossReport Report { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string Period { get; set; } = "thismonth";

    public ReportDateRange DateRange { get; set; } = ReportDateRange.ThisMonth();

    public async Task OnGetAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }
        else
        {
            DateRange = Period?.ToLower() switch
            {
                "lastmonth" => ReportDateRange.LastMonth(),
                "thisquarter" => ReportDateRange.ThisQuarter(),
                "thisyear" => ReportDateRange.ThisYear(),
                "lastyear" => ReportDateRange.LastYear(),
                _ => ReportDateRange.ThisMonth()
            };
            StartDate = DateRange.StartDate;
            EndDate = DateRange.EndDate;
        }

        Report = await _reportService.GetProfitAndLossAsync(DateRange);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var report = await _reportService.GetProfitAndLossAsync(DateRange);
        var pdfBytes = _reportService.ExportToPdf(report, "Profit & Loss Statement");

        return File(pdfBytes, "application/pdf", $"ProfitLoss_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var report = await _reportService.GetProfitAndLossAsync(DateRange);

        // Combine all line items for CSV export
        var allItems = new List<PnLLineItem>();
        allItems.AddRange(report.RevenueItems);
        allItems.AddRange(report.COGSItems);
        allItems.AddRange(report.OperatingExpenseItems);
        allItems.AddRange(report.OtherIncomeItems);
        allItems.AddRange(report.OtherExpenseItems);

        var csvBytes = _reportService.ExportToCsv(allItems);

        return File(csvBytes, "text/csv", $"ProfitLoss_{DateTime.Now:yyyyMMdd}.csv");
    }
}
