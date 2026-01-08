using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Admin.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly AdminDbContext _context;

    public IndexModel(AdminDbContext context)
    {
        _context = context;
    }

    public DashboardStats Stats { get; set; } = new();
    public List<Tenant> RecentTenants { get; set; } = new();
    public Dictionary<string, int> PlanDistribution { get; set; } = new();

    public async Task OnGetAsync()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // Get stats
        Stats.TotalTenants = await _context.Tenants.CountAsync();
        Stats.NewTenantsThisMonth = await _context.Tenants
            .Where(t => t.CreatedAt >= startOfMonth)
            .CountAsync();

        Stats.ActiveSubscriptions = await _context.TenantSubscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .CountAsync();

        Stats.TrialSubscriptions = await _context.TenantSubscriptions
            .Where(s => s.Status == SubscriptionStatus.Trial)
            .CountAsync();

        Stats.MonthlyRevenue = await _context.TenantSubscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .SumAsync(s => s.Amount);

        Stats.SuspendedTenants = await _context.Tenants
            .Where(t => t.IsSuspended)
            .CountAsync();

        Stats.PendingCancellations = await _context.TenantSubscriptions
            .Where(s => s.CancelAtPeriodEnd && s.Status == SubscriptionStatus.Active)
            .CountAsync();

        // Calculate revenue growth (simplified - just show 0 if no previous data)
        Stats.RevenueGrowthPercent = 0;

        // Get recent tenants
        RecentTenants = await _context.Tenants
            .Include(t => t.CurrentSubscription)
                .ThenInclude(s => s!.Plan)
            .OrderByDescending(t => t.CreatedAt)
            .Take(5)
            .ToListAsync();

        // Get plan distribution
        var planStats = await _context.TenantSubscriptions
            .Where(s => s.Status == SubscriptionStatus.Active)
            .Include(s => s.Plan)
            .GroupBy(s => s.Plan!.Name)
            .Select(g => new { PlanName = g.Key, Count = g.Count() })
            .ToListAsync();

        PlanDistribution = planStats.ToDictionary(p => p.PlanName, p => p.Count);
    }
}

public class DashboardStats
{
    public int TotalTenants { get; set; }
    public int NewTenantsThisMonth { get; set; }
    public int ActiveSubscriptions { get; set; }
    public int TrialSubscriptions { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal RevenueGrowthPercent { get; set; }
    public int SuspendedTenants { get; set; }
    public int PendingCancellations { get; set; }
}
