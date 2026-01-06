using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Common.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Web.Pages.Admin.Integrations;

public class IndexModel : PageModel
{
    private readonly IOptions<CrmIntegrationsSettings> _settings;
    private readonly IEnumerable<ICrmSyncService> _syncServices;

    public IndexModel(
        IOptions<CrmIntegrationsSettings> settings,
        IEnumerable<ICrmSyncService> syncServices)
    {
        _settings = settings;
        _syncServices = syncServices;
    }

    public bool SalesforceEnabled { get; set; }
    public bool Dynamics365Enabled { get; set; }
    public CrmStats SalesforceStats { get; set; } = new();
    public CrmStats Dynamics365Stats { get; set; } = new();
    public List<CrmSyncLog> RecentSyncLogs { get; set; } = new();

    public void OnGet()
    {
        SalesforceEnabled = _settings.Value.Salesforce.Enabled;
        Dynamics365Enabled = _settings.Value.Dynamics365.Enabled;

        // TODO: Load actual stats from database
        SalesforceStats = new CrmStats { Contacts = 0, Leads = 0, Opportunities = 0, Accounts = 0 };
        Dynamics365Stats = new CrmStats { Contacts = 0, Leads = 0, Opportunities = 0, Accounts = 0 };
    }

    public async Task<IActionResult> OnPostSyncSalesforceAsync()
    {
        var service = _syncServices.FirstOrDefault(s => s.CrmType == "Salesforce");
        if (service == null)
        {
            TempData["Error"] = "Salesforce integration is not configured";
            return RedirectToPage();
        }

        try
        {
            var result = await service.FullSyncAsync();
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
        var service = _syncServices.FirstOrDefault(s => s.CrmType == "Dynamics365");
        if (service == null)
        {
            TempData["Error"] = "Dynamics 365 integration is not configured";
            return RedirectToPage();
        }

        try
        {
            var result = await service.FullSyncAsync();
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

    public class CrmStats
    {
        public int Contacts { get; set; }
        public int Leads { get; set; }
        public int Opportunities { get; set; }
        public int Accounts { get; set; }
    }
}
