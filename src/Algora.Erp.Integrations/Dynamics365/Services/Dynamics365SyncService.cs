using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Dynamics365.Client;
using Microsoft.Extensions.Logging;

namespace Algora.Erp.Integrations.Dynamics365.Services;

public class Dynamics365SyncService : ICrmSyncService
{
    private readonly Dynamics365Client _client;
    private readonly ICrmMappingService _mappingService;
    private readonly ILogger<Dynamics365SyncService> _logger;

    public string CrmType => "Dynamics365";

    public Dynamics365SyncService(
        Dynamics365Client client,
        ICrmMappingService mappingService,
        ILogger<Dynamics365SyncService> logger)
    {
        _client = client;
        _mappingService = mappingService;
        _logger = logger;
    }

    public async Task<SyncResult> SyncContactsAsync(SyncDirection direction, CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        var processed = 0;
        var created = 0;
        var updated = 0;
        var failed = 0;
        var errors = new List<SyncError>();

        try
        {
            _logger.LogInformation("Starting Dynamics 365 contact sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var contacts = await _client.QueryAsync<CrmContact>(
                    "$select=contactid,firstname,lastname,emailaddress1,telephone1,mobilephone," +
                    "jobtitle,department,address1_line1,address1_city,address1_stateorprovince," +
                    "address1_postalcode,address1_country,description,createdon,modifiedon" +
                    "&$filter=statecode eq 0&$top=1000", ct);

                foreach (var contact in contacts)
                {
                    processed++;
                    try
                    {
                        var mapping = await _mappingService.GetMappingByCrmIdAsync(
                            contact.Id!, "Contact", CrmType, ct);

                        if (mapping == null)
                        {
                            created++;
                        }
                        else
                        {
                            updated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        errors.Add(new SyncError
                        {
                            EntityId = contact.Id,
                            EntityType = "Contact",
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            _logger.LogInformation("Dynamics 365 contact sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                processed, created, updated, failed);

            return new SyncResult
            {
                CrmType = CrmType,
                EntityType = "Contact",
                Direction = direction,
                RecordsProcessed = processed,
                RecordsCreated = created,
                RecordsUpdated = updated,
                RecordsFailed = failed,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow,
                Errors = errors
            };
        }
        catch (CrmApiException ex)
        {
            _logger.LogError(ex, "Dynamics 365 contact sync failed");
            return SyncResult.Failure(CrmType, "Contact", direction, ex.Message);
        }
    }

    public async Task<SyncResult> SyncLeadsAsync(SyncDirection direction, CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        var processed = 0;
        var created = 0;
        var updated = 0;
        var failed = 0;
        var errors = new List<SyncError>();

        try
        {
            _logger.LogInformation("Starting Dynamics 365 lead sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var leads = await _client.QueryAsync<CrmLead>(
                    "$select=leadid,firstname,lastname,companyname,emailaddress1,telephone1," +
                    "mobilephone,jobtitle,industrycode,leadsourcecode,statuscode,leadqualitycode," +
                    "revenue,numberofemployees,address1_line1,address1_city,address1_stateorprovince," +
                    "address1_postalcode,address1_country,description,websiteurl,createdon,modifiedon" +
                    "&$filter=statecode eq 0&$top=1000", ct);

                foreach (var lead in leads)
                {
                    processed++;
                    try
                    {
                        var mapping = await _mappingService.GetMappingByCrmIdAsync(
                            lead.Id!, "Lead", CrmType, ct);

                        if (mapping == null)
                        {
                            created++;
                        }
                        else
                        {
                            updated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        errors.Add(new SyncError
                        {
                            EntityId = lead.Id,
                            EntityType = "Lead",
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            _logger.LogInformation("Dynamics 365 lead sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                processed, created, updated, failed);

            return new SyncResult
            {
                CrmType = CrmType,
                EntityType = "Lead",
                Direction = direction,
                RecordsProcessed = processed,
                RecordsCreated = created,
                RecordsUpdated = updated,
                RecordsFailed = failed,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow,
                Errors = errors
            };
        }
        catch (CrmApiException ex)
        {
            _logger.LogError(ex, "Dynamics 365 lead sync failed");
            return SyncResult.Failure(CrmType, "Lead", direction, ex.Message);
        }
    }

    public async Task<SyncResult> SyncOpportunitiesAsync(SyncDirection direction, CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        var processed = 0;
        var created = 0;
        var updated = 0;
        var failed = 0;
        var errors = new List<SyncError>();

        try
        {
            _logger.LogInformation("Starting Dynamics 365 opportunity sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var opportunities = await _client.QueryAsync<CrmOpportunity>(
                    "$select=opportunityid,name,_parentaccountid_value,totalamount," +
                    "salesstagecode,closeprobability,estimatedclosedate,description," +
                    "statecode,statuscode,createdon,modifiedon" +
                    "&$filter=statecode eq 0&$top=1000", ct);

                foreach (var opportunity in opportunities)
                {
                    processed++;
                    try
                    {
                        var mapping = await _mappingService.GetMappingByCrmIdAsync(
                            opportunity.Id!, "Opportunity", CrmType, ct);

                        if (mapping == null)
                        {
                            created++;
                        }
                        else
                        {
                            updated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        errors.Add(new SyncError
                        {
                            EntityId = opportunity.Id,
                            EntityType = "Opportunity",
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            _logger.LogInformation("Dynamics 365 opportunity sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                processed, created, updated, failed);

            return new SyncResult
            {
                CrmType = CrmType,
                EntityType = "Opportunity",
                Direction = direction,
                RecordsProcessed = processed,
                RecordsCreated = created,
                RecordsUpdated = updated,
                RecordsFailed = failed,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow,
                Errors = errors
            };
        }
        catch (CrmApiException ex)
        {
            _logger.LogError(ex, "Dynamics 365 opportunity sync failed");
            return SyncResult.Failure(CrmType, "Opportunity", direction, ex.Message);
        }
    }

    public async Task<SyncResult> SyncAccountsAsync(SyncDirection direction, CancellationToken ct = default)
    {
        var startedAt = DateTime.UtcNow;
        var processed = 0;
        var created = 0;
        var updated = 0;
        var failed = 0;
        var errors = new List<SyncError>();

        try
        {
            _logger.LogInformation("Starting Dynamics 365 account sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var accounts = await _client.QueryAsync<CrmAccount>(
                    "$select=accountid,name,accountnumber,industrycode,revenue," +
                    "numberofemployees,telephone1,fax,websiteurl,address1_line1," +
                    "address1_city,address1_stateorprovince,address1_postalcode," +
                    "address1_country,address2_line1,address2_city,address2_stateorprovince," +
                    "address2_postalcode,address2_country,description,_parentaccountid_value," +
                    "_ownerid_value,createdon,modifiedon" +
                    "&$filter=statecode eq 0&$top=1000", ct);

                foreach (var account in accounts)
                {
                    processed++;
                    try
                    {
                        var mapping = await _mappingService.GetMappingByCrmIdAsync(
                            account.Id!, "Account", CrmType, ct);

                        if (mapping == null)
                        {
                            created++;
                        }
                        else
                        {
                            updated++;
                        }
                    }
                    catch (Exception ex)
                    {
                        failed++;
                        errors.Add(new SyncError
                        {
                            EntityId = account.Id,
                            EntityType = "Account",
                            ErrorMessage = ex.Message
                        });
                    }
                }
            }

            _logger.LogInformation("Dynamics 365 account sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
                processed, created, updated, failed);

            return new SyncResult
            {
                CrmType = CrmType,
                EntityType = "Account",
                Direction = direction,
                RecordsProcessed = processed,
                RecordsCreated = created,
                RecordsUpdated = updated,
                RecordsFailed = failed,
                StartedAt = startedAt,
                CompletedAt = DateTime.UtcNow,
                Errors = errors
            };
        }
        catch (CrmApiException ex)
        {
            _logger.LogError(ex, "Dynamics 365 account sync failed");
            return SyncResult.Failure(CrmType, "Account", direction, ex.Message);
        }
    }

    public async Task<SyncResult> FullSyncAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting Dynamics 365 full sync");

        var results = new List<SyncResult>
        {
            await SyncAccountsAsync(SyncDirection.Bidirectional, ct),
            await SyncContactsAsync(SyncDirection.Bidirectional, ct),
            await SyncLeadsAsync(SyncDirection.Bidirectional, ct),
            await SyncOpportunitiesAsync(SyncDirection.Bidirectional, ct)
        };

        var totalProcessed = results.Sum(r => r.RecordsProcessed);
        var totalCreated = results.Sum(r => r.RecordsCreated);
        var totalUpdated = results.Sum(r => r.RecordsUpdated);
        var totalFailed = results.Sum(r => r.RecordsFailed);
        var allErrors = results.SelectMany(r => r.Errors).ToList();

        _logger.LogInformation("Dynamics 365 full sync completed: {Processed} total processed, {Created} created, {Updated} updated, {Failed} failed",
            totalProcessed, totalCreated, totalUpdated, totalFailed);

        return new SyncResult
        {
            CrmType = CrmType,
            EntityType = "All",
            Direction = SyncDirection.Bidirectional,
            RecordsProcessed = totalProcessed,
            RecordsCreated = totalCreated,
            RecordsUpdated = totalUpdated,
            RecordsFailed = totalFailed,
            StartedAt = results.First().StartedAt,
            CompletedAt = DateTime.UtcNow,
            Errors = allErrors
        };
    }
}
