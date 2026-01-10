using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Procurement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Procurement.Suppliers;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalSuppliers { get; set; }
    public int ActiveSuppliers { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int TotalPurchaseOrders { get; set; }

    public async Task OnGetAsync()
    {
        TotalSuppliers = await _context.Suppliers.CountAsync();
        ActiveSuppliers = await _context.Suppliers.CountAsync(s => s.IsActive);
        TotalOutstanding = await _context.Suppliers.SumAsync(s => s.CurrentBalance);
        TotalPurchaseOrders = await _context.PurchaseOrders.CountAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.Suppliers
            .Include(s => s.PurchaseOrders)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(s =>
                s.Name.ToLower().Contains(search) ||
                s.Code.ToLower().Contains(search) ||
                (s.Email != null && s.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(s => s.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var suppliers = await query
            .OrderBy(s => s.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_SuppliersTableRows", new SuppliersTableViewModel
        {
            Suppliers = suppliers,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public IActionResult OnGetCreateForm()
    {
        return Partial("_SupplierForm", new SupplierFormViewModel
        {
            IsEdit = false
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var supplier = await _context.Suppliers.FirstOrDefaultAsync(s => s.Id == id);

        if (supplier == null)
            return NotFound();

        return Partial("_SupplierForm", new SupplierFormViewModel
        {
            IsEdit = true,
            Supplier = supplier
        });
    }

    public async Task<IActionResult> OnPostAsync(SupplierFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Supplier? supplier;

        if (input.Id.HasValue)
        {
            supplier = await _context.Suppliers.FindAsync(input.Id.Value);
            if (supplier == null)
                return NotFound();
        }
        else
        {
            supplier = new Supplier
            {
                Id = Guid.NewGuid(),
                Code = await GenerateSupplierCodeAsync()
            };
            _context.Suppliers.Add(supplier);
        }

        supplier.Name = input.Name;
        supplier.ContactPerson = input.ContactPerson;
        supplier.Email = input.Email;
        supplier.Phone = input.Phone;
        supplier.Fax = input.Fax;
        supplier.Website = input.Website;
        supplier.Address = input.Address;
        supplier.City = input.City;
        supplier.State = input.State;
        supplier.Country = input.Country;
        supplier.PostalCode = input.PostalCode;
        supplier.BankName = input.BankName;
        supplier.BankAccountNumber = input.BankAccountNumber;
        supplier.BankRoutingNumber = input.BankRoutingNumber;
        supplier.TaxId = input.TaxId;
        supplier.PaymentTermsDays = input.PaymentTermsDays;
        supplier.Currency = input.Currency;
        supplier.LeadTimeDays = input.LeadTimeDays;
        supplier.MinimumOrderAmount = input.MinimumOrderAmount;
        supplier.IsActive = input.IsActive;
        supplier.Notes = input.Notes;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier == null)
            return NotFound();

        // Check if supplier has purchase orders
        var hasPOs = await _context.PurchaseOrders.AnyAsync(p => p.SupplierId == id);
        if (hasPOs)
        {
            return BadRequest("Cannot delete supplier with existing purchase orders.");
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }

    private async Task<string> GenerateSupplierCodeAsync()
    {
        var lastSupplier = await _context.Suppliers
            .IgnoreQueryFilters()
            .OrderByDescending(s => s.Code)
            .FirstOrDefaultAsync(s => s.Code.StartsWith("SUP"));

        if (lastSupplier == null)
            return "SUP0001";

        var lastNumber = int.Parse(lastSupplier.Code.Replace("SUP", ""));
        return $"SUP{(lastNumber + 1):D4}";
    }
}

public class SuppliersTableViewModel
{
    public List<Supplier> Suppliers { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Procurement/Suppliers",
        Handler = "Table",
        HxTarget = "#tableContent",
        HxInclude = "#searchInput,#statusFilter,#pageSizeSelect"
    };
}

public class SupplierFormViewModel
{
    public bool IsEdit { get; set; }
    public Supplier? Supplier { get; set; }
}

public class SupplierFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Website { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string? TaxId { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public string Currency { get; set; } = "USD";
    public int LeadTimeDays { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
