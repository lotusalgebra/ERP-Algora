using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Shop.Account;

public class ProfileModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public ProfileModel(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public WebCustomer? Customer { get; set; }

    [BindProperty]
    public string FirstName { get; set; } = string.Empty;

    [BindProperty]
    public string LastName { get; set; } = string.Empty;

    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string? Phone { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var customerId = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out var id))
        {
            return RedirectToPage("Index");
        }

        Customer = await _context.WebCustomers.FindAsync(id);
        if (Customer == null)
        {
            return RedirectToPage("Index");
        }

        FirstName = Customer.FirstName;
        LastName = Customer.LastName;
        Email = Customer.Email;
        Phone = Customer.Phone;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var customerId = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out var id))
        {
            return RedirectToPage("Index");
        }

        Customer = await _context.WebCustomers.FindAsync(id);
        if (Customer == null)
        {
            return RedirectToPage("Index");
        }

        Customer.FirstName = FirstName;
        Customer.LastName = LastName;
        Customer.Email = Email;
        Customer.Phone = Phone;
        Customer.ModifiedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Profile updated successfully";
        return RedirectToPage();
    }
}
