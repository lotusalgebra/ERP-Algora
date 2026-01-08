using Algora.Erp.Admin.Entities;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Algora.Erp.Admin.Pages.Modules;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IModuleService _moduleService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IModuleService moduleService, ILogger<IndexModel> logger)
    {
        _moduleService = moduleService;
        _logger = logger;
    }

    public List<PlanModule> Modules { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public decimal TotalMonthlyRevenue { get; set; }
    public int CoreModulesCount { get; set; }

    [BindProperty]
    public ModuleInput Input { get; set; } = new();

    [BindProperty]
    public Guid? ModuleId { get; set; }

    public async Task OnGetAsync()
    {
        Modules = await _moduleService.GetAllModulesAsync(includeInactive: true);
        Categories = await _moduleService.GetCategoriesAsync();
        CoreModulesCount = Modules.Count(m => m.IsCore);
        TotalMonthlyRevenue = Modules.Where(m => m.IsActive).Sum(m => m.MonthlyPrice);
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

            if (ModuleId.HasValue && ModuleId.Value != Guid.Empty)
            {
                // Update existing module
                var request = new UpdateModuleRequest
                {
                    Code = Input.Code,
                    Name = Input.Name,
                    Description = Input.Description,
                    Icon = Input.Icon,
                    MonthlyPrice = Input.MonthlyPrice,
                    AnnualPrice = Input.AnnualPrice,
                    Currency = "INR",
                    IsCore = Input.IsCore,
                    RequiredModules = Input.RequiredModules,
                    DisplayOrder = Input.DisplayOrder,
                    IsActive = Input.IsActive,
                    Category = Input.Category
                };

                var success = await _moduleService.UpdateModuleAsync(ModuleId.Value, request, userId);
                if (!success)
                {
                    return BadRequest(new { error = "Module not found" });
                }

                _logger.LogInformation("Module {ModuleCode} updated by {UserId}", Input.Code, userId);
            }
            else
            {
                // Create new module
                var request = new CreateModuleRequest
                {
                    Code = Input.Code,
                    Name = Input.Name,
                    Description = Input.Description,
                    Icon = Input.Icon,
                    MonthlyPrice = Input.MonthlyPrice,
                    AnnualPrice = Input.AnnualPrice,
                    Currency = "INR",
                    IsCore = Input.IsCore,
                    RequiredModules = Input.RequiredModules,
                    DisplayOrder = Input.DisplayOrder,
                    IsActive = Input.IsActive,
                    Category = Input.Category
                };

                await _moduleService.CreateModuleAsync(request, userId);
                _logger.LogInformation("Module {ModuleCode} created by {UserId}", Input.Code, userId);
            }

            return new JsonResult(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving module");
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostToggleAsync(Guid moduleId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _moduleService.ToggleModuleStatusAsync(moduleId, userId);

            if (success)
            {
                return new JsonResult(new { success = true });
            }

            return BadRequest(new { error = "Module not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling module status");
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid moduleId)
    {
        try
        {
            var success = await _moduleService.DeleteModuleAsync(moduleId);

            if (success)
            {
                _logger.LogWarning("Module {ModuleId} deleted by {UserId}", moduleId, GetCurrentUserId());
                return new JsonResult(new { success = true });
            }

            return BadRequest(new { error = "Module not found" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting module");
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class ModuleInput
{
    [Required(ErrorMessage = "Module code is required")]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(@"^[a-z0-9_]+$", ErrorMessage = "Code must be lowercase letters, numbers, and underscores only")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Module name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string Icon { get; set; } = "bi-box";

    [Required]
    [Range(0, 100000)]
    public decimal MonthlyPrice { get; set; }

    [Required]
    [Range(0, 1000000)]
    public decimal AnnualPrice { get; set; }

    public bool IsCore { get; set; }

    public string? RequiredModules { get; set; }

    public int DisplayOrder { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    [Required]
    public string Category { get; set; } = "Core";
}
