using Algora.Erp.Integrations.Common.Models;

namespace Algora.Erp.Integrations.Common.Interfaces;

public interface ICrmSyncService
{
    string CrmType { get; }
    Task<SyncResult> SyncContactsAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> SyncLeadsAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> SyncOpportunitiesAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> SyncAccountsAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> FullSyncAsync(CancellationToken ct = default);
}
