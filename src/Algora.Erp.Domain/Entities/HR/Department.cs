using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.HR;

public class Department : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? ManagerId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public Department? ParentDepartment { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Department> SubDepartments { get; set; } = new List<Department>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}
