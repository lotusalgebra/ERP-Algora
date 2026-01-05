using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices;

public class PrintModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public PrintModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public Invoice Invoice { get; set; } = null!;
    public List<InvoicePayment> Payments { get; set; } = new();

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

        Invoice = invoice;
        Payments = invoice.Payments.OrderBy(p => p.PaymentDate).ToList();

        return Page();
    }
}
