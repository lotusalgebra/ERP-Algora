using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Admin.Services;

public interface IModuleService
{
    Task<List<PlanModule>> GetAllModulesAsync(bool includeInactive = false);
    Task<List<PlanModule>> GetModulesByCategoryAsync(string category);
    Task<PlanModule?> GetModuleByIdAsync(Guid id);
    Task<PlanModule?> GetModuleByCodeAsync(string code);
    Task<PlanModule> CreateModuleAsync(CreateModuleRequest request, Guid createdBy);
    Task<bool> UpdateModuleAsync(Guid id, UpdateModuleRequest request, Guid updatedBy);
    Task<bool> ToggleModuleStatusAsync(Guid id, Guid updatedBy);
    Task<bool> DeleteModuleAsync(Guid id);
    Task<List<string>> GetCategoriesAsync();
    Task<decimal> CalculatePlanPriceAsync(List<Guid> moduleIds, bool annual = false);
}

public class ModuleService : IModuleService
{
    private readonly AdminDbContext _context;
    private readonly ILogger<ModuleService> _logger;

    public ModuleService(AdminDbContext context, ILogger<ModuleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PlanModule>> GetAllModulesAsync(bool includeInactive = false)
    {
        var query = _context.PlanModules.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(m => m.IsActive);
        }

        return await query.OrderBy(m => m.DisplayOrder).ToListAsync();
    }

    public async Task<List<PlanModule>> GetModulesByCategoryAsync(string category)
    {
        return await _context.PlanModules
            .Where(m => m.Category == category && m.IsActive)
            .OrderBy(m => m.DisplayOrder)
            .ToListAsync();
    }

    public async Task<PlanModule?> GetModuleByIdAsync(Guid id)
    {
        return await _context.PlanModules.FindAsync(id);
    }

    public async Task<PlanModule?> GetModuleByCodeAsync(string code)
    {
        return await _context.PlanModules
            .FirstOrDefaultAsync(m => m.Code == code);
    }

    public async Task<PlanModule> CreateModuleAsync(CreateModuleRequest request, Guid createdBy)
    {
        // Check for duplicate code
        if (await _context.PlanModules.AnyAsync(m => m.Code == request.Code))
        {
            throw new InvalidOperationException($"Module with code '{request.Code}' already exists");
        }

        var module = new PlanModule
        {
            Code = request.Code.ToLowerInvariant(),
            Name = request.Name,
            Description = request.Description,
            Icon = request.Icon ?? "bi-box",
            MonthlyPrice = request.MonthlyPrice,
            AnnualPrice = request.AnnualPrice,
            Currency = request.Currency ?? "INR",
            IsCore = request.IsCore,
            RequiredModules = request.RequiredModules,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
            Category = request.Category ?? "Core",
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.PlanModules.Add(module);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Module {ModuleCode} created by {UserId}", module.Code, createdBy);

        return module;
    }

    public async Task<bool> UpdateModuleAsync(Guid id, UpdateModuleRequest request, Guid updatedBy)
    {
        var module = await _context.PlanModules.FindAsync(id);
        if (module == null)
        {
            return false;
        }

        // Check for duplicate code if changed
        if (request.Code != module.Code &&
            await _context.PlanModules.AnyAsync(m => m.Code == request.Code && m.Id != id))
        {
            throw new InvalidOperationException($"Module with code '{request.Code}' already exists");
        }

        module.Code = request.Code.ToLowerInvariant();
        module.Name = request.Name;
        module.Description = request.Description;
        module.Icon = request.Icon ?? "bi-box";
        module.MonthlyPrice = request.MonthlyPrice;
        module.AnnualPrice = request.AnnualPrice;
        module.Currency = request.Currency ?? "INR";
        module.IsCore = request.IsCore;
        module.RequiredModules = request.RequiredModules;
        module.DisplayOrder = request.DisplayOrder;
        module.IsActive = request.IsActive;
        module.Category = request.Category ?? "Core";
        module.ModifiedAt = DateTime.UtcNow;
        module.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Module {ModuleCode} updated by {UserId}", module.Code, updatedBy);

        return true;
    }

    public async Task<bool> ToggleModuleStatusAsync(Guid id, Guid updatedBy)
    {
        var module = await _context.PlanModules.FindAsync(id);
        if (module == null)
        {
            return false;
        }

        module.IsActive = !module.IsActive;
        module.ModifiedAt = DateTime.UtcNow;
        module.ModifiedBy = updatedBy;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Module {ModuleCode} status toggled to {IsActive} by {UserId}",
            module.Code, module.IsActive, updatedBy);

        return true;
    }

    public async Task<bool> DeleteModuleAsync(Guid id)
    {
        var module = await _context.PlanModules.FindAsync(id);
        if (module == null)
        {
            return false;
        }

        // Check if module is used in any plan
        var usageCount = await _context.BillingPlanModules.CountAsync(pm => pm.ModuleId == id);
        if (usageCount > 0)
        {
            throw new InvalidOperationException(
                $"Cannot delete module '{module.Name}'. It is used in {usageCount} billing plans.");
        }

        _context.PlanModules.Remove(module);
        await _context.SaveChangesAsync();

        _logger.LogWarning("Module {ModuleCode} deleted", module.Code);

        return true;
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        return await _context.PlanModules
            .Select(m => m.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    public async Task<decimal> CalculatePlanPriceAsync(List<Guid> moduleIds, bool annual = false)
    {
        var modules = await _context.PlanModules
            .Where(m => moduleIds.Contains(m.Id) && m.IsActive)
            .ToListAsync();

        return annual
            ? modules.Sum(m => m.AnnualPrice)
            : modules.Sum(m => m.MonthlyPrice);
    }
}

public class CreateModuleRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal AnnualPrice { get; set; }
    public string? Currency { get; set; }
    public bool IsCore { get; set; }
    public string? RequiredModules { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Category { get; set; }
}

public class UpdateModuleRequest : CreateModuleRequest
{
}
