using Algora.Erp.Integrations.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Algora.Erp.Web.Pages.Admin.Integrations;

public class SyncHistoryModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? CrmFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? EntityFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string DateRange { get; set; } = "7";

    [BindProperty(SupportsGet = true)]
    public int CurrentPage { get; set; } = 1;

    public int TotalPages { get; set; } = 1;
    public int TotalSyncs { get; set; }
    public int SuccessfulSyncs { get; set; }
    public int PartialSyncs { get; set; }
    public int FailedSyncs { get; set; }

    public List<CrmSyncLog> SyncLogs { get; set; } = new();

    public void OnGet()
    {
        // TODO: Load from database
        // For now, return sample data
        SyncLogs = new List<CrmSyncLog>
        {
            new CrmSyncLog
            {
                Id = Guid.NewGuid(),
                CrmType = "Salesforce",
                EntityType = "Contact",
                Direction = "Bidirectional",
                RecordsProcessed = 150,
                RecordsSucceeded = 148,
                RecordsFailed = 2,
                StartedAt = DateTime.UtcNow.AddHours(-2),
                CompletedAt = DateTime.UtcNow.AddHours(-2).AddSeconds(45),
                ErrorMessage = "2 contacts failed validation: missing email address"
            },
            new CrmSyncLog
            {
                Id = Guid.NewGuid(),
                CrmType = "Dynamics365",
                EntityType = "Account",
                Direction = "ToErp",
                RecordsProcessed = 75,
                RecordsSucceeded = 75,
                RecordsFailed = 0,
                StartedAt = DateTime.UtcNow.AddHours(-4),
                CompletedAt = DateTime.UtcNow.AddHours(-4).AddSeconds(23)
            },
            new CrmSyncLog
            {
                Id = Guid.NewGuid(),
                CrmType = "Salesforce",
                EntityType = "Lead",
                Direction = "ToCrm",
                RecordsProcessed = 45,
                RecordsSucceeded = 45,
                RecordsFailed = 0,
                StartedAt = DateTime.UtcNow.AddHours(-6),
                CompletedAt = DateTime.UtcNow.AddHours(-6).AddSeconds(12)
            },
            new CrmSyncLog
            {
                Id = Guid.NewGuid(),
                CrmType = "Dynamics365",
                EntityType = null,
                Direction = "Bidirectional",
                RecordsProcessed = 500,
                RecordsSucceeded = 500,
                RecordsFailed = 0,
                StartedAt = DateTime.UtcNow.AddDays(-1),
                CompletedAt = DateTime.UtcNow.AddDays(-1).AddMinutes(3)
            }
        };

        // Apply filters
        if (!string.IsNullOrEmpty(CrmFilter))
        {
            SyncLogs = SyncLogs.Where(l => l.CrmType == CrmFilter).ToList();
        }

        if (!string.IsNullOrEmpty(EntityFilter))
        {
            SyncLogs = SyncLogs.Where(l => l.EntityType == EntityFilter).ToList();
        }

        if (!string.IsNullOrEmpty(StatusFilter))
        {
            SyncLogs = StatusFilter switch
            {
                "success" => SyncLogs.Where(l => l.RecordsFailed == 0 && l.RecordsProcessed > 0).ToList(),
                "partial" => SyncLogs.Where(l => l.RecordsSucceeded > 0 && l.RecordsFailed > 0).ToList(),
                "failed" => SyncLogs.Where(l => l.RecordsSucceeded == 0 && l.RecordsFailed > 0).ToList(),
                _ => SyncLogs
            };
        }

        // Calculate stats
        TotalSyncs = SyncLogs.Count;
        SuccessfulSyncs = SyncLogs.Count(l => l.RecordsFailed == 0 && l.RecordsProcessed > 0);
        PartialSyncs = SyncLogs.Count(l => l.RecordsSucceeded > 0 && l.RecordsFailed > 0);
        FailedSyncs = SyncLogs.Count(l => l.RecordsSucceeded == 0 && l.RecordsFailed > 0);
    }
}
