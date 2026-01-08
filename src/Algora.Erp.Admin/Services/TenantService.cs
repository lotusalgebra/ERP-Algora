using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Admin.Services;

public interface ITenantService
{
    Task<List<Tenant>> GetTenantsAsync(string? search = null, TenantStatus? status = null, bool includeDeleted = false);
    Task<Tenant?> GetTenantByIdAsync(Guid id, bool includeDeleted = false);
    Task<Tenant?> GetTenantBySubdomainAsync(string subdomain);
    Task<Tenant> CreateTenantAsync(CreateTenantRequest request, Guid createdBy);
    Task<Tenant> UpdateTenantAsync(Guid id, UpdateTenantRequest request, Guid modifiedBy);
    Task<bool> SoftDeleteTenantAsync(Guid id, string reason, Guid deletedBy);
    Task<bool> RestoreTenantAsync(Guid id, Guid restoredBy);
    Task<bool> SuspendTenantAsync(Guid id, string reason, Guid suspendedBy);
    Task<bool> UnsuspendTenantAsync(Guid id, Guid unsuspendedBy);
    Task<bool> PermanentDeleteTenantAsync(Guid id);
    Task<TenantStats> GetTenantStatsAsync();
}

public class TenantService : ITenantService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<TenantService> _logger;

    public TenantService(AdminDbContext context, ILogger<TenantService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Tenant>> GetTenantsAsync(string? search = null, TenantStatus? status = null, bool includeDeleted = false)
    {
        var query = includeDeleted
            ? _context.Tenants.IgnoreQueryFilters()
            : _context.Tenants.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(search) ||
                t.Subdomain.ToLower().Contains(search) ||
                t.ContactEmail.ToLower().Contains(search) ||
                (t.CompanyName != null && t.CompanyName.ToLower().Contains(search)));
        }

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        return await query
            .Include(t => t.CurrentSubscription)
            .ThenInclude(s => s!.Plan)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Tenant?> GetTenantByIdAsync(Guid id, bool includeDeleted = false)
    {
        var query = includeDeleted
            ? _context.Tenants.IgnoreQueryFilters()
            : _context.Tenants.AsQueryable();

        return await query
            .Include(t => t.CurrentSubscription)
            .ThenInclude(s => s!.Plan)
            .Include(t => t.Subscriptions)
            .Include(t => t.Users)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Tenant?> GetTenantBySubdomainAsync(string subdomain)
    {
        return await _context.Tenants
            .Include(t => t.CurrentSubscription)
            .FirstOrDefaultAsync(t => t.Subdomain.ToLower() == subdomain.ToLower());
    }

    public async Task<Tenant> CreateTenantAsync(CreateTenantRequest request, Guid createdBy)
    {
        // Check subdomain uniqueness
        if (await _context.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Subdomain.ToLower() == request.Subdomain.ToLower()))
        {
            throw new InvalidOperationException($"Subdomain '{request.Subdomain}' is already taken");
        }

        var tenant = new Tenant
        {
            Name = request.Name,
            Subdomain = request.Subdomain.ToLower(),
            ContactEmail = request.ContactEmail,
            ContactPhone = request.ContactPhone,
            ContactPerson = request.ContactPerson,
            CompanyName = request.CompanyName,
            TaxId = request.TaxId,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PostalCode = request.PostalCode,
            CurrencyCode = request.CurrencyCode ?? "INR",
            TimeZone = request.TimeZone ?? "Asia/Kolkata",
            Status = TenantStatus.Pending,
            CreatedBy = createdBy,
            DatabaseName = !string.IsNullOrWhiteSpace(request.DatabaseName)
                ? request.DatabaseName
                : $"AlgoraErp_{request.Subdomain}"
        };

        // Set trial period
        if (request.StartWithTrial)
        {
            tenant.TrialStartedAt = DateTime.UtcNow;
            tenant.TrialEndsAt = DateTime.UtcNow.AddDays(request.TrialDays);
        }

        _context.Tenants.Add(tenant);

        // Create subscription if plan selected
        if (request.PlanId.HasValue)
        {
            var plan = await _context.BillingPlans.FindAsync(request.PlanId.Value);
            if (plan != null)
            {
                var subscription = new TenantSubscription
                {
                    TenantId = tenant.Id,
                    PlanId = plan.Id,
                    BillingCycle = request.BillingCycle,
                    StartDate = DateTime.UtcNow,
                    CurrentPeriodStart = DateTime.UtcNow,
                    CurrentPeriodEnd = GetPeriodEnd(DateTime.UtcNow, request.BillingCycle),
                    Status = request.StartWithTrial ? SubscriptionStatus.Trial : SubscriptionStatus.Active,
                    Amount = request.BillingCycle == BillingCycle.Annual ? plan.AnnualPrice : plan.MonthlyPrice,
                    Currency = plan.Currency,
                    IsTrialPeriod = request.StartWithTrial,
                    TrialEndDate = request.StartWithTrial ? DateTime.UtcNow.AddDays(request.TrialDays) : null,
                    AutoRenew = true,
                    NextBillingDate = request.StartWithTrial
                        ? DateTime.UtcNow.AddDays(request.TrialDays)
                        : GetPeriodEnd(DateTime.UtcNow, request.BillingCycle),
                    CreatedBy = createdBy
                };

                subscription.TotalAmount = subscription.Amount;
                _context.TenantSubscriptions.Add(subscription);

                tenant.CurrentSubscriptionId = subscription.Id;
                tenant.MaxUsers = plan.MaxUsers;
            }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new tenant: {TenantName} ({Subdomain})", tenant.Name, tenant.Subdomain);

        return tenant;
    }

    public async Task<Tenant> UpdateTenantAsync(Guid id, UpdateTenantRequest request, Guid modifiedBy)
    {
        var tenant = await _context.Tenants.FindAsync(id)
            ?? throw new InvalidOperationException("Tenant not found");

        tenant.Name = request.Name;
        tenant.ContactEmail = request.ContactEmail;
        tenant.ContactPhone = request.ContactPhone;
        tenant.ContactPerson = request.ContactPerson;
        tenant.CompanyName = request.CompanyName;
        tenant.TaxId = request.TaxId;
        tenant.LogoUrl = request.LogoUrl;
        tenant.PrimaryColor = request.PrimaryColor;
        tenant.Address = request.Address;
        tenant.City = request.City;
        tenant.State = request.State;
        tenant.Country = request.Country;
        tenant.PostalCode = request.PostalCode;
        tenant.CurrencyCode = request.CurrencyCode;
        tenant.TimeZone = request.TimeZone;
        tenant.MaxUsers = request.MaxUsers;
        tenant.ModifiedAt = DateTime.UtcNow;
        tenant.ModifiedBy = modifiedBy;

        await _context.SaveChangesAsync();
        return tenant;
    }

    public async Task<bool> SoftDeleteTenantAsync(Guid id, string reason, Guid deletedBy)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return false;

        tenant.IsDeleted = true;
        tenant.DeletedAt = DateTime.UtcNow;
        tenant.DeletedBy = deletedBy;
        tenant.DeletionReason = reason;
        tenant.Status = TenantStatus.Deleted;
        tenant.IsActive = false;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Soft deleted tenant: {TenantId}, Reason: {Reason}", id, reason);
        return true;
    }

    public async Task<bool> RestoreTenantAsync(Guid id, Guid restoredBy)
    {
        var tenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant == null) return false;

        tenant.IsDeleted = false;
        tenant.DeletedAt = null;
        tenant.DeletedBy = null;
        tenant.DeletionReason = null;
        tenant.Status = TenantStatus.Active;
        tenant.IsActive = true;
        tenant.ModifiedAt = DateTime.UtcNow;
        tenant.ModifiedBy = restoredBy;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Restored tenant: {TenantId}", id);
        return true;
    }

    public async Task<bool> SuspendTenantAsync(Guid id, string reason, Guid suspendedBy)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return false;

        tenant.IsSuspended = true;
        tenant.SuspendedAt = DateTime.UtcNow;
        tenant.SuspendedBy = suspendedBy;
        tenant.SuspensionReason = reason;
        tenant.Status = TenantStatus.Suspended;
        tenant.IsActive = false;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Suspended tenant: {TenantId}, Reason: {Reason}", id, reason);
        return true;
    }

    public async Task<bool> UnsuspendTenantAsync(Guid id, Guid unsuspendedBy)
    {
        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant == null) return false;

        tenant.IsSuspended = false;
        tenant.SuspendedAt = null;
        tenant.SuspendedBy = null;
        tenant.SuspensionReason = null;
        tenant.Status = TenantStatus.Active;
        tenant.IsActive = true;
        tenant.ModifiedAt = DateTime.UtcNow;
        tenant.ModifiedBy = unsuspendedBy;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Unsuspended tenant: {TenantId}", id);
        return true;
    }

    public async Task<bool> PermanentDeleteTenantAsync(Guid id)
    {
        var tenant = await _context.Tenants.IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tenant == null) return false;

        _context.Tenants.Remove(tenant);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Permanently deleted tenant: {TenantId}", id);
        return true;
    }

    public async Task<TenantStats> GetTenantStatsAsync()
    {
        var allTenants = await _context.Tenants.IgnoreQueryFilters().ToListAsync();

        return new TenantStats
        {
            TotalTenants = allTenants.Count(t => !t.IsDeleted),
            ActiveTenants = allTenants.Count(t => t.Status == TenantStatus.Active && !t.IsDeleted),
            TrialTenants = allTenants.Count(t => t.IsInTrial && !t.IsDeleted),
            SuspendedTenants = allTenants.Count(t => t.Status == TenantStatus.Suspended && !t.IsDeleted),
            DeletedTenants = allTenants.Count(t => t.IsDeleted),
            NewThisMonth = allTenants.Count(t => t.CreatedAt.Month == DateTime.UtcNow.Month &&
                                                  t.CreatedAt.Year == DateTime.UtcNow.Year && !t.IsDeleted)
        };
    }

    private static DateTime GetPeriodEnd(DateTime start, BillingCycle cycle)
    {
        return cycle switch
        {
            BillingCycle.Monthly => start.AddMonths(1),
            BillingCycle.Quarterly => start.AddMonths(3),
            BillingCycle.SemiAnnual => start.AddMonths(6),
            BillingCycle.Annual => start.AddYears(1),
            _ => start.AddMonths(1)
        };
    }
}

public class CreateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string Subdomain { get; set; } = string.Empty;
    public string? DatabaseName { get; set; }
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactPerson { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? TimeZone { get; set; }
    public Guid? PlanId { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
    public bool StartWithTrial { get; set; } = true;
    public int TrialDays { get; set; } = 14;
}

public class UpdateTenantRequest
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string? ContactPhone { get; set; }
    public string? ContactPerson { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxId { get; set; }
    public string? LogoUrl { get; set; }
    public string? PrimaryColor { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? CurrencyCode { get; set; }
    public string? TimeZone { get; set; }
    public int MaxUsers { get; set; }
}

public class TenantStats
{
    public int TotalTenants { get; set; }
    public int ActiveTenants { get; set; }
    public int TrialTenants { get; set; }
    public int SuspendedTenants { get; set; }
    public int DeletedTenants { get; set; }
    public int NewThisMonth { get; set; }
}
