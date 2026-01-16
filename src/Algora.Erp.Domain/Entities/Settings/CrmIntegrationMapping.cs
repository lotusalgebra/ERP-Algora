namespace Algora.Erp.Domain.Entities.Settings;

/// <summary>
/// Maps ERP entities to their corresponding CRM entities for synchronization
/// </summary>
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
