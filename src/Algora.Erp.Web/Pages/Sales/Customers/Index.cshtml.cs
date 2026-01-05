using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Sales;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Sales.Customers;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal TotalReceivables { get; set; }
    public int TotalSalesOrders { get; set; }

    public async Task OnGetAsync()
    {
        TotalCustomers = await _context.Customers.CountAsync();
        ActiveCustomers = await _context.Customers.CountAsync(c => c.IsActive);
        TotalReceivables = await _context.Customers.SumAsync(c => c.CurrentBalance);
        TotalSalesOrders = await _context.SalesOrders.CountAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? typeFilter, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.Customers
            .Include(c => c.SalesOrders)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Code.ToLower().Contains(search) ||
                (c.Email != null && c.Email.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<CustomerType>(typeFilter, out var customerType))
        {
            query = query.Where(c => c.CustomerType == customerType);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(c => c.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var customers = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_CustomersTableRows", new CustomersTableViewModel
        {
            Customers = customers,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public IActionResult OnGetCreateForm()
    {
        return Partial("_CustomerForm", new CustomerFormViewModel
        {
            IsEdit = false
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
            return NotFound();

        return Partial("_CustomerForm", new CustomerFormViewModel
        {
            IsEdit = true,
            Customer = customer
        });
    }

    public async Task<IActionResult> OnPostAsync(CustomerFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Customer? customer;

        if (input.Id.HasValue)
        {
            customer = await _context.Customers.FindAsync(input.Id.Value);
            if (customer == null)
                return NotFound();
        }
        else
        {
            customer = new Customer
            {
                Id = Guid.NewGuid(),
                Code = await GenerateCustomerCodeAsync()
            };
            _context.Customers.Add(customer);
        }

        customer.Name = input.Name;
        customer.CustomerType = input.CustomerType;
        customer.ContactPerson = input.ContactPerson;
        customer.Email = input.Email;
        customer.Phone = input.Phone;
        customer.Mobile = input.Mobile;
        customer.Website = input.Website;
        customer.BillingAddress = input.BillingAddress;
        customer.BillingCity = input.BillingCity;
        customer.BillingState = input.BillingState;
        customer.BillingCountry = input.BillingCountry;
        customer.BillingPostalCode = input.BillingPostalCode;
        customer.ShippingAddress = input.ShippingAddress;
        customer.ShippingCity = input.ShippingCity;
        customer.ShippingState = input.ShippingState;
        customer.ShippingCountry = input.ShippingCountry;
        customer.ShippingPostalCode = input.ShippingPostalCode;
        customer.TaxId = input.TaxId;
        customer.IsTaxExempt = input.IsTaxExempt;
        customer.PaymentTermsDays = input.PaymentTermsDays;
        customer.CreditLimit = input.CreditLimit;
        customer.Currency = input.Currency;
        customer.IsActive = input.IsActive;
        customer.Notes = input.Notes;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer == null)
            return NotFound();

        // Check if customer has orders
        var hasOrders = await _context.SalesOrders.AnyAsync(o => o.CustomerId == id);
        if (hasOrders)
        {
            return BadRequest("Cannot delete customer with existing orders.");
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateCustomerCodeAsync()
    {
        var lastCustomer = await _context.Customers
            .IgnoreQueryFilters()
            .OrderByDescending(c => c.Code)
            .FirstOrDefaultAsync(c => c.Code.StartsWith("CUS"));

        if (lastCustomer == null)
            return "CUS0001";

        var lastNumber = int.Parse(lastCustomer.Code.Replace("CUS", ""));
        return $"CUS{(lastNumber + 1):D4}";
    }
}

public class CustomersTableViewModel
{
    public List<Customer> Customers { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class CustomerFormViewModel
{
    public bool IsEdit { get; set; }
    public Customer? Customer { get; set; }
}

public class CustomerFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public CustomerType CustomerType { get; set; } = CustomerType.Company;
    public string? ContactPerson { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Website { get; set; }
    public string? BillingAddress { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? TaxId { get; set; }
    public bool IsTaxExempt { get; set; }
    public int PaymentTermsDays { get; set; } = 30;
    public decimal CreditLimit { get; set; }
    public string Currency { get; set; } = "USD";
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}
