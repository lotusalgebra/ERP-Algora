namespace Algora.Erp.Domain.Entities.Common;

/// <summary>
/// Entity that belongs to a specific tenant
/// Used in tenant-specific databases
/// </summary>
public abstract class TenantEntity : AuditableEntity
{
    // TenantId is implicit - each tenant has its own database
    // This class is used for entities that exist within tenant databases
}
