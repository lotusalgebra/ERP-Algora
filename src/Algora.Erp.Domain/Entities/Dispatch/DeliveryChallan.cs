using Algora.Erp.Domain.Entities.Common;
using Algora.Erp.Domain.Entities.Inventory;
using Algora.Erp.Domain.Entities.Sales;

namespace Algora.Erp.Domain.Entities.Dispatch;

public class DeliveryChallan : AuditableEntity
{
    public string ChallanNumber { get; set; } = string.Empty;
    public DateTime ChallanDate { get; set; }

    public Guid? SalesOrderId { get; set; }
    public SalesOrder? SalesOrder { get; set; }

    public Guid CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }

    public DeliveryChallanStatus Status { get; set; } = DeliveryChallanStatus.Draft;

    // Denormalized customer name for display
    public string? CustomerName { get; set; }

    // Transport Details
    public string? VehicleNumber { get; set; }
    public string? TransportMode { get; set; }
    public string? DriverName { get; set; }
    public string? DriverPhone { get; set; }
    public string? TransporterName { get; set; }

    // Timestamps
    public DateTime? ConfirmedAt { get; set; }
    public Guid? ConfirmedBy { get; set; }
    public DateTime? DispatchedAt { get; set; }
    public Guid? DispatchedBy { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public Guid? DeliveredBy { get; set; }

    // Shipping Address (ShipTo fields)
    public string? ShipToName { get; set; }
    public string? ShipToPhone { get; set; }
    public string? ShipToAddress1 { get; set; }
    public string? ShipToAddress2 { get; set; }
    public string? ShipToCity { get; set; }
    public string? ShipToState { get; set; }
    public string? ShipToPostalCode { get; set; }
    public string? ShipToCountry { get; set; }

    // Legacy shipping address (keep for backward compatibility)
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingCountry { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }

    // Package Information
    public int? TotalPackages { get; set; }
    public decimal? TotalWeight { get; set; }
    public string? WeightUnit { get; set; }
    public decimal TotalQuantity { get; set; }

    public string? Reference { get; set; }
    public string? Notes { get; set; }

    public ICollection<DeliveryChallanLine> Lines { get; set; } = new List<DeliveryChallanLine>();
}

public class DeliveryChallanLine : AuditableEntity
{
    public Guid DeliveryChallanId { get; set; }
    public DeliveryChallan? DeliveryChallan { get; set; }

    public Guid? SalesOrderLineId { get; set; }

    public Guid ProductId { get; set; }
    public Product? Product { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;

    public int LineNumber { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = "EA";

    public string? BatchNumber { get; set; }
    public string? SerialNumbers { get; set; }
    public string? Notes { get; set; }
}

public enum DeliveryChallanStatus
{
    Draft = 0,
    Confirmed = 1,
    Dispatched = 2,
    Delivered = 3,
    Cancelled = 9
}
