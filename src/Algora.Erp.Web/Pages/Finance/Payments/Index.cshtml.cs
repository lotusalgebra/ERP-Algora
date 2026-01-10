using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Algora.Erp.Web.Pages.Finance.Payments;

public class IndexModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public IndexModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public PaymentListResult PaymentResult { get; set; } = new();
    public PaymentStatistics Statistics { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public new int Page { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 20;

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public PaymentMethod? PaymentMethod { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string SortBy { get; set; } = "PaymentDate";

    [BindProperty(SupportsGet = true)]
    public bool SortDescending { get; set; } = true;

    public List<SelectListItem> PaymentMethodOptions { get; set; } = new();

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = PaymentResult.TotalCount,
        PageUrl = "/Finance/Payments",
        Handler = "TableRows",
        HxTarget = "#tableContent",
        HxInclude = "#searchInput,#statusFilter"
    };

    public async Task OnGetAsync()
    {
        // Get payments
        PaymentResult = await _paymentService.GetPaymentsAsync(new PaymentListRequest
        {
            Page = Page,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            PaymentMethod = PaymentMethod,
            FromDate = FromDate,
            ToDate = ToDate,
            SortBy = SortBy,
            SortDescending = SortDescending
        });

        // Get statistics for current month by default
        var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Statistics = await _paymentService.GetPaymentStatisticsAsync(startOfMonth);

        // Payment method dropdown
        PaymentMethodOptions = Enum.GetValues<PaymentMethod>()
            .Select(m => new SelectListItem { Value = ((int)m).ToString(), Text = m.ToString() })
            .ToList();
        PaymentMethodOptions.Insert(0, new SelectListItem { Value = "", Text = "All Methods" });
    }

    public async Task<IActionResult> OnGetTableRowsAsync()
    {
        PaymentResult = await _paymentService.GetPaymentsAsync(new PaymentListRequest
        {
            Page = Page,
            PageSize = PageSize,
            SearchTerm = SearchTerm,
            PaymentMethod = PaymentMethod,
            FromDate = FromDate,
            ToDate = ToDate,
            SortBy = SortBy,
            SortDescending = SortDescending
        });

        return Partial("_PaymentsTableRows", this);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid paymentId)
    {
        try
        {
            await _paymentService.DeletePaymentAsync(paymentId);
            TempData["Success"] = "Payment deleted successfully.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete payment: {ex.Message}";
        }

        return RedirectToPage();
    }
}
