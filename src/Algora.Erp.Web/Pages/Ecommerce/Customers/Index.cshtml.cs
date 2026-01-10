using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Algora.Erp.Web.Pages.Ecommerce.Customers;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public IndexModel(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int CustomersWithOrders { get; set; }
    public int NewThisMonth { get; set; }

    public async Task OnGetAsync()
    {
        TotalCustomers = await _context.WebCustomers.CountAsync();
        ActiveCustomers = await _context.WebCustomers.CountAsync(c => c.IsActive);
        CustomersWithOrders = await _context.WebCustomers.CountAsync(c => c.OrderCount > 0);

        var monthStart = new DateTime(_dateTime.UtcNow.Year, _dateTime.UtcNow.Month, 1);
        NewThisMonth = await _context.WebCustomers.CountAsync(c => c.CreatedAt >= monthStart);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.WebCustomers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search) ||
                (c.Phone != null && c.Phone.Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(c => c.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var customers = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_CustomersTableRows", new WebCustomersTableViewModel
        {
            Customers = customers,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        return Partial("_CustomerForm", new WebCustomerFormViewModel { IsEdit = false });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var customer = await _context.WebCustomers.FindAsync(id);
        if (customer == null)
            return NotFound();

        return Partial("_CustomerForm", new WebCustomerFormViewModel
        {
            IsEdit = true,
            Customer = customer
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var customer = await _context.WebCustomers
            .Include(c => c.Orders.OrderByDescending(o => o.OrderDate).Take(10))
                .ThenInclude(o => o.Items)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound();

        return Partial("_CustomerDetails", customer);
    }

    public async Task<IActionResult> OnPostAsync(Guid? id, string firstName, string lastName, string email,
        string? phone, string? address, string? city, string? state,
        string? postalCode, string? country, string? notes, bool isActive = true)
    {
        WebCustomer customer;

        if (id.HasValue)
        {
            customer = await _context.WebCustomers.FindAsync(id.Value);
            if (customer == null)
                return NotFound();
        }
        else
        {
            customer = new WebCustomer { Id = Guid.NewGuid() };
            _context.WebCustomers.Add(customer);
        }

        customer.FirstName = firstName;
        customer.LastName = lastName;
        customer.Email = email;
        customer.Phone = phone;
        customer.Address = address;
        customer.City = city;
        customer.State = state;
        customer.PostalCode = postalCode;
        customer.Country = country;
        customer.Notes = notes;
        customer.IsActive = isActive;

        if (!id.HasValue)
        {
            customer.CreatedAt = _dateTime.UtcNow;
        }
        customer.ModifiedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var customer = await _context.WebCustomers.FindAsync(id);
        if (customer != null)
        {
            _context.WebCustomers.Remove(customer);
            await _context.SaveChangesAsync();
        }

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnGetExportAsync(string? search, string? statusFilter)
    {
        var query = _context.WebCustomers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(search) ||
                c.LastName.ToLower().Contains(search) ||
                c.Email.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(c => c.IsActive == isActive);
        }

        var customers = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Name,Email,Phone,City,Country,Orders,Total Spent,Status,Joined");

        foreach (var c in customers)
        {
            csv.AppendLine($"\"{c.FirstName} {c.LastName}\",\"{c.Email}\",\"{c.Phone}\",\"{c.City}\",\"{c.Country}\",{c.OrderCount},{c.TotalSpent:F2},{(c.IsActive ? "Active" : "Inactive")},{c.CreatedAt:yyyy-MM-dd}");
        }

        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", $"customers-{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}

public class WebCustomersTableViewModel
{
    public List<WebCustomer> Customers { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Ecommerce/Customers",
        Handler = "Table",
        HxTarget = "#customersTableBody",
        HxInclude = "#searchInput,#statusFilter"
    };
}

public class WebCustomerFormViewModel
{
    public bool IsEdit { get; set; }
    public WebCustomer? Customer { get; set; }
}
