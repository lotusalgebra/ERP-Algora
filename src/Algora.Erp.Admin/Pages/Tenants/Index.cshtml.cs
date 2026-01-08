using Algora.Erp.Admin.Entities;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Algora.Erp.Admin.Pages.Tenants;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ITenantService _tenantService;
    private readonly IPlanService _planService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ITenantService tenantService, IPlanService planService, ILogger<IndexModel> logger)
    {
        _tenantService = tenantService;
        _planService = planService;
        _logger = logger;
    }

    public List<Tenant> Tenants { get; set; } = new();
    public List<BillingPlan> Plans { get; set; } = new();
    public TenantStats Stats { get; set; } = new();
    public bool ShowDeleted { get; set; }

    [BindProperty]
    public CreateTenantInput CreateInput { get; set; } = new();

    [BindProperty]
    public DeleteTenantInput DeleteInput { get; set; } = new();

    [BindProperty]
    public SuspendTenantInput SuspendInput { get; set; } = new();

    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }

    public async Task OnGetAsync(string? search = null, string? status = null, bool showDeleted = false)
    {
        SearchTerm = search;
        StatusFilter = status;
        ShowDeleted = showDeleted;

        TenantStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TenantStatus>(status, out var parsed))
        {
            statusEnum = parsed;
        }

        Tenants = await _tenantService.GetTenantsAsync(search, statusEnum, showDeleted);
        Stats = await _tenantService.GetTenantStatsAsync();
        Plans = await _planService.GetAllPlansAsync();
    }

    public async Task<IActionResult> OnGetTableRowsAsync(string? search = null, string? status = null, bool showDeleted = false)
    {
        TenantStatus? statusEnum = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<TenantStatus>(status, out var parsed))
        {
            statusEnum = parsed;
        }

        Tenants = await _tenantService.GetTenantsAsync(search, statusEnum, showDeleted);
        ShowDeleted = showDeleted;
        return Partial("_TenantTableRows", this);
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GetCurrentUserId();
            var request = new CreateTenantRequest
            {
                Name = CreateInput.Name,
                Subdomain = CreateInput.Subdomain,
                ContactEmail = CreateInput.ContactEmail,
                ContactPhone = CreateInput.ContactPhone,
                Country = CreateInput.Country,
                ContactPerson = $"{CreateInput.AdminFirstName} {CreateInput.AdminLastName}".Trim()
            };

            var tenant = await _tenantService.CreateTenantAsync(request, userId);
            _logger.LogInformation("Tenant {TenantName} created by {UserId}", tenant.Name, userId);

            return new JsonResult(new { success = true, message = "Tenant created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant");
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _tenantService.SoftDeleteTenantAsync(DeleteInput.TenantId, DeleteInput.Reason ?? "No reason provided", userId);

            if (success)
            {
                _logger.LogInformation("Tenant {TenantId} soft deleted by {UserId}", DeleteInput.TenantId, userId);
                return new JsonResult(new { success = true, message = "Tenant deleted successfully" });
            }

            return BadRequest(new { error = "Failed to delete tenant" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting tenant {TenantId}", DeleteInput.TenantId);
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostSuspendAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _tenantService.SuspendTenantAsync(SuspendInput.TenantId, SuspendInput.Reason ?? "No reason provided", userId);

            if (success)
            {
                _logger.LogInformation("Tenant {TenantId} suspended by {UserId}", SuspendInput.TenantId, userId);
                return new JsonResult(new { success = true, message = "Tenant suspended successfully" });
            }

            return BadRequest(new { error = "Failed to suspend tenant" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending tenant {TenantId}", SuspendInput.TenantId);
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostUnsuspendAsync(Guid tenantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _tenantService.UnsuspendTenantAsync(tenantId, userId);

            if (success)
            {
                _logger.LogInformation("Tenant {TenantId} unsuspended by {UserId}", tenantId, userId);
                return new JsonResult(new { success = true, message = "Tenant unsuspended successfully" });
            }

            return BadRequest(new { error = "Failed to unsuspend tenant" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsuspending tenant {TenantId}", tenantId);
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostRestoreAsync(Guid tenantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _tenantService.RestoreTenantAsync(tenantId, userId);

            if (success)
            {
                _logger.LogInformation("Tenant {TenantId} restored by {UserId}", tenantId, userId);
                return new JsonResult(new { success = true, message = "Tenant restored successfully" });
            }

            return BadRequest(new { error = "Failed to restore tenant" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring tenant {TenantId}", tenantId);
            return BadRequest(new { error = ex.Message });
        }
    }

    public async Task<IActionResult> OnPostPermanentDeleteAsync(Guid tenantId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var success = await _tenantService.PermanentDeleteTenantAsync(tenantId);

            if (success)
            {
                _logger.LogWarning("Tenant {TenantId} permanently deleted by {UserId}", tenantId, userId);
                return new JsonResult(new { success = true, message = "Tenant permanently deleted" });
            }

            return BadRequest(new { error = "Failed to permanently delete tenant" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error permanently deleting tenant {TenantId}", tenantId);
            return BadRequest(new { error = ex.Message });
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class CreateTenantInput
{
    [Required(ErrorMessage = "Company name is required")]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Subdomain is required")]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Subdomain can only contain lowercase letters, numbers, and hyphens")]
    public string Subdomain { get; set; } = string.Empty;

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string ContactEmail { get; set; } = string.Empty;

    public string? ContactPhone { get; set; }
    public string? Country { get; set; }

    [Required(ErrorMessage = "Admin email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string AdminEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin first name is required")]
    public string AdminFirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Admin last name is required")]
    public string AdminLastName { get; set; } = string.Empty;
}

public class DeleteTenantInput
{
    public Guid TenantId { get; set; }
    public string? Reason { get; set; }
}

public class SuspendTenantInput
{
    public Guid TenantId { get; set; }
    public string? Reason { get; set; }
}
