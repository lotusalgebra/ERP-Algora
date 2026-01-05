using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Payments;

public class DetailsModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public DetailsModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public InvoicePayment Payment { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);

        if (payment == null)
            return NotFound();

        Payment = payment;
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        try
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
                return NotFound();

            var invoiceId = payment.InvoiceId;
            await _paymentService.DeletePaymentAsync(id);

            TempData["Success"] = "Payment deleted successfully.";
            return RedirectToPage("/Finance/Invoices/Details", new { id = invoiceId });
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to delete payment: {ex.Message}";
            return RedirectToPage(new { id });
        }
    }
}
