using Algora.Erp.Admin.Entities;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Algora.Erp.Admin.Pages.Tenants;

[Authorize]
public class EditModel : PageModel
{
    private readonly ITenantService _tenantService;
    private readonly IPlanService _planService;
    private readonly ILogger<EditModel> _logger;

    public EditModel(
        ITenantService tenantService,
        IPlanService planService,
        ILogger<EditModel> logger)
    {
        _tenantService = tenantService;
        _planService = planService;
        _logger = logger;
    }

    public Tenant Tenant { get; set; } = null!;
    public List<BillingPlan> Plans { get; set; } = new();

    [BindProperty]
    public EditTenantInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        Tenant = tenant;
        Plans = await _planService.GetAllPlansAsync();

        // Populate input from tenant
        Input = new EditTenantInput
        {
            Id = tenant.Id,
            Name = tenant.Name,
            Subdomain = tenant.Subdomain,
            DatabaseName = tenant.DatabaseName,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            ContactPerson = tenant.ContactPerson,
            CompanyName = tenant.CompanyName,
            TaxId = tenant.TaxId,
            Address = tenant.Address,
            City = tenant.City,
            State = tenant.State,
            Country = tenant.Country,
            PostalCode = tenant.PostalCode,
            CurrencyCode = tenant.CurrencyCode ?? "INR",
            TimeZone = tenant.TimeZone ?? "Asia/Kolkata",
            MaxUsers = tenant.MaxUsers
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var tenant = await _tenantService.GetTenantByIdAsync(Input.Id);
            if (tenant == null) return NotFound();

            Tenant = tenant;
            Plans = await _planService.GetAllPlansAsync();
            return Page();
        }

        try
        {
            var userId = GetCurrentUserId();

            var request = new UpdateTenantRequest
            {
                Name = Input.Name,
                ContactEmail = Input.ContactEmail,
                ContactPhone = Input.ContactPhone,
                ContactPerson = Input.ContactPerson,
                CompanyName = Input.CompanyName,
                TaxId = Input.TaxId,
                Address = Input.Address,
                City = Input.City,
                State = Input.State,
                Country = Input.Country,
                PostalCode = Input.PostalCode,
                CurrencyCode = Input.CurrencyCode,
                TimeZone = Input.TimeZone,
                MaxUsers = Input.MaxUsers
            };

            await _tenantService.UpdateTenantAsync(Input.Id, request, userId);

            _logger.LogInformation("Tenant {TenantId} updated by {UserId}", Input.Id, userId);

            TempData["SuccessMessage"] = "Tenant updated successfully.";
            return RedirectToPage("Details", new { id = Input.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tenant {TenantId}", Input.Id);
            ModelState.AddModelError(string.Empty, ex.Message);

            var tenant = await _tenantService.GetTenantByIdAsync(Input.Id);
            if (tenant == null) return NotFound();

            Tenant = tenant;
            Plans = await _planService.GetAllPlansAsync();
            return Page();
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}

public class EditTenantInput
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Organization name is required")]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public string Subdomain { get; set; } = string.Empty;

    [StringLength(100)]
    public string? DatabaseName { get; set; }

    [Required(ErrorMessage = "Contact email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string ContactEmail { get; set; } = string.Empty;

    [Phone]
    public string? ContactPhone { get; set; }

    [StringLength(100)]
    public string? ContactPerson { get; set; }

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(50)]
    public string? TaxId { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(100)]
    public string? State { get; set; }

    [StringLength(100)]
    public string? Country { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(10)]
    public string CurrencyCode { get; set; } = "INR";

    [StringLength(100)]
    public string TimeZone { get; set; } = "Asia/Kolkata";

    [Range(1, 10000)]
    public int MaxUsers { get; set; }
}
