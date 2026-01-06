using Algora.Erp.Integrations.Common.Models;

namespace Algora.Erp.Integrations.Common.Interfaces;

public interface ICrmMappingService
{
    Task<CrmIntegrationMapping?> GetMappingAsync(Guid erpEntityId, string entityType, string crmType, CancellationToken ct = default);
    Task<CrmIntegrationMapping?> GetMappingByCrmIdAsync(string crmEntityId, string entityType, string crmType, CancellationToken ct = default);
    Task<CrmIntegrationMapping> CreateMappingAsync(CrmIntegrationMapping mapping, CancellationToken ct = default);
    Task UpdateMappingAsync(CrmIntegrationMapping mapping, CancellationToken ct = default);
    Task DeleteMappingAsync(Guid id, CancellationToken ct = default);
    Task<List<CrmIntegrationMapping>> GetMappingsByTypeAsync(string entityType, string crmType, CancellationToken ct = default);
}
