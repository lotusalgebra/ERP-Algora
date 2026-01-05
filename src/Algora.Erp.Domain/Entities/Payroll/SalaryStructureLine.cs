using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Payroll;

public class SalaryStructureLine : TenantEntity
{
    public Guid SalaryStructureId { get; set; }
    public Guid SalaryComponentId { get; set; }
    public CalculationType CalculationType { get; set; } = CalculationType.Fixed;
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    public int SortOrder { get; set; }

    public SalaryStructure? SalaryStructure { get; set; }
    public SalaryComponent? Component { get; set; }
}
