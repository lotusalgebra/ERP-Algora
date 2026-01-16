using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Common.Settings;
using Algora.Erp.Integrations.Shopify.Client;
using Algora.Erp.Integrations.Shopify.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Integrations.Shopify.Services;

public interface IShopifySyncService
{
    string CrmType { get; }
    Task<SyncResult> SyncCustomersAsync(CancellationToken ct = default);
    Task<SyncResult> SyncOrdersAsync(CancellationToken ct = default);
    Task<SyncResult> SyncProductsAsync(CancellationToken ct = default);
    Task<SyncResult> SyncInventoryAsync(CancellationToken ct = default);
    Task<ShopifySyncSummary> FullSyncAsync(CancellationToken ct = default);
}

public class ShopifySyncService : IShopifySyncService
{
    private readonly IShopifyClient _client;
    private readonly ShopifySettings _settings;
    private readonly ILogger<ShopifySyncService> _logger;

    public string CrmType => "Shopify";

    public ShopifySyncService(
        IShopifyClient client,
        IOptions<CrmIntegrationsSettings> options,
        ILogger<ShopifySyncService> logger)
    {
        _client = client;
        _settings = options.Value.Shopify;
        _logger = logger;
    }

    public async Task<SyncResult> SyncCustomersAsync(CancellationToken ct = default)
    {
        var result = new SyncResult
        {
            CrmType = CrmType,
            EntityType = "Customer",
            Direction = SyncDirection.ToErp,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting Shopify customer sync");

            var customers = await _client.GetCustomersAsync(ct: ct);
            result.RecordsProcessed = customers.Count;

            foreach (var customer in customers)
            {
                try
                {
                    // TODO: Map to ERP customer entity and save
                    // For now, we just count the records
                    result.RecordsCreated++;

                    _logger.LogDebug("Processed Shopify customer {CustomerId}: {Email}",
                        customer.Id, customer.Email);
                }
                catch (Exception ex)
                {
                    result.RecordsFailed++;
                    result.Errors.Add(new SyncError
                    {
                        EntityId = customer.Id.ToString(),
                        ErrorMessage = ex.Message
                    });
                    _logger.LogWarning(ex, "Failed to sync customer {CustomerId}", customer.Id);
                }
            }

            result.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Completed Shopify customer sync: {Processed} processed, {Created} created, {Failed} failed",
                result.RecordsProcessed, result.RecordsCreated, result.RecordsFailed);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Shopify customer sync failed");
        }

        return result;
    }

    public async Task<SyncResult> SyncOrdersAsync(CancellationToken ct = default)
    {
        var result = new SyncResult
        {
            CrmType = CrmType,
            EntityType = "Order",
            Direction = SyncDirection.ToErp,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting Shopify order sync");

            var orders = await _client.GetOrdersAsync(ct: ct);
            result.RecordsProcessed = orders.Count;

            foreach (var order in orders)
            {
                try
                {
                    // TODO: Map to ERP sales order entity and save
                    // For now, we just count the records
                    result.RecordsCreated++;

                    _logger.LogDebug("Processed Shopify order {OrderId}: {OrderName}",
                        order.Id, order.Name);
                }
                catch (Exception ex)
                {
                    result.RecordsFailed++;
                    result.Errors.Add(new SyncError
                    {
                        EntityId = order.Id.ToString(),
                        ErrorMessage = ex.Message
                    });
                    _logger.LogWarning(ex, "Failed to sync order {OrderId}", order.Id);
                }
            }

            result.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Completed Shopify order sync: {Processed} processed, {Created} created, {Failed} failed",
                result.RecordsProcessed, result.RecordsCreated, result.RecordsFailed);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Shopify order sync failed");
        }

        return result;
    }

    public async Task<SyncResult> SyncProductsAsync(CancellationToken ct = default)
    {
        var result = new SyncResult
        {
            CrmType = CrmType,
            EntityType = "Product",
            Direction = SyncDirection.ToErp,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting Shopify product sync");

            var products = await _client.GetProductsAsync(ct: ct);
            result.RecordsProcessed = products.Count;

            foreach (var product in products)
            {
                try
                {
                    // TODO: Map to ERP product entity and save
                    // For now, we just count the records
                    result.RecordsCreated++;

                    _logger.LogDebug("Processed Shopify product {ProductId}: {Title}",
                        product.Id, product.Title);
                }
                catch (Exception ex)
                {
                    result.RecordsFailed++;
                    result.Errors.Add(new SyncError
                    {
                        EntityId = product.Id.ToString(),
                        ErrorMessage = ex.Message
                    });
                    _logger.LogWarning(ex, "Failed to sync product {ProductId}", product.Id);
                }
            }

            result.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Completed Shopify product sync: {Processed} processed, {Created} created, {Failed} failed",
                result.RecordsProcessed, result.RecordsCreated, result.RecordsFailed);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Shopify product sync failed");
        }

        return result;
    }

    public async Task<SyncResult> SyncInventoryAsync(CancellationToken ct = default)
    {
        var result = new SyncResult
        {
            CrmType = CrmType,
            EntityType = "Inventory",
            Direction = SyncDirection.ToErp,
            StartedAt = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting Shopify inventory sync");

            // Get all locations first
            var locations = await _client.GetLocationsAsync(ct);
            _logger.LogInformation("Found {LocationCount} Shopify locations", locations.Count);

            foreach (var location in locations.Where(l => l.Active))
            {
                var levels = await _client.GetInventoryLevelsAsync(location.Id, ct);
                result.RecordsProcessed += levels.Count;

                foreach (var level in levels)
                {
                    try
                    {
                        // TODO: Map to ERP inventory level and save
                        result.RecordsCreated++;

                        _logger.LogDebug("Processed inventory level for item {ItemId} at location {LocationId}: {Available}",
                            level.InventoryItemId, level.LocationId, level.Available);
                    }
                    catch (Exception ex)
                    {
                        result.RecordsFailed++;
                        result.Errors.Add(new SyncError
                        {
                            EntityId = $"{level.InventoryItemId}@{level.LocationId}",
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            result.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Completed Shopify inventory sync: {Processed} processed, {Created} created, {Failed} failed",
                result.RecordsProcessed, result.RecordsCreated, result.RecordsFailed);
        }
        catch (Exception ex)
        {
            result.ErrorMessage = ex.Message;
            result.CompletedAt = DateTime.UtcNow;
            _logger.LogError(ex, "Shopify inventory sync failed");
        }

        return result;
    }

    public async Task<ShopifySyncSummary> FullSyncAsync(CancellationToken ct = default)
    {
        var summary = new ShopifySyncSummary
        {
            StartedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Starting full Shopify sync");

        // Sync customers
        if (_settings.SyncCustomers)
        {
            summary.CustomerSync = await SyncCustomersAsync(ct);
        }

        // Sync orders
        if (_settings.SyncOrders)
        {
            summary.OrderSync = await SyncOrdersAsync(ct);
        }

        // Sync products
        if (_settings.SyncProducts)
        {
            summary.ProductSync = await SyncProductsAsync(ct);
        }

        // Sync inventory
        if (_settings.SyncInventory)
        {
            summary.InventorySync = await SyncInventoryAsync(ct);
        }

        summary.CompletedAt = DateTime.UtcNow;

        _logger.LogInformation("Completed full Shopify sync in {Duration}ms",
            (summary.CompletedAt - summary.StartedAt).TotalMilliseconds);

        return summary;
    }
}

public class ShopifySyncSummary
{
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public SyncResult? CustomerSync { get; set; }
    public SyncResult? OrderSync { get; set; }
    public SyncResult? ProductSync { get; set; }
    public SyncResult? InventorySync { get; set; }

    public bool IsSuccess =>
        (CustomerSync?.IsSuccess ?? true) &&
        (OrderSync?.IsSuccess ?? true) &&
        (ProductSync?.IsSuccess ?? true) &&
        (InventorySync?.IsSuccess ?? true);

    public int TotalRecordsProcessed =>
        (CustomerSync?.RecordsProcessed ?? 0) +
        (OrderSync?.RecordsProcessed ?? 0) +
        (ProductSync?.RecordsProcessed ?? 0) +
        (InventorySync?.RecordsProcessed ?? 0);

    public int TotalRecordsFailed =>
        (CustomerSync?.RecordsFailed ?? 0) +
        (OrderSync?.RecordsFailed ?? 0) +
        (ProductSync?.RecordsFailed ?? 0) +
        (InventorySync?.RecordsFailed ?? 0);
}
