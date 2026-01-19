using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Salesforce.Auth;
using Algora.Erp.Integrations.Salesforce.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Admin.Integrations;

public class SalesforceModel : PageModel
{
    private readonly IIntegrationSettingsService _settingsService;
    private readonly IEnumerable<ICrmSyncService> _syncServices;
    private readonly IServiceProvider _serviceProvider;

    private const string IntegrationType = SalesforceAuthHandler.IntegrationType;

    public SalesforceModel(
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
    public string ClientId { get; set; } = string.Empty;

    [BindProperty]
    public string ClientSecret { get; set; } = string.Empty;

    [BindProperty]
    public string Username { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public string SecurityToken { get; set; } = string.Empty;

    [BindProperty]
    public string InstanceUrl { get; set; } = "https://login.salesforce.com";

    [BindProperty]
    public string ApiVersion { get; set; } = "v58.0";

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

        var settings = await _settingsService.GetSettingsAsync<SalesforceSettingsData>(IntegrationType);
        if (settings != null)
        {
            InstanceUrl = settings.InstanceUrl;
            ApiVersion = settings.ApiVersion;
            Username = settings.Username;
        }

        var credentials = await _settingsService.GetCredentialsAsync<SalesforceCredentials>(IntegrationType);
        if (credentials != null)
        {
            ClientId = credentials.ClientId;
            ClientSecret = string.IsNullOrEmpty(credentials.ClientSecret) ? "" : "••••••••";
            Password = string.IsNullOrEmpty(credentials.Password) ? "" : "••••••••";
            SecurityToken = string.IsNullOrEmpty(credentials.SecurityToken) ? "" : "••••••••";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Get existing credentials to preserve masked values
            var existingCredentials = await _settingsService.GetCredentialsAsync<SalesforceCredentials>(IntegrationType);

            var settings = new SalesforceSettingsData
            {
                InstanceUrl = InstanceUrl,
                ApiVersion = ApiVersion,
                Username = Username
            };

            var credentials = new SalesforceCredentials
            {
                ClientId = ClientId,
                ClientSecret = ClientSecret == "••••••••" ? existingCredentials?.ClientSecret ?? "" : ClientSecret,
                Password = Password == "••••••••" ? existingCredentials?.Password ?? "" : Password,
                SecurityToken = SecurityToken == "••••••••" ? existingCredentials?.SecurityToken ?? "" : SecurityToken
            };

            await _settingsService.SaveSettingsAsync(
                IntegrationType,
                settings,
                credentials,
                enabled: true,
                syncIntervalMinutes: SyncIntervalMinutes);

            TempData["Success"] = "Salesforce settings saved successfully";
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

            var client = _serviceProvider.GetService<SalesforceClient>();
            if (client == null)
            {
                TempData["Error"] = "Salesforce client is not configured";
                return RedirectToPage();
            }

            var isConnected = await client.TestConnectionAsync();
            await _settingsService.UpdateTestResultAsync(
                IntegrationType,
                isConnected,
                isConnected ? null : "Connection test returned false");

            if (isConnected)
            {
                TempData["Success"] = "Connection successful! Salesforce is ready to sync.";
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
            TempData["Error"] = "Salesforce sync service is not available";
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
            TempData["Success"] = "Salesforce disconnected successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to disconnect: {ex.Message}";
        }

        return RedirectToPage();
    }
}
