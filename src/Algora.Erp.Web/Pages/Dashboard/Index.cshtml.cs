using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.HR;
using Algora.Erp.Domain.Entities.Manufacturing;
using Algora.Erp.Domain.Entities.Payroll;
using Algora.Erp.Domain.Entities.Procurement;
using Algora.Erp.Domain.Entities.Projects;
using Algora.Erp.Domain.Entities.Sales;
using Algora.Erp.Domain.Enums;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Dashboard;

public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    // Summary Stats
    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalProducts { get; set; }
    public int TotalEmployees { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalSuppliers { get; set; }

    // Module Stats
    public int ActiveProjects { get; set; }
    public int PendingPurchaseOrders { get; set; }
    public int OpenWorkOrders { get; set; }
    public int PendingPayrollRuns { get; set; }
    public decimal TotalBillableHours { get; set; }
    public int LowStockProducts { get; set; }

    // Recent Data
    public List<SalesOrder> RecentOrders { get; set; } = new();
    public List<Project> ActiveProjectsList { get; set; } = new();

    // Alerts
    public int PendingLeaveRequests { get; set; }
    public int OverdueInvoices { get; set; }
    public int UpcomingMilestones { get; set; }

    public async Task OnGetAsync()
    {
        // Summary Stats
        TotalRevenue = await _context.SalesOrders
            .Where(o => o.Status >= SalesOrderStatus.Confirmed)
            .SumAsync(o => o.TotalAmount);

        TotalOrders = await _context.SalesOrders.CountAsync();
        TotalProducts = await _context.Products.CountAsync();
        TotalEmployees = await _context.Employees
            .Where(e => e.EmploymentStatus == EmploymentStatus.Active)
            .CountAsync();
        TotalCustomers = await _context.Customers.Where(c => c.IsActive).CountAsync();
        TotalSuppliers = await _context.Suppliers.Where(s => s.IsActive).CountAsync();

        // Module Stats
        ActiveProjects = await _context.Projects
            .Where(p => p.Status == ProjectStatus.Active)
            .CountAsync();

        PendingPurchaseOrders = await _context.PurchaseOrders
            .Where(po => po.Status == PurchaseOrderStatus.Pending || po.Status == PurchaseOrderStatus.Approved)
            .CountAsync();

        OpenWorkOrders = await _context.WorkOrders
            .Where(wo => wo.Status == WorkOrderStatus.Released || wo.Status == WorkOrderStatus.InProgress)
            .CountAsync();

        PendingPayrollRuns = await _context.PayrollRuns
            .Where(pr => pr.Status == PayrollRunStatus.Draft || pr.Status == PayrollRunStatus.Processing)
            .CountAsync();

        TotalBillableHours = await _context.TimeEntries
            .Where(t => t.IsBillable)
            .SumAsync(t => t.Hours);

        LowStockProducts = await _context.Products
            .Where(p => _context.StockLevels
                .Where(s => s.ProductId == p.Id)
                .Sum(s => s.QuantityOnHand) <= p.ReorderLevel)
            .CountAsync();

        // Recent Orders
        RecentOrders = await _context.SalesOrders
            .Include(o => o.Customer)
            .OrderByDescending(o => o.OrderDate)
            .Take(5)
            .ToListAsync();

        // Active Projects
        ActiveProjectsList = await _context.Projects
            .Include(p => p.Customer)
            .Include(p => p.ProjectManager)
            .Where(p => p.Status == ProjectStatus.Active)
            .OrderByDescending(p => p.StartDate)
            .Take(5)
            .ToListAsync();

        // Alerts
        PendingLeaveRequests = await _context.LeaveRequests
            .Where(lr => lr.Status == LeaveStatus.Pending)
            .CountAsync();

        UpcomingMilestones = await _context.ProjectMilestones
            .Where(m => m.Status == MilestoneStatus.Pending || m.Status == MilestoneStatus.InProgress)
            .Where(m => m.DueDate <= DateTime.UtcNow.AddDays(7))
            .CountAsync();
    }
}
