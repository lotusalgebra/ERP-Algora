using Algora.Erp.Admin.Entities;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;

namespace Algora.Erp.Admin.Pages.Plans;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IPlanService _planService;
    private readonly IModuleService _moduleService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IPlanService planService, IModuleService moduleService, ILogger<IndexModel> logger)
    {
        _planService = planService;
        _moduleService = moduleService;
        _logger = logger;
    }

    public List<BillingPlan> Plans { get; set; } = new();
    public List<PlanModule> Modules { get; set; } = new();
    public Dictionary<Guid, int> PlanSubscriptions { get; set; } = new();
    public decimal TotalMRR { get; set; }
    public int TotalSubscriptions { get; set; }

    [BindProperty]
    public PlanInput Input { get; set; } = new();

    [BindProperty]
    public Guid? PlanId { get; set; }

    [BindProperty]
    public List<Guid> SelectedModules { get; set; } = new();

    public async Task OnGetAsync()
    {
        Plans = await _planService.GetAllPlansAsync(includeInactive: true);
        Modules = await _moduleService.GetAllModulesAsync(includeInactive: false);

        // Get subscription counts for each plan
        foreach (var plan in Plans)
        {
            var stats = await _planService.GetPlanUsageStatsAsync(plan.Id);
            PlanSubscriptions[plan.Id] = stats.ActiveSubscriptions;
            TotalMRR += stats.MonthlyRecurringRevenue;
            TotalSubscriptions += stats.ActiveSubscriptions;
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            return BadRequest(new { error = string.Join(", ", errors) });
        }

        try
        {
            var userId = GetCurrentUserId();

            // Calculate prices based on selected modules
            var monthlyPrice = Input.MonthlyPrice;
            var annualPrice = Input.AnnualPrice;

            if (SelectedModules.Any())
            {
                monthlyPrice = await _moduleService.CalculatePlanPriceAsync(SelectedModules, false);
                annualPrice = await _moduleService.CalculatePlanPriceAsync(SelectedModules, true);
            }

            // Convert selected modules to JSON
            var modulesJson = JsonSerializer.Serialize(SelectedModules);

            if (PlanId.HasValue && PlanId.Value != Guid.Empty)
            {
                // Update existing plan
                var request = new UpdatePlanRequest
                {
                    Code = Input.Code,
                    Name = Input.Name,
                    Description = Input.Description,
                    MonthlyPrice = monthlyPrice,
                    AnnualPrice = annualPrice,
                    MaxUsers = Input.MaxUsers,
                    MaxWarehouses = Input.MaxWarehouses,
                    MaxProducts = Input.MaxProducts,
                    MaxMonthlyTransactions = Input.MaxMonthlyTransactions,
                    StorageLimitMb = Input.StorageLimitMb,
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    SortOrder = Input.SortOrder,
                    IncludedModules = modulesJson
                };

                var success = await _planService.UpdatePlanAsync(PlanId.Value, request, userId);
                if (!success)
                {
                    return BadRequest(new { error = "Plan not found" });
                }

                _logger.LogInformation("Plan {PlanCode} updated by {UserId}", Input.Code, userId);
            }
            else
            {
                // Create new plan
                var request = new CreatePlanRequest
                {
                    Code = Input.Code,
                    Name = Input.Name,
                    Description = Input.Description,
                    MonthlyPrice = monthlyPrice,
                    AnnualPrice = annualPrice,
                    MaxUsers = Input.MaxUsers,
                    MaxWarehouses = Input.MaxWarehouses,
                    MaxProducts = Input.MaxProducts,
                    MaxMonthlyTransactions = Input.MaxMonthlyTransactions,
                    StorageLimitMb = Input.StorageLimitMb,
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    SortOrder = Input.SortOrder,
                    IncludedModules = modulesJson
                };

                await _planService.CreatePlanAsync(request, userId);
                _logger.LogInformation("Plan {PlanCode} created by {UserId}", Input.Code, userId);
            }

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving plan");
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid planId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _planService.TogglePlanStatusAsync(planId, userId);

            if (success)
            {
                return new JsonResult(new { success = true });
            }

            return BadRequest(new { error = "Plan not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling plan status");
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid planId)
    {
        try
        {
            var success = await _planService.DeletePlanAsync(planId);

            if (success)
            {
                _logger.LogWarning("Plan {PlanId} deleted by {UserId}", planId, GetCurrentUserId());
                return new JsonResult(new { success = true });
            }

            return BadRequest(new { error = "Plan not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting plan");
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnGetCalculatePriceAsync(string moduleIds)
    {
        try
        {
            var ids = string.IsNullOrEmpty(moduleIds)
                ? new List<Guid>()
                : moduleIds.Split(',').Select(Guid.Parse).ToList();

            var monthlyPrice = await _moduleService.CalculatePlanPriceAsync(ids, false);
            var annualPrice = await _moduleService.CalculatePlanPriceAsync(ids, true);

            return new JsonResult(new { monthlyPrice, annualPrice });
        }
        catch
        {
            return new JsonResult(new { monthlyPrice = 0, annualPrice = 0 });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class PlanInput
{
    [Required(ErrorMessage = "Plan code is required")]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(@"^[A-Z0-9_]+$", ErrorMessage = "Code must be uppercase letters, numbers, and underscores only")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Plan name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(0, 1000000)]
    public decimal MonthlyPrice { get; set; }

    [Range(0, 10000000)]
    public decimal AnnualPrice { get; set; }

    public int MaxUsers { get; set; } = 5;
    public int MaxWarehouses { get; set; } = 1;
    public int MaxProducts { get; set; } = 100;
    public int MaxMonthlyTransactions { get; set; } = 100;
    public int StorageLimitMb { get; set; } = 1000;

    public bool IsActive { get; set; } = true;
    public bool IsFeatured { get; set; }
    public int SortOrder { get; set; } = 1;
}
