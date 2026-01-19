using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Dynamics365.Auth;
using Algora.Erp.Integrations.Dynamics365.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Admin.Integrations;

public class Dynamics365Model : PageModel
{
    private readonly IIntegrationSettingsService _settingsService;
    private readonly IEnumerable<ICrmSyncService> _syncServices;
    private readonly IServiceProvider _serviceProvider;

    private const string IntegrationType = Dynamics365AuthHandler.IntegrationType;

    public Dynamics365Model(
        IIntegrationSettingsService settingsService,
        IEnumerable<ICrmSyncService> syncServices,
        IServiceProvider serviceProvider)
    {
        _settingsService = settingsService;
        _syncServices = syncServices;
        _serviceProvider = serviceProvider;
    }

    public bool IsConnected { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public bool? LastTestSuccess { get; set; }
    public string? LastTestError { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool? LastSyncSuccess { get; set; }

    [BindProperty]
    public string TenantId { get; set; } = string.Empty;

    [BindProperty]
    public string ClientId { get; set; } = string.Empty;

    [BindProperty]
    public string ClientSecret { get; set; } = string.Empty;

    [BindProperty]
    public string InstanceUrl { get; set; } = string.Empty;

    [BindProperty]
    public string ApiVersion { get; set; } = "v9.2";

    [BindProperty]
    public int SyncIntervalMinutes { get; set; } = 30;

    [BindProperty]
    public bool SyncContacts { get; set; } = true;

    [BindProperty]
    public bool SyncLeads { get; set; } = true;

    [BindProperty]
    public bool SyncOpportunities { get; set; } = true;

    [BindProperty]
    public bool SyncAccounts { get; set; } = true;

    public async Task OnGetAsync()
    {
        var integration = await _settingsService.GetIntegrationAsync(IntegrationType);
        if (integration != null)
        {
            IsConnected = integration.IsEnabled;
            LastTestedAt = integration.LastTestedAt;
            LastTestSuccess = integration.LastTestSuccess;
            LastTestError = integration.LastTestError;
            LastSyncAt = integration.LastSyncAt;
            LastSyncSuccess = integration.LastSyncSuccess;
            SyncIntervalMinutes = integration.SyncIntervalMinutes;
        }

        var settings = await _settingsService.GetSettingsAsync<Dynamics365SettingsData>(IntegrationType);
        if (settings != null)
        {
            TenantId = settings.TenantId;
            InstanceUrl = settings.InstanceUrl;
            ApiVersion = settings.ApiVersion;
        }

        var credentials = await _settingsService.GetCredentialsAsync<Dynamics365Credentials>(IntegrationType);
        if (credentials != null)
        {
            ClientId = credentials.ClientId;
            ClientSecret = string.IsNullOrEmpty(credentials.ClientSecret) ? "" : "••••••••";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Get existing credentials to preserve masked values
            var existingCredentials = await _settingsService.GetCredentialsAsync<Dynamics365Credentials>(IntegrationType);

            var settings = new Dynamics365SettingsData
            {
                TenantId = TenantId,
                InstanceUrl = InstanceUrl,
                ApiVersion = ApiVersion
            };

            var credentials = new Dynamics365Credentials
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret == "••••••••" ? existingCredentials?.ClientSecret ?? "" : ClientSecret
            };

            await _settingsService.SaveSettingsAsync(
                IntegrationType,
                settings,
                credentials,
                enabled: true,
                syncIntervalMinutes: SyncIntervalMinutes);

            TempData["Success"] = "Dynamics 365 settings saved successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to save settings: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestAsync()
    {
        try
        {
            // First save the settings if they've changed
            await OnPostAsync();

            var client = _serviceProvider.GetService<Dynamics365Client>();
            if (client == null)
            {
                TempData["Error"] = "Dynamics 365 client is not configured";
                return RedirectToPage();
            }

            var isConnected = await client.TestConnectionAsync();
            await _settingsService.UpdateTestResultAsync(
                IntegrationType,
                isConnected,
                isConnected ? null : "Connection test returned false");

            if (isConnected)
            {
                TempData["Success"] = "Connection successful! Dynamics 365 is ready to sync.";
            }
            else
            {
                TempData["Error"] = "Connection failed. Please check your credentials.";
            }
        }
        catch (Exception ex)
        {
            await _settingsService.UpdateTestResultAsync(IntegrationType, false, ex.Message);
            TempData["Error"] = $"Connection test failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSyncAsync()
    {
        var service = _syncServices.FirstOrDefault(s => s.CrmType == IntegrationType);
        if (service == null)
        {
            TempData["Error"] = "Dynamics 365 sync service is not available";
            return RedirectToPage();
        }

        try
        {
            var result = await service.FullSyncAsync();
            await _settingsService.UpdateSyncResultAsync(
                IntegrationType,
                result.IsSuccess,
                result.RecordsProcessed);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Sync completed: {result.RecordsProcessed} records processed, {result.RecordsCreated} created, {result.RecordsUpdated} updated";
            }
            else
            {
                TempData["Error"] = $"Sync completed with errors: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            await _settingsService.UpdateSyncResultAsync(IntegrationType, false, 0);
            TempData["Error"] = $"Sync failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDisconnectAsync()
    {
        try
        {
            await _settingsService.DeleteAsync(IntegrationType);
            TempData["Success"] = "Dynamics 365 disconnected successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to disconnect: {ex.Message}";
        }

        return RedirectToPage();
    }
}
