using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop;

public class OrderConfirmationModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public OrderConfirmationModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public WebOrder? Order { get; set; }

    public async Task OnGetAsync(Guid id)
    {
        Order = await _context.WebOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);
    }
}
