namespace Algora.Erp.Integrations.Common.Models;

public class SyncResult
{
    public string CrmType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public SyncDirection Direction { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccess => RecordsFailed == 0;
    public string? ErrorMessage { get; set; }
    public List<SyncError> Errors { get; set; } = new();

    public static SyncResult Success(string crmType, string entityType, SyncDirection direction, int processed, int created, int updated)
    {
        return new SyncResult
        {
            CrmType = crmType,
            EntityType = entityType,
            Direction = direction,
            RecordsProcessed = processed,
            RecordsCreated = created,
            RecordsUpdated = updated,
            RecordsFailed = 0,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };
    }

    public static SyncResult Failure(string crmType, string entityType, SyncDirection direction, string errorMessage)
    {
        return new SyncResult
        {
            CrmType = crmType,
            EntityType = entityType,
            Direction = direction,
            RecordsFailed = 1,
            ErrorMessage = errorMessage,
            StartedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow
        };
    }
}

public class SyncError
{
    public string? EntityId { get; set; }
    public string? EntityType { get; set; }
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
}

public enum SyncDirection
{
    ToErp,
    ToCrm,
    Bidirectional
}
