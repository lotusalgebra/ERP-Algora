using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Settings;

/// <summary>
/// Stores CRM/ERP integration configuration settings in the database
/// </summary>
public class IntegrationSettings : AuditableEntity
{
    /// <summary>
    /// Type of integration: "Salesforce", "Dynamics365", "Shopify"
    /// </summary>
    public string IntegrationType { get; set; } = string.Empty;

    /// <summary>
    /// Whether this integration is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// Non-sensitive configuration stored as JSON
    /// </summary>
    public string SettingsJson { get; set; } = "{}";

    /// <summary>
    /// Encrypted sensitive credentials (API keys, secrets, tokens)
    /// </summary>
    public string? EncryptedCredentials { get; set; }

    /// <summary>
    /// Sync interval in minutes
    /// </summary>
    public int SyncIntervalMinutes { get; set; } = 30;

    /// <summary>
    /// When the connection was last tested
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    /// <summary>
    /// Result of the last connection test
    /// </summary>
    public bool? LastTestSuccess { get; set; }

    /// <summary>
    /// Error message from last test if failed
    /// </summary>
    public string? LastTestError { get; set; }

    /// <summary>
    /// When the last sync completed
    /// </summary>
    public DateTime? LastSyncAt { get; set; }

    /// <summary>
    /// Whether the last sync was successful
    /// </summary>
    public bool? LastSyncSuccess { get; set; }

    /// <summary>
    /// Number of records processed in last sync
    /// </summary>
    public int? LastSyncRecordsProcessed { get; set; }
}

/// <summary>
/// Non-sensitive Salesforce configuration
/// </summary>
public class SalesforceSettingsData
{
    public string InstanceUrl { get; set; } = "https://login.salesforce.com";
    public string ApiVersion { get; set; } = "v58.0";
    public string Username { get; set; } = string.Empty;
}

/// <summary>
/// Sensitive Salesforce credentials
/// </summary>
public class SalesforceCredentials
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string SecurityToken { get; set; } = string.Empty;
}

/// <summary>
/// Non-sensitive Dynamics 365 configuration
/// </summary>
public class Dynamics365SettingsData
{
    public string TenantId { get; set; } = string.Empty;
    public string InstanceUrl { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "v9.2";
}

/// <summary>
/// Sensitive Dynamics 365 credentials
/// </summary>
public class Dynamics365Credentials
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// Non-sensitive Shopify configuration
/// </summary>
public class ShopifySettingsData
{
    public string ShopDomain { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2024-01";
    public bool SyncCustomers { get; set; } = true;
    public bool SyncOrders { get; set; } = true;
    public bool SyncProducts { get; set; } = true;
    public bool SyncInventory { get; set; } = true;
}

/// <summary>
/// Sensitive Shopify credentials
/// </summary>
public class ShopifyCredentials
{
    public string AccessToken { get; set; } = string.Empty;
}
