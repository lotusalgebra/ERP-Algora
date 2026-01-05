using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.HR;

public class LeaveRequest : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public LeaveType LeaveType { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsHalfDay { get; set; }
    public HalfDayType? HalfDayType { get; set; }

    public decimal TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }

    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? RejectionReason { get; set; }

    public Guid? ApprovedBy { get; set; }
    public Employee? Approver { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Emergency contact during leave
    public string? EmergencyContact { get; set; }
    public string? HandoverNotes { get; set; }
}

public class LeaveBalance : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int Year { get; set; }
    public LeaveType LeaveType { get; set; }

    public decimal TotalEntitlement { get; set; }
    public decimal Used { get; set; }
    public decimal Pending { get; set; }
    public decimal CarriedForward { get; set; }
    public decimal Adjustments { get; set; }

    public decimal Available => TotalEntitlement + CarriedForward + Adjustments - Used - Pending;
}

public enum LeaveType
{
    Annual = 0,
    Sick = 1,
    Personal = 2,
    Maternity = 3,
    Paternity = 4,
    Bereavement = 5,
    Unpaid = 6,
    Compensatory = 7,
    Marriage = 8,
    Study = 9
}

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}

public enum HalfDayType
{
    FirstHalf = 0,
    SecondHalf = 1
}
