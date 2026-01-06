using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Common.Interfaces;
using Algora.Erp.Integrations.Common.Models;
using Algora.Erp.Integrations.Salesforce.Client;
using Microsoft.Extensions.Logging;

namespace Algora.Erp.Integrations.Salesforce.Services;

public class SalesforceSyncService : ICrmSyncService
{
    private readonly SalesforceClient _client;
    private readonly ICrmMappingService _mappingService;
    private readonly ILogger<SalesforceSyncService> _logger;

    public string CrmType => "Salesforce";

    public SalesforceSyncService(
        SalesforceClient client,
        ICrmMappingService mappingService,
        ILogger<SalesforceSyncService> logger)
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
            _logger.LogInformation("Starting Salesforce contact sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var contacts = await _client.QueryAsync<CrmContact>(
                    "SELECT Id, FirstName, LastName, Email, Phone, MobilePhone, Title, Department, " +
                    "AccountId, Account.Name, MailingStreet, MailingCity, MailingState, " +
                    "MailingPostalCode, MailingCountry, Description, CreatedDate, LastModifiedDate " +
                    "FROM Contact WHERE IsDeleted = false LIMIT 1000", ct);

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

            _logger.LogInformation("Salesforce contact sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
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
            _logger.LogError(ex, "Salesforce contact sync failed");
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
            _logger.LogInformation("Starting Salesforce lead sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var leads = await _client.QueryAsync<CrmLead>(
                    "SELECT Id, FirstName, LastName, Company, Email, Phone, MobilePhone, Title, " +
                    "Industry, LeadSource, Status, Rating, AnnualRevenue, NumberOfEmployees, " +
                    "Street, City, State, PostalCode, Country, Description, Website, " +
                    "IsConverted, ConvertedDate, ConvertedAccountId, ConvertedContactId, " +
                    "ConvertedOpportunityId, CreatedDate, LastModifiedDate " +
                    "FROM Lead WHERE IsDeleted = false LIMIT 1000", ct);

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

            _logger.LogInformation("Salesforce lead sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
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
            _logger.LogError(ex, "Salesforce lead sync failed");
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
            _logger.LogInformation("Starting Salesforce opportunity sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var opportunities = await _client.QueryAsync<CrmOpportunity>(
                    "SELECT Id, Name, AccountId, Account.Name, Amount, StageName, Probability, " +
                    "CloseDate, Type, LeadSource, NextStep, Description, IsClosed, IsWon, " +
                    "ForecastCategory, CreatedDate, LastModifiedDate " +
                    "FROM Opportunity WHERE IsDeleted = false LIMIT 1000", ct);

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

            _logger.LogInformation("Salesforce opportunity sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
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
            _logger.LogError(ex, "Salesforce opportunity sync failed");
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
            _logger.LogInformation("Starting Salesforce account sync, direction: {Direction}", direction);

            if (direction == SyncDirection.ToErp || direction == SyncDirection.Bidirectional)
            {
                var accounts = await _client.QueryAsync<CrmAccount>(
                    "SELECT Id, Name, AccountNumber, Type, Industry, AnnualRevenue, " +
                    "NumberOfEmployees, Phone, Fax, Website, BillingStreet, BillingCity, " +
                    "BillingState, BillingPostalCode, BillingCountry, ShippingStreet, " +
                    "ShippingCity, ShippingState, ShippingPostalCode, ShippingCountry, " +
                    "Description, ParentId, OwnerId, Rating, CreatedDate, LastModifiedDate " +
                    "FROM Account WHERE IsDeleted = false LIMIT 1000", ct);

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

            _logger.LogInformation("Salesforce account sync completed: {Processed} processed, {Created} created, {Updated} updated, {Failed} failed",
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
            _logger.LogError(ex, "Salesforce account sync failed");
            return SyncResult.Failure(CrmType, "Account", direction, ex.Message);
        }
    }

    public async Task<SyncResult> FullSyncAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting Salesforce full sync");

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

        _logger.LogInformation("Salesforce full sync completed: {Processed} total processed, {Created} created, {Updated} updated, {Failed} failed",
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
