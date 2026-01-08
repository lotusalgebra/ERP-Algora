using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Admin.Services;

public interface IPlanService
{
    Task<List<BillingPlan>> GetAllPlansAsync(bool includeInactive = false);
    Task<BillingPlan?> GetPlanByIdAsync(Guid id);
    Task<BillingPlan?> GetPlanByCodeAsync(string code);
    Task<BillingPlan> CreatePlanAsync(CreatePlanRequest request, Guid createdBy);
    Task<bool> UpdatePlanAsync(Guid id, UpdatePlanRequest request, Guid updatedBy);
    Task<bool> TogglePlanStatusAsync(Guid id, Guid updatedBy);
    Task<bool> DeletePlanAsync(Guid id);
    Task<PlanUsageStats> GetPlanUsageStatsAsync(Guid planId);
}

public class PlanService : IPlanService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<PlanService> _logger;

    public PlanService(AdminDbContext context, ILogger<PlanService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<BillingPlan>> GetAllPlansAsync(bool includeInactive = false)
    {
        var query = _context.BillingPlans.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        return await query.OrderBy(p => p.DisplayOrder).ToListAsync();
    }

    public async Task<BillingPlan?> GetPlanByIdAsync(Guid id)
    {
        return await _context.BillingPlans.FindAsync(id);
    }

    public async Task<BillingPlan?> GetPlanByCodeAsync(string code)
    {
        return await _context.BillingPlans
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<BillingPlan> CreatePlanAsync(CreatePlanRequest request, Guid createdBy)
    {
        // Check for duplicate code
        if (await _context.BillingPlans.AnyAsync(p => p.Code == request.Code))
        {
            throw new InvalidOperationException($"Plan with code '{request.Code}' already exists");
        }

        var plan = new BillingPlan
        {
            Code = request.Code.ToUpperInvariant(),
            Name = request.Name,
            Description = request.Description,
            MonthlyPrice = request.MonthlyPrice,
            AnnualPrice = request.AnnualPrice,
            MaxUsers = request.MaxUsers,
            MaxWarehouses = request.MaxWarehouses,
            MaxProducts = request.MaxProducts,
            MaxTransactionsPerMonth = request.MaxMonthlyTransactions,
            StorageLimitMB = request.StorageLimitMb,
            Features = request.Features ?? "[]",
            IncludedModules = request.IncludedModules ?? "[]",
            IsActive = request.IsActive,
            IsPopular = request.IsFeatured,
            DisplayOrder = request.SortOrder,
            CreatedAt = DateTime.UtcNow
        };

        _context.BillingPlans.Add(plan);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Plan {PlanCode} created by {UserId}", plan.Code, createdBy);

        return plan;
    }

    public async Task<bool> UpdatePlanAsync(Guid id, UpdatePlanRequest request, Guid updatedBy)
    {
        var plan = await _context.BillingPlans.FindAsync(id);
        if (plan == null)
        {
            return false;
        }

        // Check for duplicate code if changed
        if (request.Code != plan.Code &&
            await _context.BillingPlans.AnyAsync(p => p.Code == request.Code && p.Id != id))
        {
            throw new InvalidOperationException($"Plan with code '{request.Code}' already exists");
        }

        plan.Code = request.Code.ToUpperInvariant();
        plan.Name = request.Name;
        plan.Description = request.Description;
        plan.MonthlyPrice = request.MonthlyPrice;
        plan.AnnualPrice = request.AnnualPrice;
        plan.MaxUsers = request.MaxUsers;
        plan.MaxWarehouses = request.MaxWarehouses;
        plan.MaxProducts = request.MaxProducts;
        plan.MaxTransactionsPerMonth = request.MaxMonthlyTransactions;
        plan.StorageLimitMB = request.StorageLimitMb;
        plan.Features = request.Features ?? "[]";
        plan.IncludedModules = request.IncludedModules ?? "[]";
        plan.IsActive = request.IsActive;
        plan.IsPopular = request.IsFeatured;
        plan.DisplayOrder = request.SortOrder;
        plan.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Plan {PlanCode} updated by {UserId}", plan.Code, updatedBy);

        return true;
    }

    public async Task<bool> TogglePlanStatusAsync(Guid id, Guid updatedBy)
    {
        var plan = await _context.BillingPlans.FindAsync(id);
        if (plan == null)
        {
            return false;
        }

        plan.IsActive = !plan.IsActive;
        plan.ModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Plan {PlanCode} status toggled to {IsActive} by {UserId}",
            plan.Code, plan.IsActive, updatedBy);

        return true;
    }

    public async Task<bool> DeletePlanAsync(Guid id)
    {
        var plan = await _context.BillingPlans.FindAsync(id);
        if (plan == null)
        {
            return false;
        }

        // Check if plan is in use
        var subscriptionCount = await _context.TenantSubscriptions
            .CountAsync(s => s.PlanId == id && s.Status != SubscriptionStatus.Cancelled);

        if (subscriptionCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete plan '{plan.Name}'. It has {subscriptionCount} active subscriptions.");
        }

        _context.BillingPlans.Remove(plan);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Plan {PlanCode} deleted", plan.Code);

        return true;
    }

    public async Task<PlanUsageStats> GetPlanUsageStatsAsync(Guid planId)
    {
        var activeSubscriptions = await _context.TenantSubscriptions
            .CountAsync(s => s.PlanId == planId && s.Status == SubscriptionStatus.Active);

        var totalRevenue = await _context.TenantSubscriptions
            .Where(s => s.PlanId == planId)
            .SumAsync(s => s.Amount);

        var monthlyRevenue = await _context.TenantSubscriptions
            .Where(s => s.PlanId == planId && s.Status == SubscriptionStatus.Active)
            .SumAsync(s => s.Amount);

        return new PlanUsageStats
        {
            ActiveSubscriptions = activeSubscriptions,
            TotalRevenue = totalRevenue,
            MonthlyRecurringRevenue = monthlyRevenue
        };
    }
}

public class CreatePlanRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public int MaxUsers { get; set; }
    public int MaxWarehouses { get; set; }
    public int MaxProducts { get; set; }
    public int MaxMonthlyTransactions { get; set; }
    public int StorageLimitMb { get; set; }
    public string? Features { get; set; }
    public string? IncludedModules { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; }
}

public class UpdatePlanRequest : CreatePlanRequest
{
}

public class PlanUsageStats
{
    public int ActiveSubscriptions { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRecurringRevenue { get; set; }
}
