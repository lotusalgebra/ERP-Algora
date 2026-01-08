using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Admin.Services;

public interface IBackupService
{
    Task<List<DatabaseBackup>> GetBackupsAsync(Guid tenantId);
    Task<DatabaseBackup?> GetBackupByIdAsync(Guid backupId);
    Task<DatabaseBackup> CreateBackupAsync(Guid tenantId, BackupType type, Guid createdBy, string? notes = null);
    Task<bool> DeleteBackupAsync(Guid backupId);
    Task<bool> RestoreBackupAsync(Guid backupId, string? targetDatabaseName = null);
    Task CleanupExpiredBackupsAsync();
    Task<BackupStats> GetBackupStatsAsync(Guid? tenantId = null);
    string GetBackupDirectory();
}

public class BackupService : IBackupService
{
    private readonly AdminDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BackupService> _logger;
    private readonly string _backupPath;
    private readonly int _retentionDays;

    public BackupService(
        AdminDbContext context,
        IConfiguration configuration,
        ILogger<BackupService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;

        // Get backup settings from configuration
        _backupPath = _configuration["Backup:Path"] ?? Path.Combine(AppContext.BaseDirectory, "Backups");
        _retentionDays = _configuration.GetValue<int>("Backup:RetentionDays", 30);

        // Ensure backup directory exists
        if (!Directory.Exists(_backupPath))
        {
            Directory.CreateDirectory(_backupPath);
        }
    }

    public string GetBackupDirectory() => _backupPath;

    public async Task<List<DatabaseBackup>> GetBackupsAsync(Guid tenantId)
    {
        return await _context.DatabaseBackups
            .Where(b => b.TenantId == tenantId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();
    }

    public async Task<DatabaseBackup?> GetBackupByIdAsync(Guid backupId)
    {
        return await _context.DatabaseBackups
            .Include(b => b.Tenant)
            .FirstOrDefaultAsync(b => b.Id == backupId);
    }

    public async Task<DatabaseBackup> CreateBackupAsync(Guid tenantId, BackupType type, Guid createdBy, string? notes = null)
    {
        var tenant = await _context.Tenants.FindAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant not found");

        if (string.IsNullOrWhiteSpace(tenant.DatabaseName))
        {
            throw new InvalidOperationException("Tenant database name is not configured");
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        var backupTypeName = type switch
        {
            BackupType.Full => "FULL",
            BackupType.Differential => "DIFF",
            BackupType.TransactionLog => "LOG",
            _ => "FULL"
        };

        var fileName = $"{tenant.DatabaseName}_{backupTypeName}_{timestamp}.bak";
        var filePath = Path.Combine(_backupPath, tenant.Subdomain, fileName);

        // Ensure tenant backup directory exists
        var tenantBackupDir = Path.Combine(_backupPath, tenant.Subdomain);
        if (!Directory.Exists(tenantBackupDir))
        {
            Directory.CreateDirectory(tenantBackupDir);
        }

        var backup = new DatabaseBackup
        {
            TenantId = tenantId,
            DatabaseName = tenant.DatabaseName,
            FileName = fileName,
            FilePath = filePath,
            Type = type,
            Status = BackupStatus.Pending,
            CreatedBy = createdBy,
            Notes = notes,
            ExpiresAt = DateTime.UtcNow.AddDays(_retentionDays)
        };

        _context.DatabaseBackups.Add(backup);
        await _context.SaveChangesAsync();

        try
        {
            backup.Status = BackupStatus.InProgress;
            backup.StartedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Get connection string for tenant database
            var masterConnectionString = _configuration.GetConnectionString("AdminDb")!;
            var tenantConnectionString = BuildTenantConnectionString(masterConnectionString, tenant.DatabaseName);

            // Execute backup command
            await ExecuteBackupAsync(tenantConnectionString, tenant.DatabaseName, filePath, type);

            // Update backup record with success
            backup.Status = BackupStatus.Completed;
            backup.CompletedAt = DateTime.UtcNow;

            // Get file size
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                backup.FileSizeBytes = fileInfo.Length;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Backup completed successfully for tenant {TenantId}, Database: {DatabaseName}, File: {FileName}",
                tenantId, tenant.DatabaseName, fileName);
        }
        catch (Exception ex)
        {
            backup.Status = BackupStatus.Failed;
            backup.CompletedAt = DateTime.UtcNow;
            backup.ErrorMessage = ex.Message;
            await _context.SaveChangesAsync();

            _logger.LogError(ex,
                "Backup failed for tenant {TenantId}, Database: {DatabaseName}",
                tenantId, tenant.DatabaseName);

            throw;
        }

        return backup;
    }

    public async Task<bool> DeleteBackupAsync(Guid backupId)
    {
        var backup = await _context.DatabaseBackups.FindAsync(backupId);
        if (backup == null) return false;

        // Delete physical file if exists
        if (File.Exists(backup.FilePath))
        {
            try
            {
                File.Delete(backup.FilePath);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete backup file: {FilePath}", backup.FilePath);
            }
        }

        backup.Status = BackupStatus.Deleted;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Backup {BackupId} deleted", backupId);
        return true;
    }

    public async Task<bool> RestoreBackupAsync(Guid backupId, string? targetDatabaseName = null)
    {
        var backup = await _context.DatabaseBackups
            .Include(b => b.Tenant)
            .FirstOrDefaultAsync(b => b.Id == backupId);

        if (backup == null)
            throw new InvalidOperationException("Backup not found");

        if (backup.Status != BackupStatus.Completed)
            throw new InvalidOperationException("Cannot restore from incomplete backup");

        if (!File.Exists(backup.FilePath))
            throw new InvalidOperationException("Backup file not found on disk");

        var databaseToRestore = targetDatabaseName ?? backup.DatabaseName;

        try
        {
            var masterConnectionString = _configuration.GetConnectionString("AdminDb")!;

            // Use master database connection for restore
            var masterConn = new SqlConnectionStringBuilder(masterConnectionString)
            {
                InitialCatalog = "master"
            }.ConnectionString;

            await ExecuteRestoreAsync(masterConn, databaseToRestore, backup.FilePath);

            _logger.LogInformation(
                "Database {DatabaseName} restored successfully from backup {BackupId}",
                databaseToRestore, backupId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to restore database {DatabaseName} from backup {BackupId}",
                databaseToRestore, backupId);
            throw;
        }
    }

    public async Task CleanupExpiredBackupsAsync()
    {
        var expiredBackups = await _context.DatabaseBackups
            .Where(b => b.ExpiresAt.HasValue && b.ExpiresAt < DateTime.UtcNow && b.Status != BackupStatus.Deleted)
            .ToListAsync();

        foreach (var backup in expiredBackups)
        {
            await DeleteBackupAsync(backup.Id);
        }

        _logger.LogInformation("Cleaned up {Count} expired backups", expiredBackups.Count);
    }

    public async Task<BackupStats> GetBackupStatsAsync(Guid? tenantId = null)
    {
        var query = _context.DatabaseBackups.AsQueryable();

        if (tenantId.HasValue)
        {
            query = query.Where(b => b.TenantId == tenantId.Value);
        }

        var backups = await query.ToListAsync();

        return new BackupStats
        {
            TotalBackups = backups.Count(b => b.Status != BackupStatus.Deleted),
            CompletedBackups = backups.Count(b => b.Status == BackupStatus.Completed),
            FailedBackups = backups.Count(b => b.Status == BackupStatus.Failed),
            PendingBackups = backups.Count(b => b.Status == BackupStatus.Pending || b.Status == BackupStatus.InProgress),
            TotalSizeBytes = backups.Where(b => b.Status == BackupStatus.Completed).Sum(b => b.FileSizeBytes ?? 0),
            LastBackupDate = backups.Where(b => b.Status == BackupStatus.Completed)
                .OrderByDescending(b => b.CompletedAt)
                .FirstOrDefault()?.CompletedAt
        };
    }

    private static string BuildTenantConnectionString(string masterConnectionString, string databaseName)
    {
        var builder = new SqlConnectionStringBuilder(masterConnectionString)
        {
            InitialCatalog = databaseName
        };
        return builder.ConnectionString;
    }

    private async Task ExecuteBackupAsync(string connectionString, string databaseName, string backupPath, BackupType type)
    {
        var backupType = type switch
        {
            BackupType.Differential => "DATABASE DIFFERENTIAL",
            BackupType.TransactionLog => "LOG",
            _ => "DATABASE"
        };

        var sql = $@"
            BACKUP {backupType} [{databaseName}]
            TO DISK = @backupPath
            WITH FORMAT,
                 MEDIANAME = 'AlgoraErpBackup',
                 NAME = @backupName,
                 STATS = 10";

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = 3600; // 1 hour timeout for large databases
        command.Parameters.AddWithValue("@backupPath", backupPath);
        command.Parameters.AddWithValue("@backupName", $"{databaseName} - {type} Backup - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}");

        await command.ExecuteNonQueryAsync();
    }

    private async Task ExecuteRestoreAsync(string connectionString, string databaseName, string backupPath)
    {
        // First, set database to single user mode to disconnect all users
        var setSingleUserSql = $@"
            IF EXISTS (SELECT 1 FROM sys.databases WHERE name = @databaseName)
            BEGIN
                ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            END";

        // Restore the database
        var restoreSql = $@"
            RESTORE DATABASE [{databaseName}]
            FROM DISK = @backupPath
            WITH REPLACE,
                 STATS = 10";

        // Set back to multi-user mode
        var setMultiUserSql = $@"
            ALTER DATABASE [{databaseName}] SET MULTI_USER;";

        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        // Set single user
        using (var cmd = new SqlCommand(setSingleUserSql, connection))
        {
            cmd.Parameters.AddWithValue("@databaseName", databaseName);
            await cmd.ExecuteNonQueryAsync();
        }

        try
        {
            // Restore
            using (var cmd = new SqlCommand(restoreSql, connection))
            {
                cmd.CommandTimeout = 3600;
                cmd.Parameters.AddWithValue("@backupPath", backupPath);
                await cmd.ExecuteNonQueryAsync();
            }
        }
        finally
        {
            // Set multi-user
            using var cmd = new SqlCommand(setMultiUserSql, connection);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}

public class BackupStats
{
    public int TotalBackups { get; set; }
    public int CompletedBackups { get; set; }
    public int FailedBackups { get; set; }
    public int PendingBackups { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime? LastBackupDate { get; set; }

    public string TotalSizeFormatted
    {
        get
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = TotalSizeBytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }
    }
}
