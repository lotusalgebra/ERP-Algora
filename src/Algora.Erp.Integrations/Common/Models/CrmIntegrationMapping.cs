namespace Algora.Erp.Integrations.Common.Models;

public class CrmIntegrationMapping
{
    public Guid Id { get; set; }
    public string CrmType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public Guid ErpEntityId { get; set; }
    public string CrmEntityId { get; set; } = string.Empty;
    public DateTime LastSyncedAt { get; set; }
    public string SyncStatus { get; set; } = "Synced";
    public string? LastSyncError { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CrmSyncLog
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CrmType { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string Direction { get; set; } = string.Empty;
    public int RecordsProcessed { get; set; }
    public int RecordsSucceeded { get; set; }
    public int RecordsFailed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TenantCrmCredentials
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string CrmType { get; set; } = string.Empty;
    public string EncryptedCredentials { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
    public DateTime? LastValidatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
