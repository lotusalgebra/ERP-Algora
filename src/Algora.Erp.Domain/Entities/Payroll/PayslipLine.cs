using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Payroll;

public class PayslipLine : TenantEntity
{
    public Guid PayslipId { get; set; }
    public Guid? SalaryComponentId { get; set; }
    public string ComponentCode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public SalaryComponentType ComponentType { get; set; }
    public CalculationType CalculationType { get; set; }
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    public bool IsTaxable { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }

    public Payslip? Payslip { get; set; }
    public SalaryComponent? SalaryComponent { get; set; }
}
