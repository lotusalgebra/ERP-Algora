using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Enums;

namespace Algora.Erp.Domain.Entities.HR;

public class Attendance : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan? TotalWorkHours { get; set; }
    public TimeSpan? OvertimeHours { get; set; }

    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? Notes { get; set; }

    // Location tracking
    public string? CheckInLocation { get; set; }
    public string? CheckOutLocation { get; set; }
    public decimal? CheckInLatitude { get; set; }
    public decimal? CheckInLongitude { get; set; }
    public decimal? CheckOutLatitude { get; set; }
    public decimal? CheckOutLongitude { get; set; }

    // IP tracking for remote work
    public string? CheckInIpAddress { get; set; }
    public string? CheckOutIpAddress { get; set; }

    public bool IsLate { get; set; }
    public bool IsEarlyDeparture { get; set; }
    public bool IsApproved { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public enum AttendanceStatus
{
    Present = 0,
    Absent = 1,
    Late = 2,
    HalfDay = 3,
    OnLeave = 4,
    Holiday = 5,
    Weekend = 6,
    WorkFromHome = 7
}
