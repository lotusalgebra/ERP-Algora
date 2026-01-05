using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices;

public class DownloadModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IInvoicePdfService _pdfService;

    public DownloadModel(IApplicationDbContext context, IInvoicePdfService pdfService)
    {
        _context = context;
        _pdfService = pdfService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound();

        var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);

        var fileName = $"Invoice_{invoice.InvoiceNumber.Replace("-", "_")}_{DateTime.Now:yyyyMMdd}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }
}
