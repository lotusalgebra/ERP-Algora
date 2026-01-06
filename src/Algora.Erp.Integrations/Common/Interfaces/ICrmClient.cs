namespace Algora.Erp.Integrations.Common.Interfaces;

public interface ICrmClient
{
    string CrmType { get; }
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync<T>(string id, CancellationToken ct = default) where T : class;
    Task<List<T>> QueryAsync<T>(string query, CancellationToken ct = default) where T : class;
    Task<string> CreateAsync<T>(T entity, CancellationToken ct = default) where T : class;
    Task UpdateAsync<T>(string id, T entity, CancellationToken ct = default) where T : class;
    Task DeleteAsync(string id, string entityType, CancellationToken ct = default);
    Task<List<T>> GetModifiedSinceAsync<T>(DateTime since, CancellationToken ct = default) where T : class;
}
