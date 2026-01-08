using Algora.Erp.Admin.Entities;
using Algora.Erp.Admin.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace Algora.Erp.Admin.Pages.Tenants;

[Authorize]
public class DetailsModel : PageModel
{
    private readonly ITenantService _tenantService;
    private readonly IBackupService _backupService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(
        ITenantService tenantService,
        IBackupService backupService,
        IConfiguration configuration,
        ILogger<DetailsModel> logger)
    {
        _tenantService = tenantService;
        _backupService = backupService;
        _configuration = configuration;
        _logger = logger;
    }

    public Tenant Tenant { get; set; } = null!;
    public List<DatabaseBackup> Backups { get; set; } = new();
    public BackupStats BackupStats { get; set; } = new();
    public string BackupDirectory { get; set; } = string.Empty;
    public int RetentionDays { get; set; }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var tenant = await _tenantService.GetTenantByIdAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        Tenant = tenant;
        Backups = await _backupService.GetBackupsAsync(id);
        BackupStats = await _backupService.GetBackupStatsAsync(id);
        BackupDirectory = _backupService.GetBackupDirectory();
        RetentionDays = _configuration.GetValue<int>("Backup:RetentionDays", 30);

        return Page();
    }

    public async Task<IActionResult> OnPostCreateBackupAsync(Guid id, BackupType type, string? notes)
    {
        try
        {
            var userId = GetCurrentUserId();
            var backup = await _backupService.CreateBackupAsync(id, type, userId, notes);

            _logger.LogInformation(
                "Backup created for tenant {TenantId} by user {UserId}",
                id, userId);

            TempData["SuccessMessage"] = $"Backup '{backup.FileName}' created successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup for tenant {TenantId}", id);
            TempData["ErrorMessage"] = $"Failed to create backup: {ex.Message}";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeleteBackupAsync(Guid id, Guid backupId)
    {
        try
        {
            await _backupService.DeleteBackupAsync(backupId);
            TempData["SuccessMessage"] = "Backup deleted successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting backup {BackupId}", backupId);
            TempData["ErrorMessage"] = $"Failed to delete backup: {ex.Message}";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRestoreBackupAsync(Guid id, Guid backupId)
    {
        try
        {
            await _backupService.RestoreBackupAsync(backupId);
            TempData["SuccessMessage"] = "Database restored successfully.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring backup {BackupId}", backupId);
            TempData["ErrorMessage"] = $"Failed to restore database: {ex.Message}";
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnGetDownloadBackupAsync(Guid id, Guid backupId)
    {
        var backup = await _backupService.GetBackupByIdAsync(backupId);
        if (backup == null || backup.TenantId != id)
        {
            return NotFound();
        }

        if (!System.IO.File.Exists(backup.FilePath))
        {
            TempData["ErrorMessage"] = "Backup file not found on disk.";
            return RedirectToPage(new { id });
        }

        var fileStream = new FileStream(backup.FilePath, FileMode.Open, FileAccess.Read);
        return File(fileStream, "application/octet-stream", backup.FileName);
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}
