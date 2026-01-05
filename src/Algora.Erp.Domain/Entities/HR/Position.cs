using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.HR;

public class Position : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? DepartmentId { get; set; }
    public decimal? MinSalary { get; set; }
    public decimal? MaxSalary { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Department? Department { get; set; }
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
