using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Dynamics365.Auth;
using Algora.Erp.Integrations.Salesforce.Auth;
using Algora.Erp.Integrations.Shopify.Auth;
using Algora.Erp.Integrations.Shopify.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Admin.Integrations;

public class IndexModel : PageModel
{
    private readonly IIntegrationSettingsService _settingsService;
    private readonly IEnumerable<ICrmSyncService> _syncServices;
    private readonly IShopifySyncService? _shopifySyncService;

    public IndexModel(
        IIntegrationSettingsService settingsService,
        IEnumerable<ICrmSyncService> syncServices,
        IShopifySyncService? shopifySyncService = null)
    {
        _settingsService = settingsService;
        _syncServices = syncServices;
        _shopifySyncService = shopifySyncService;
    }

    public bool SalesforceEnabled { get; set; }
    public bool Dynamics365Enabled { get; set; }
    public bool ShopifyEnabled { get; set; }
    public IntegrationStatus SalesforceStatus { get; set; } = new();
    public IntegrationStatus Dynamics365Status { get; set; } = new();
    public IntegrationStatus ShopifyStatus { get; set; } = new();
    public List<CrmSyncLog> RecentSyncLogs { get; set; } = new();

    public async Task OnGetAsync()
    {
        var salesforce = await _settingsService.GetIntegrationAsync(SalesforceAuthHandler.IntegrationType);
        if (salesforce != null)
        {
            SalesforceEnabled = salesforce.IsEnabled;
            SalesforceStatus = new IntegrationStatus
            {
                LastTestedAt = salesforce.LastTestedAt,
                LastTestSuccess = salesforce.LastTestSuccess,
                LastSyncAt = salesforce.LastSyncAt,
                LastSyncSuccess = salesforce.LastSyncSuccess,
                LastSyncRecords = salesforce.LastSyncRecordsProcessed
            };
        }

        var dynamics365 = await _settingsService.GetIntegrationAsync(Dynamics365AuthHandler.IntegrationType);
        if (dynamics365 != null)
        {
            Dynamics365Enabled = dynamics365.IsEnabled;
            Dynamics365Status = new IntegrationStatus
            {
                LastTestedAt = dynamics365.LastTestedAt,
                LastTestSuccess = dynamics365.LastTestSuccess,
                LastSyncAt = dynamics365.LastSyncAt,
                LastSyncSuccess = dynamics365.LastSyncSuccess,
                LastSyncRecords = dynamics365.LastSyncRecordsProcessed
            };
        }

        var shopify = await _settingsService.GetIntegrationAsync(ShopifyAuthHandler.IntegrationType);
        if (shopify != null)
        {
            ShopifyEnabled = shopify.IsEnabled;
            ShopifyStatus = new IntegrationStatus
            {
                LastTestedAt = shopify.LastTestedAt,
                LastTestSuccess = shopify.LastTestSuccess,
                LastSyncAt = shopify.LastSyncAt,
                LastSyncSuccess = shopify.LastSyncSuccess,
                LastSyncRecords = shopify.LastSyncRecordsProcessed
            };
        }

        // TODO: Load recent sync logs from database when CrmSyncLogs repository is implemented
    }

    public async Task<IActionResult> OnPostSyncSalesforceAsync()
    {
        var service = _syncServices.FirstOrDefault(s => s.CrmType == SalesforceAuthHandler.IntegrationType);
        if (service == null)
        {
            TempData["Error"] = "Salesforce integration is not configured";
            return RedirectToPage();
        }

        try
        {
            var result = await service.FullSyncAsync();
            await _settingsService.UpdateSyncResultAsync(
                SalesforceAuthHandler.IntegrationType,
                result.IsSuccess,
                result.RecordsProcessed);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Salesforce sync completed: {result.RecordsProcessed} records processed";
            }
            else
            {
                TempData["Error"] = $"Salesforce sync completed with errors: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Salesforce sync failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSyncDynamics365Async()
    {
        var service = _syncServices.FirstOrDefault(s => s.CrmType == Dynamics365AuthHandler.IntegrationType);
        if (service == null)
        {
            TempData["Error"] = "Dynamics 365 integration is not configured";
            return RedirectToPage();
        }

        try
        {
            var result = await service.FullSyncAsync();
            await _settingsService.UpdateSyncResultAsync(
                Dynamics365AuthHandler.IntegrationType,
                result.IsSuccess,
                result.RecordsProcessed);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Dynamics 365 sync completed: {result.RecordsProcessed} records processed";
            }
            else
            {
                TempData["Error"] = $"Dynamics 365 sync completed with errors: {result.ErrorMessage}";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Dynamics 365 sync failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSyncShopifyAsync()
    {
        if (_shopifySyncService == null)
        {
            TempData["Error"] = "Shopify integration is not configured";
            return RedirectToPage();
        }

        try
        {
            var result = await _shopifySyncService.FullSyncAsync();
            await _settingsService.UpdateSyncResultAsync(
                ShopifyAuthHandler.IntegrationType,
                result.IsSuccess,
                result.TotalRecordsProcessed);

            if (result.IsSuccess)
            {
                TempData["Success"] = $"Shopify sync completed: {result.TotalRecordsProcessed} records processed";
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
                TempData["Error"] = $"Shopify sync completed with errors: {string.Join(", ", errors)}";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Shopify sync failed: {ex.Message}";
        }

        return RedirectToPage();
    }

    public class IntegrationStatus
    {
        public DateTime? LastTestedAt { get; set; }
        public bool? LastTestSuccess { get; set; }
        public DateTime? LastSyncAt { get; set; }
        public bool? LastSyncSuccess { get; set; }
        public int? LastSyncRecords { get; set; }
    }
}
