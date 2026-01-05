using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.HR;

namespace Algora.Erp.Domain.Entities.Payroll;

public class Payslip : TenantEntity
{
    public string PayslipNumber { get; set; } = string.Empty;
    public Guid PayrollRunId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public DateTime PayDate { get; set; }
    public PayslipStatus Status { get; set; } = PayslipStatus.Draft;

    // Work Details
    public decimal WorkingDays { get; set; }
    public decimal DaysWorked { get; set; }
    public decimal LeavesTaken { get; set; }
    public decimal Overtime { get; set; }

    // Amounts
    public decimal BasicSalary { get; set; }
    public decimal GrossPay { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TaxableAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal NetPay { get; set; }

    // Payment Details
    public string? PaymentMethod { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionReference { get; set; }

    public string? Notes { get; set; }

    public PayrollRun? PayrollRun { get; set; }
    public Employee? Employee { get; set; }
    public ICollection<PayslipLine> Lines { get; set; } = new List<PayslipLine>();
}

public enum PayslipStatus
{
    Draft,
    Processed,
    Approved,
    Paid,
    Cancelled
}
