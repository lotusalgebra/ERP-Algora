using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Finance.Payments;

public class ReceiptModel : PageModel
{
    private readonly IPaymentService _paymentService;

    public ReceiptModel(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);

        if (payment == null)
            return NotFound();

        var pdfBytes = _paymentService.GenerateReceiptPdf(payment);
        var fileName = $"Receipt_{payment.PaymentNumber.Replace("-", "_")}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
