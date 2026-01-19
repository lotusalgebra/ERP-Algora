using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Settings;
using Algora.Erp.Integrations.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Algora.Erp.Infrastructure.Services;

public class CrmMappingService : ICrmMappingService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CrmMappingService> _logger;

    public CrmMappingService(
        IApplicationDbContext context,
        ILogger<CrmMappingService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CrmIntegrationMapping?> GetMappingAsync(
        Guid erpEntityId,
        string entityType,
        string crmType,
        CancellationToken ct = default)
    {
        return await _context.CrmIntegrationMappings
            .FirstOrDefaultAsync(m =>
                m.ErpEntityId == erpEntityId &&
                m.EntityType == entityType &&
                m.CrmType == crmType, ct);
    }

    public async Task<CrmIntegrationMapping?> GetMappingByCrmIdAsync(
        string crmEntityId,
        string entityType,
        string crmType,
        CancellationToken ct = default)
    {
        return await _context.CrmIntegrationMappings
            .FirstOrDefaultAsync(m =>
                m.CrmEntityId == crmEntityId &&
                m.EntityType == entityType &&
                m.CrmType == crmType, ct);
    }

    public async Task<CrmIntegrationMapping> CreateMappingAsync(
        CrmIntegrationMapping mapping,
        CancellationToken ct = default)
    {
        if (mapping.Id == Guid.Empty)
        {
            mapping.Id = Guid.NewGuid();
        }
        mapping.CreatedAt = DateTime.UtcNow;

        _context.CrmIntegrationMappings.Add(mapping);
        await _context.SaveChangesAsync(ct);

        _logger.LogDebug(
            "Created CRM mapping: {EntityType} ERP:{ErpId} -> CRM:{CrmId} ({CrmType})",
            mapping.EntityType, mapping.ErpEntityId, mapping.CrmEntityId, mapping.CrmType);

        return mapping;
    }

    public async Task UpdateMappingAsync(
        CrmIntegrationMapping mapping,
        CancellationToken ct = default)
    {
        var existing = await _context.CrmIntegrationMappings
            .FirstOrDefaultAsync(m => m.Id == mapping.Id, ct);

        if (existing != null)
        {
            existing.LastSyncedAt = mapping.LastSyncedAt;
            existing.SyncStatus = mapping.SyncStatus;
            existing.LastSyncError = mapping.LastSyncError;
            existing.CrmEntityId = mapping.CrmEntityId;

            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteMappingAsync(Guid id, CancellationToken ct = default)
    {
        var mapping = await _context.CrmIntegrationMappings
            .FirstOrDefaultAsync(m => m.Id == id, ct);

        if (mapping != null)
        {
            _context.CrmIntegrationMappings.Remove(mapping);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task<List<CrmIntegrationMapping>> GetMappingsByTypeAsync(
        string entityType,
        string crmType,
        CancellationToken ct = default)
    {
        return await _context.CrmIntegrationMappings
            .Where(m => m.EntityType == entityType && m.CrmType == crmType)
            .ToListAsync(ct);
    }
}
