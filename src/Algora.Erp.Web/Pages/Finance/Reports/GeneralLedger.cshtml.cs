using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class GeneralLedgerModel : PageModel
{
    private readonly IFinancialReportService _reportService;
    private readonly IApplicationDbContext _context;

    public GeneralLedgerModel(IFinancialReportService reportService, IApplicationDbContext context)
    {
        _reportService = reportService;
        _context = context;
    }

    public GeneralLedgerReport Report { get; set; } = new();
    public List<SelectListItem> AccountOptions { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? AccountId { get; set; }

    public async Task OnGetAsync()
    {
        StartDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        EndDate ??= DateTime.Today;

        var range = new ReportDateRange { StartDate = StartDate.Value, EndDate = EndDate.Value };
        Report = await _reportService.GetGeneralLedgerAsync(range, AccountId);

        await LoadAccountOptionsAsync();
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        StartDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        EndDate ??= DateTime.Today;

        var range = new ReportDateRange { StartDate = StartDate.Value, EndDate = EndDate.Value };
        var report = await _reportService.GetGeneralLedgerAsync(range, AccountId);
        var pdfBytes = _reportService.ExportToPdf(report, "General Ledger");

        return File(pdfBytes, "application/pdf", $"GeneralLedger_{StartDate.Value:yyyyMMdd}_{EndDate.Value:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        StartDate ??= new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        EndDate ??= DateTime.Today;

        var range = new ReportDateRange { StartDate = StartDate.Value, EndDate = EndDate.Value };
        var report = await _reportService.GetGeneralLedgerAsync(range, AccountId);

        // Flatten transactions for CSV export
        var transactions = report.Accounts
            .SelectMany(a => a.Transactions.Select(t => new
            {
                AccountCode = a.AccountCode,
                AccountName = a.AccountName,
                t.EntryDate,
                t.EntryNumber,
                t.Description,
                t.Reference,
                t.Debit,
                t.Credit,
                t.RunningBalance
            }))
            .ToList();

        var csvBytes = _reportService.ExportToCsv(transactions);

        return File(csvBytes, "text/csv", $"GeneralLedger_{StartDate.Value:yyyyMMdd}_{EndDate.Value:yyyyMMdd}.csv");
    }

    private async Task LoadAccountOptionsAsync()
    {
        var accounts = await _context.Accounts
            .Where(a => a.IsActive)
            .OrderBy(a => a.Code)
            .Select(a => new { a.Id, a.Code, a.Name })
            .ToListAsync();

        AccountOptions = accounts
            .Select(a => new SelectListItem($"{a.Code} - {a.Name}", a.Id.ToString()))
            .ToList();

        AccountOptions.Insert(0, new SelectListItem("All Accounts", ""));
    }
}
