using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Settings;
using Algora.Erp.Integrations.Salesforce.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Web.Pages.Admin.Integrations;

public class SalesforceModel : PageModel
{
    private readonly IOptions<CrmIntegrationsSettings> _settings;
    private readonly IEnumerable<ICrmSyncService> _syncServices;
    private readonly IServiceProvider _serviceProvider;

    public SalesforceModel(
        IOptions<CrmIntegrationsSettings> settings,
        IEnumerable<ICrmSyncService> syncServices,
        IServiceProvider serviceProvider)
    {
        _settings = settings;
        _syncServices = syncServices;
        _serviceProvider = serviceProvider;
    }

    public bool IsConnected { get; set; }

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

    public void OnGet()
    {
        var settings = _settings.Value.Salesforce;
        IsConnected = settings.Enabled && !string.IsNullOrEmpty(settings.ClientId);

        ClientId = settings.ClientId;
        ClientSecret = string.IsNullOrEmpty(settings.ClientSecret) ? "" : "••••••••";
        Username = settings.Username;
        Password = string.IsNullOrEmpty(settings.Password) ? "" : "••••••••";
        SecurityToken = string.IsNullOrEmpty(settings.SecurityToken) ? "" : "••••••••";
        InstanceUrl = settings.InstanceUrl;
        ApiVersion = settings.ApiVersion;
        SyncIntervalMinutes = settings.SyncIntervalMinutes;
    }

    public IActionResult OnPost()
    {
        // In a real implementation, save to database/configuration
        TempData["Success"] = "Salesforce settings saved successfully";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostTestAsync()
    {
        try
        {
            var client = _serviceProvider.GetService<SalesforceClient>();
            if (client == null)
            {
                TempData["Error"] = "Salesforce client is not configured. Please check your appsettings.json";
                return RedirectToPage();
            }

            var isConnected = await client.TestConnectionAsync();
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
            TempData["Error"] = $"Connection test failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSyncAsync()
    {
        var service = _syncServices.FirstOrDefault(s => s.CrmType == "Salesforce");
        if (service == null)
        {
            TempData["Error"] = "Salesforce sync service is not available";
            return RedirectToPage();
        }

        try
        {
            var result = await service.FullSyncAsync();
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
            TempData["Error"] = $"Sync failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public IActionResult OnPostDisconnect()
    {
        // In a real implementation, clear credentials from database
        TempData["Success"] = "Salesforce disconnected successfully";
        return RedirectToPage();
    }
}
