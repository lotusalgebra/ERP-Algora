using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Ecommerce.Orders;

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

    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public decimal TodaysRevenue { get; set; }

    public async Task OnGetAsync()
    {
        TotalOrders = await _context.WebOrders.CountAsync();
        PendingOrders = await _context.WebOrders.CountAsync(o => o.Status == WebOrderStatus.Pending);
        ProcessingOrders = await _context.WebOrders.CountAsync(o => o.Status == WebOrderStatus.Processing);

        var today = _dateTime.UtcNow.Date;
        TodaysRevenue = await _context.WebOrders
            .Where(o => o.OrderDate >= today && o.Status != WebOrderStatus.Cancelled && o.Status != WebOrderStatus.Refunded)
            .SumAsync(o => o.Total);
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.WebOrders
            .Include(o => o.Customer)
            .Include(o => o.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(search) ||
                (o.Customer != null && (o.Customer.FirstName.ToLower().Contains(search) ||
                                        o.Customer.LastName.ToLower().Contains(search) ||
                                        o.Customer.Email.ToLower().Contains(search))) ||
                o.ShippingFirstName.ToLower().Contains(search) ||
                o.ShippingLastName.ToLower().Contains(search) ||
                o.CustomerEmail.ToLower().Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<WebOrderStatus>(statusFilter, out var status))
        {
            query = query.Where(o => o.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var orders = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_OrdersTableRows", new WebOrdersTableViewModel
        {
            Orders = orders,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetDetailsAsync(Guid id)
    {
        var order = await _context.WebOrders
            .Include(o => o.Customer)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return NotFound();

        return Partial("_OrderDetails", order);
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(Guid id, string status)
    {
        if (!Enum.TryParse<WebOrderStatus>(status, out var orderStatus))
            return BadRequest("Invalid status");

        var order = await _context.WebOrders.FindAsync(id);
        if (order == null)
            return NotFound();

        order.Status = orderStatus;

        if (orderStatus == WebOrderStatus.Cancelled)
        {
            order.CancelledAt = _dateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null);
    }
}

public class WebOrdersTableViewModel
{
    public List<WebOrder> Orders { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Ecommerce/Orders",
        Handler = "Table",
        HxTarget = "#ordersTableBody",
        HxInclude = "#searchInput,#statusFilter,#customerFilter,#pageSizeSelect"
    };
}
