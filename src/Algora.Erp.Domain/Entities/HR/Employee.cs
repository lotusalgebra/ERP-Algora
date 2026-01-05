using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Enums;

namespace Algora.Erp.Domain.Entities.HR;

public class Employee : TenantEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? NationalId { get; set; }
    public string? PassportNumber { get; set; }
    public string? TaxId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? Avatar { get; set; }

    // Employment Details
    public Guid? DepartmentId { get; set; }
    public Guid? PositionId { get; set; }
    public Guid? ManagerId { get; set; }
    public DateTime HireDate { get; set; }
    public DateTime? TerminationDate { get; set; }
    public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
    public EmploymentStatus EmploymentStatus { get; set; } = EmploymentStatus.Active;
    public decimal? BaseSalary { get; set; }
    public string? SalaryCurrency { get; set; } = "USD";
    public PayFrequency PayFrequency { get; set; } = PayFrequency.Monthly;

    // Emergency Contact
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelation { get; set; }

    // Navigation Properties
    public Department? Department { get; set; }
    public Position? Position { get; set; }
    public Employee? Manager { get; set; }
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    public string FullName => $"{FirstName} {LastName}";
}
