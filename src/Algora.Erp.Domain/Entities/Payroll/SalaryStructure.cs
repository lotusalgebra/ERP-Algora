using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Enums;

namespace Algora.Erp.Domain.Entities.Payroll;

public class SalaryStructure : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal BaseSalary { get; set; }
    public string Currency { get; set; } = "USD";
    public PayFrequency PayFrequency { get; set; } = PayFrequency.Monthly;
    public bool IsActive { get; set; } = true;

    public ICollection<SalaryStructureLine> Lines { get; set; } = new List<SalaryStructureLine>();

    public decimal TotalEarnings => Lines.Where(l => l.Component?.ComponentType == SalaryComponentType.Earning)
        .Sum(l => l.Amount);

    public decimal TotalDeductions => Lines.Where(l => l.Component?.ComponentType == SalaryComponentType.Deduction)
        .Sum(l => l.Amount);

    public decimal NetSalary => TotalEarnings - TotalDeductions;
}
