using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Payroll;

public class SalaryComponent : TenantEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SalaryComponentType ComponentType { get; set; } = SalaryComponentType.Earning;
    public CalculationType CalculationType { get; set; } = CalculationType.Fixed;
    public decimal DefaultValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public bool IsTaxable { get; set; } = true;
    public bool IsRecurring { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    public ICollection<SalaryStructureLine> SalaryStructureLines { get; set; } = new List<SalaryStructureLine>();
}

public enum SalaryComponentType
{
    Earning,
    Deduction,
    Reimbursement,
    Tax
}

public enum CalculationType
{
    Fixed,
    PercentageOfBasic,
    PercentageOfGross,
    Formula
}
