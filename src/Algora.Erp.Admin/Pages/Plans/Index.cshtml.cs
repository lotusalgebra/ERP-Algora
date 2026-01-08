using Algora.Erp.Admin.Entities;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Algora.Erp.Admin.Pages.Plans;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IPlanService _planService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IPlanService planService, ILogger<IndexModel> logger)
    {
        _planService = planService;
        _logger = logger;
    }

    public List<BillingPlan> Plans { get; set; } = new();
    public Dictionary<Guid, int> PlanSubscriptions { get; set; } = new();
    public decimal TotalMRR { get; set; }
    public int TotalSubscriptions { get; set; }

    [BindProperty]
    public PlanInput Input { get; set; } = new();

    [BindProperty]
    public Guid? PlanId { get; set; }

    public async Task OnGetAsync()
    {
        Plans = await _planService.GetAllPlansAsync(includeInactive: true);

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
            return BadRequest(new { error = "Invalid form data" });
        }

        try
        {
            var userId = GetCurrentUserId();

            if (PlanId.HasValue && PlanId.Value != Guid.Empty)
            {
                // Update existing plan
                var request = new UpdatePlanRequest
                {
                    Code = Input.Code,
                    Name = Input.Name,
                    Description = Input.Description,
                    MonthlyPrice = Input.MonthlyPrice,
                    AnnualPrice = Input.AnnualPrice,
                    MaxUsers = Input.MaxUsers,
                    MaxWarehouses = Input.MaxWarehouses,
                    MaxProducts = Input.MaxProducts,
                    MaxMonthlyTransactions = Input.MaxMonthlyTransactions,
                    StorageLimitMb = Input.StorageLimitMb,
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    SortOrder = Input.SortOrder
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
                    MonthlyPrice = Input.MonthlyPrice,
                    AnnualPrice = Input.AnnualPrice,
                    MaxUsers = Input.MaxUsers,
                    MaxWarehouses = Input.MaxWarehouses,
                    MaxProducts = Input.MaxProducts,
                    MaxMonthlyTransactions = Input.MaxMonthlyTransactions,
                    StorageLimitMb = Input.StorageLimitMb,
                    IsActive = Input.IsActive,
                    IsFeatured = Input.IsFeatured,
                    SortOrder = Input.SortOrder
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

    [Required]
    [Range(0, 10000)]
    public decimal MonthlyPrice { get; set; }

    [Required]
    [Range(0, 100000)]
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
