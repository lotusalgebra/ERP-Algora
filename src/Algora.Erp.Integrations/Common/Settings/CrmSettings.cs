namespace Algora.Erp.Integrations.Common.Settings;

public class CrmIntegrationsSettings
{
    public const string SectionName = "CrmIntegrations";

    public SalesforceSettings Salesforce { get; set; } = new();
    public Dynamics365Settings Dynamics365 { get; set; } = new();
    public ShopifySettings Shopify { get; set; } = new();
}

public class SalesforceSettings
{
    public bool Enabled { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SecurityToken { get; set; } = string.Empty;
    public string InstanceUrl { get; set; } = "https://login.salesforce.com";
    public string ApiVersion { get; set; } = "v58.0";
    public int SyncIntervalMinutes { get; set; } = 30;
}

public class Dynamics365Settings
{
    public bool Enabled { get; set; }
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string InstanceUrl { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v9.2";
    public int SyncIntervalMinutes { get; set; } = 30;
}

public class ShopifySettings
{
    public bool Enabled { get; set; }
    public string ShopDomain { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-01";
    public int SyncIntervalMinutes { get; set; } = 15;
    public bool SyncCustomers { get; set; } = true;
    public bool SyncOrders { get; set; } = true;
    public bool SyncProducts { get; set; } = true;
    public bool SyncInventory { get; set; } = true;
}
