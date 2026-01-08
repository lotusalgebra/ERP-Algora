namespace Algora.Erp.Admin.Entities;

/// <summary>
/// Represents a database backup for a tenant
/// </summary>
public class DatabaseBackup
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public string DatabaseName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public BackupType Type { get; set; } = BackupType.Full;
    public BackupStatus Status { get; set; } = BackupStatus.Pending;

    public long? FileSizeBytes { get; set; }
    public string? FileSizeFormatted => FormatFileSize(FileSizeBytes);

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue && StartedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;

    public string? ErrorMessage { get; set; }

    // Retention
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt;

    // Audit
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public string? Notes { get; set; }

    private static string? FormatFileSize(long? bytes)
    {
        if (!bytes.HasValue) return null;

        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes.Value;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len /= 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public enum BackupType
{
    Full = 0,
    Differential = 1,
    TransactionLog = 2
}

public enum BackupStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    Deleted = 4
}
