using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Enums;

namespace Algora.Erp.Domain.Entities.Payroll;

public class PayrollRun : TenantEntity
{
    public string RunNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public PayrollRunStatus Status { get; set; } = PayrollRunStatus.Draft;
    public PayFrequency PayFrequency { get; set; } = PayFrequency.Monthly;
    public string Currency { get; set; } = "USD";
    public int EmployeeCount { get; set; }
    public decimal TotalGrossPay { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetPay { get; set; }
    public decimal TotalEmployerCosts { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public Guid? ProcessedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? Notes { get; set; }

    public ICollection<Payslip> Payslips { get; set; } = new List<Payslip>();
}

public enum PayrollRunStatus
{
    Draft,
    Processing,
    Processed,
    Approved,
    Paid,
    Cancelled
}
