using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop.Account;

public class AddressesModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public AddressesModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public WebCustomer? Customer { get; set; }
    public List<CustomerAddress> Addresses { get; set; } = new();

    [BindProperty]
    public CustomerAddress NewAddress { get; set; } = new();

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

        Addresses = await _context.CustomerAddresses
            .Where(a => a.CustomerId == id)
            .OrderByDescending(a => a.IsDefault)
            .ThenBy(a => a.Label)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var customerId = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out var id))
        {
            return RedirectToPage("Index");
        }

        NewAddress.Id = Guid.NewGuid();
        NewAddress.CustomerId = id;

        // If this is the first address or marked as default, set it as default
        var hasAddresses = await _context.CustomerAddresses.AnyAsync(a => a.CustomerId == id);
        if (!hasAddresses || NewAddress.IsDefault)
        {
            // Clear other defaults
            var existingDefaults = await _context.CustomerAddresses
                .Where(a => a.CustomerId == id && a.IsDefault)
                .ToListAsync();
            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
            }
            NewAddress.IsDefault = true;
        }

        _context.CustomerAddresses.Add(NewAddress);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Address added successfully";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid addressId)
    {
        var customerId = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out var id))
        {
            return RedirectToPage("Index");
        }

        var address = await _context.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.CustomerId == id);

        if (address != null)
        {
            _context.CustomerAddresses.Remove(address);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Address deleted successfully";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSetDefaultAsync(Guid addressId)
    {
        var customerId = HttpContext.Session.GetString("CustomerId");
        if (string.IsNullOrEmpty(customerId) || !Guid.TryParse(customerId, out var id))
        {
            return RedirectToPage("Index");
        }

        // Clear all defaults
        var addresses = await _context.CustomerAddresses
            .Where(a => a.CustomerId == id)
            .ToListAsync();

        foreach (var addr in addresses)
        {
            addr.IsDefault = addr.Id == addressId;
        }

        await _context.SaveChangesAsync();
        TempData["Success"] = "Default address updated";
        return RedirectToPage();
    }
}
