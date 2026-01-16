using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Shopify.Auth;
using Algora.Erp.Integrations.Shopify.Client;
using Algora.Erp.Integrations.Shopify.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Admin.Integrations;

public class ShopifyModel : PageModel
{
    private readonly IIntegrationSettingsService _settingsService;
    private readonly IShopifySyncService? _syncService;
    private readonly IServiceProvider _serviceProvider;

    private const string IntegrationType = ShopifyAuthHandler.IntegrationType;

    public ShopifyModel(
        IIntegrationSettingsService settingsService,
        IShopifySyncService? syncService,
        IServiceProvider serviceProvider)
    {
        _settingsService = settingsService;
        _syncService = syncService;
        _serviceProvider = serviceProvider;
    }

    public bool IsConnected { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public bool? LastTestSuccess { get; set; }
    public string? LastTestError { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool? LastSyncSuccess { get; set; }

    [BindProperty]
    public string ShopDomain { get; set; } = string.Empty;

    [BindProperty]
    public string AccessToken { get; set; } = string.Empty;

    [BindProperty]
    public string ApiVersion { get; set; } = "2024-01";

    [BindProperty]
    public int SyncIntervalMinutes { get; set; } = 30;

    [BindProperty]
    public bool SyncCustomers { get; set; } = true;

    [BindProperty]
    public bool SyncOrders { get; set; } = true;

    [BindProperty]
    public bool SyncProducts { get; set; } = true;

    [BindProperty]
    public bool SyncInventory { get; set; } = true;

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

        var settings = await _settingsService.GetSettingsAsync<ShopifySettingsData>(IntegrationType);
        if (settings != null)
        {
            ShopDomain = settings.ShopDomain;
            ApiVersion = settings.ApiVersion;
            SyncCustomers = settings.SyncCustomers;
            SyncOrders = settings.SyncOrders;
            SyncProducts = settings.SyncProducts;
            SyncInventory = settings.SyncInventory;
        }

        var credentials = await _settingsService.GetCredentialsAsync<ShopifyCredentials>(IntegrationType);
        if (credentials != null)
        {
            AccessToken = string.IsNullOrEmpty(credentials.AccessToken) ? "" : "••••••••";
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Get existing credentials to preserve masked values
            var existingCredentials = await _settingsService.GetCredentialsAsync<ShopifyCredentials>(IntegrationType);

            var settings = new ShopifySettingsData
            {
                ShopDomain = ShopDomain,
                ApiVersion = ApiVersion,
                SyncCustomers = SyncCustomers,
                SyncOrders = SyncOrders,
                SyncProducts = SyncProducts,
                SyncInventory = SyncInventory
            };

            var credentials = new ShopifyCredentials
            {
                AccessToken = AccessToken == "••••••••" ? existingCredentials?.AccessToken ?? "" : AccessToken
            };

            await _settingsService.SaveSettingsAsync(
                IntegrationType,
                settings,
                credentials,
                enabled: true,
                syncIntervalMinutes: SyncIntervalMinutes);

            TempData["Success"] = "Shopify settings saved successfully";
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

            var client = _serviceProvider.GetService<IShopifyClient>();
            if (client == null)
            {
                TempData["Error"] = "Shopify client is not configured";
                return RedirectToPage();
            }

            var isConnected = await client.TestConnectionAsync();
            await _settingsService.UpdateTestResultAsync(
                IntegrationType,
                isConnected,
                isConnected ? null : "Connection test returned false");

            if (isConnected)
            {
                TempData["Success"] = "Connection successful! Shopify is ready to sync.";
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
        if (_syncService == null)
        {
            TempData["Error"] = "Shopify sync service is not available";
            return RedirectToPage();
        }

        try
        {
            var result = await _syncService.FullSyncAsync();
            await _settingsService.UpdateSyncResultAsync(
                IntegrationType,
                result.IsSuccess,
                result.TotalRecordsProcessed);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Sync completed: {result.TotalRecordsProcessed} records processed";
            }
            else
            {
                var errors = new List<string>();
                if (result.CustomerSync != null && !result.CustomerSync.IsSuccess)
                    errors.Add($"Customers: {result.CustomerSync.ErrorMessage}");
                if (result.OrderSync != null && !result.OrderSync.IsSuccess)
                    errors.Add($"Orders: {result.OrderSync.ErrorMessage}");
                if (result.ProductSync != null && !result.ProductSync.IsSuccess)
                    errors.Add($"Products: {result.ProductSync.ErrorMessage}");
                if (result.InventorySync != null && !result.InventorySync.IsSuccess)
                    errors.Add($"Inventory: {result.InventorySync.ErrorMessage}");
                TempData["Error"] = $"Sync completed with errors: {string.Join(", ", errors)}";
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
            TempData["Success"] = "Shopify disconnected successfully";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to disconnect: {ex.Message}";
        }

        return RedirectToPage();
    }
}
