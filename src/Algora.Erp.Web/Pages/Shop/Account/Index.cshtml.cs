using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop.Account;

public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public IndexModel(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public WebCustomer? Customer { get; set; }
    public List<WebOrder> RecentOrders { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Get customer from session/cookie
        var customerId = HttpContext.Session.GetString("CustomerId");
        if (!string.IsNullOrEmpty(customerId) && Guid.TryParse(customerId, out var id))
        {
            Customer = await _context.WebCustomers.FindAsync(id);

            if (Customer != null)
            {
                RecentOrders = await _context.WebOrders
                    .Where(o => o.CustomerId == Customer.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .Take(5)
                    .ToListAsync();
            }
        }
    }

    public async Task<IActionResult> OnPostLoginAsync(string loginEmail, string loginPassword)
    {
        // Simple email-based login (no password check for demo)
        var customer = await _context.WebCustomers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == loginEmail.ToLower() && c.IsActive);

        if (customer != null)
        {
            HttpContext.Session.SetString("CustomerId", customer.Id.ToString());
            return RedirectToPage();
        }

        TempData["Error"] = "Invalid email or password";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRegisterAsync(string firstName, string lastName, string email, string password)
    {
        // Check if email already exists
        var existing = await _context.WebCustomers
            .AnyAsync(c => c.Email.ToLower() == email.ToLower());

        if (existing)
        {
            TempData["Error"] = "An account with this email already exists";
            return RedirectToPage();
        }

        var customer = new WebCustomer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            IsActive = true,
            CreatedAt = _dateTime.UtcNow
        };

        _context.WebCustomers.Add(customer);
        await _context.SaveChangesAsync();

        HttpContext.Session.SetString("CustomerId", customer.Id.ToString());
        return RedirectToPage();
    }
}
