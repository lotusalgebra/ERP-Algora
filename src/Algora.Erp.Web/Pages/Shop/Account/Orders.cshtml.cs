using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop.Account;

public class OrdersModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public OrdersModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public WebCustomer? Customer { get; set; }
    public List<WebOrder> Orders { get; set; } = new();
    public WebOrder? Order { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid? id)
    {
        var customerId = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out var custId))
        {
            return RedirectToPage("/Shop/Account/Index");
        }

        Customer = await _context.WebCustomers.FindAsync(custId);
        if (Customer == null)
        {
            return RedirectToPage("/Shop/Account/Index");
        }

        if (id.HasValue)
        {
            // View single order
            Order = await _context.WebOrders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id.Value && o.CustomerId == Customer.Id);

            if (Order == null)
            {
                return RedirectToPage("/Shop/Account/Orders");
            }
        }
        else
        {
            // List all orders
            Orders = await _context.WebOrders
                .Include(o => o.Items)
                .Where(o => o.CustomerId == Customer.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        return Page();
    }
}
