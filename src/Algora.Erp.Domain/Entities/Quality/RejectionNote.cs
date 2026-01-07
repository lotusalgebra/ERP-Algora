using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Procurement;

namespace Algora.Erp.Domain.Entities.Quality;

public class RejectionNote : AuditableEntity
{
    public string RejectionNumber { get; set; } = string.Empty;
    public DateTime RejectionDate { get; set; }

    // Source Document
    public string SourceDocumentType { get; set; } = string.Empty;
    public Guid SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }

    // Quality Inspection Reference
    public Guid? QualityInspectionId { get; set; }
    public QualityInspection? QualityInspection { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;

    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public Guid? SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    // Quantities
    public decimal RejectedQuantity { get; set; }
    public string UnitOfMeasure { get; set; } = "EA";
    public decimal UnitCost { get; set; }
    public decimal TotalValue { get; set; }

    // Rejection Details
    public string RejectionReason { get; set; } = string.Empty;
    public RejectionCategory RejectionCategory { get; set; }
    public string? DefectDescription { get; set; }

    // Disposition
    public RejectionDisposition DispositionStatus { get; set; } = RejectionDisposition.Pending;
    public string? DispositionAction { get; set; }
    public DateTime? DispositionDate { get; set; }
    public Guid? DisposedBy { get; set; }
    public string? DisposerName { get; set; }

    // Supplier Return (if applicable)
    public Guid? DebitNoteId { get; set; }
    public string? DebitNoteNumber { get; set; }
    public DateTime? ReturnDate { get; set; }
    public string? ReturnReference { get; set; }

    // Scrap (if applicable)
    public DateTime? ScrapDate { get; set; }
    public string? ScrapReference { get; set; }
    public decimal? ScrapValue { get; set; }

    // Rework (if applicable)
    public string? ReworkInstructions { get; set; }
    public DateTime? ReworkCompletedAt { get; set; }
    public decimal? ReworkCost { get; set; }

    public string? Notes { get; set; }
    public string? AttachmentUrls { get; set; } // JSON array of attachment URLs
}

public enum RejectionCategory
{
    QualityDefect = 0,
    DamagedInTransit = 1,
    WrongProduct = 2,
    QuantityMismatch = 3,
    ExpiredProduct = 4,
    PackagingDefect = 5,
    DocumentationError = 6,
    ContaminationIssue = 7,
    SpecificationMismatch = 8,
    Other = 99
}

public enum RejectionDisposition
{
    Pending = 0,
    ReturnToSupplier = 1,
    Scrapped = 2,
    Reworked = 3,
    Disposed = 4,
    AcceptedWithDeviation = 5,
    HoldForReview = 6
}
