using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;

namespace Algora.Erp.Domain.Entities.Quality;

public class QualityInspection : AuditableEntity
{
    public string InspectionNumber { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; }

    public InspectionType InspectionType { get; set; }

    // Source Document (GRN, Delivery Challan, Production Order)
    public string SourceDocumentType { get; set; } = string.Empty;
    public Guid SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public Guid? SupplierId { get; set; }

    public InspectionStatus Status { get; set; } = InspectionStatus.Pending;

    // Quantities
    public decimal TotalQuantity { get; set; }
    public decimal SampleSize { get; set; }
    public decimal InspectedQuantity { get; set; }
    public decimal PassedQuantity { get; set; }
    public decimal FailedQuantity { get; set; }

    // Inspector Details
    public Guid? InspectedBy { get; set; }
    public DateTime? InspectedAt { get; set; }
    public string? InspectorName { get; set; }

    // Approval
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApproverName { get; set; }

    // Result
    public QCOverallResult OverallResult { get; set; } = QCOverallResult.Pending;
    public string? ResultRemarks { get; set; }
    public string? Notes { get; set; }

    public ICollection<QualityParameter> Parameters { get; set; } = new List<QualityParameter>();
}

public class QualityParameter : AuditableEntity
{
    public Guid QualityInspectionId { get; set; }
    public QualityInspection? QualityInspection { get; set; }

    public int SequenceNumber { get; set; }
    public string ParameterName { get; set; } = string.Empty;
    public string? ParameterCode { get; set; }

    // Expected Values
    public string? ExpectedValue { get; set; }
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? Tolerance { get; set; }
    public string? Unit { get; set; }

    // Actual Values
    public string? ActualValue { get; set; }
    public decimal? MeasuredValue { get; set; }

    // Result
    public QCParameterResult Result { get; set; } = QCParameterResult.Pending;
    public string? Remarks { get; set; }

    // Evidence
    public string? AttachmentUrl { get; set; }
}

public enum InspectionType
{
    Incoming = 0,    // For goods receipt
    Outgoing = 1,    // For delivery/dispatch
    InProcess = 2,   // During production
    Random = 3,      // Random quality audit
    Final = 4        // Final inspection before shipment
}

public enum InspectionStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 9
}

public enum QCOverallResult
{
    Pending = 0,
    Pass = 1,
    Fail = 2,
    PartialPass = 3,
    ConditionalPass = 4
}

public enum QCParameterResult
{
    Pending = 0,
    Pass = 1,
    Fail = 2,
    Warning = 3,
    NotApplicable = 4
}
