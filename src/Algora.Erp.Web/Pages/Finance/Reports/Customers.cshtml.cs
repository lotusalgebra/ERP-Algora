using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Reports;

public class CustomersModel : PageModel
{
    private readonly IFinancialReportService _reportService;

    public CustomersModel(IFinancialReportService reportService)
    {
        _reportService = reportService;
    }

    public List<CustomerRevenueItem> Customers { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalOutstanding { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int TopN { get; set; } = 50;

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

        Customers = await _reportService.GetRevenueByCustomerAsync(DateRange, TopN);
        TotalRevenue = Customers.Sum(c => c.TotalRevenue);
        TotalPaid = Customers.Sum(c => c.PaidAmount);
        TotalOutstanding = Customers.Sum(c => c.OutstandingAmount);
    }

    public async Task<IActionResult> OnGetExportPdfAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var customers = await _reportService.GetRevenueByCustomerAsync(DateRange, TopN);
        var pdfBytes = _reportService.ExportToPdf(customers, "Customer Revenue Report");

        return File(pdfBytes, "application/pdf", $"CustomerRevenue_{DateTime.Now:yyyyMMdd}.pdf");
    }

    public async Task<IActionResult> OnGetExportCsvAsync()
    {
        if (StartDate.HasValue && EndDate.HasValue)
        {
            DateRange = ReportDateRange.Custom(StartDate.Value, EndDate.Value);
        }

        var customers = await _reportService.GetRevenueByCustomerAsync(DateRange, TopN);
        var csvBytes = _reportService.ExportToCsv(customers);

        return File(csvBytes, "text/csv", $"CustomerRevenue_{DateTime.Now:yyyyMMdd}.csv");
    }
}
