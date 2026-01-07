using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;

namespace Algora.Erp.Domain.Entities.Procurement;

public class GoodsReceiptNote : AuditableEntity
{
    public string GrnNumber { get; set; } = string.Empty;
    public DateTime GrnDate { get; set; }

    public Guid? PurchaseOrderId { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }

    public Guid SupplierId { get; set; }
    public Supplier? Supplier { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public GoodsReceiptStatus Status { get; set; } = GoodsReceiptStatus.Draft;

    // Receipt Details
    public Guid? ReceivedBy { get; set; }
    public DateTime? ReceivedAt { get; set; }

    // Supplier Invoice Reference
    public string? SupplierInvoiceNumber { get; set; }
    public DateTime? SupplierInvoiceDate { get; set; }

    // Transport Details
    public string? VehicleNumber { get; set; }
    public string? DriverName { get; set; }
    public string? TransporterName { get; set; }
    public string? LrNumber { get; set; } // Lorry Receipt Number
    public DateTime? LrDate { get; set; }

    // Computed/Summary Fields
    public string? PurchaseOrderNumber { get; set; }
    public string? SupplierName { get; set; }
    public string? WayBillNumber { get; set; }
    public bool QCRequired { get; set; }

    // Quantity Totals
    public decimal TotalOrderedQuantity { get; set; }
    public decimal TotalReceivedQuantity { get; set; }
    public decimal TotalAcceptedQuantity { get; set; }
    public decimal TotalRejectedQuantity { get; set; }

    // Value Totals
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalValue { get; set; }

    // QC Timestamps
    public DateTime? QCCompletedAt { get; set; }

    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public ICollection<GoodsReceiptLine> Lines { get; set; } = new List<GoodsReceiptLine>();
}

public class GoodsReceiptLine : AuditableEntity
{
    public Guid GoodsReceiptNoteId { get; set; }
    public GoodsReceiptNote? GoodsReceiptNote { get; set; }

    public Guid? PurchaseOrderLineId { get; set; }
    public PurchaseOrderLine? PurchaseOrderLine { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;

    public int LineNumber { get; set; }

    // Quantities
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }

    public string UnitOfMeasure { get; set; } = "EA";
    public decimal UnitPrice { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }

    // QC Status
    public GoodsReceiptLineQCStatus QCStatus { get; set; } = GoodsReceiptLineQCStatus.Pending;
    public Guid? QualityInspectionId { get; set; }

    // Batch/Serial Tracking
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? SerialNumbers { get; set; }

    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
}

public enum GoodsReceiptStatus
{
    Draft = 0,
    Pending = 1,
    QCPending = 2,
    QCCompleted = 3,
    Accepted = 4,
    PartiallyAccepted = 5,
    Rejected = 6,
    Cancelled = 9
}

public enum GoodsReceiptLineQCStatus
{
    Pending = 0,
    Passed = 1,
    Failed = 2,
    PartialPass = 3,
    NotRequired = 4
}
