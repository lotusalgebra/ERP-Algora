using Algora.Erp.Domain.Entities.Ecommerce;

namespace Algora.Erp.Application.Common.Interfaces.Ecommerce;

/// <summary>
/// Service for managing eCommerce orders
/// </summary>
public interface IWebOrderService
{
    /// <summary>
    /// Creates an order from a cart
    /// </summary>
    Task<WebOrder> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by ID
    /// </summary>
    Task<WebOrder?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an order by order number
    /// </summary>
    Task<WebOrder?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists orders with filtering and pagination
    /// </summary>
    Task<OrderListResult> GetOrdersAsync(OrderListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets orders for a customer
    /// </summary>
    Task<List<WebOrder>> GetCustomerOrdersAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates order status
    /// </summary>
    Task UpdateWebOrderStatusAsync(Guid orderId, WebOrderStatus status, string? notes = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates payment status
    /// </summary>
    Task UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status, string? transactionId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds fulfillment/shipping info
    /// </summary>
    Task AddFulfillmentAsync(Guid orderId, FulfillmentDto fulfillment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an order
    /// </summary>
    Task CancelOrderAsync(Guid orderId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refunds an order
    /// </summary>
    Task RefundOrderAsync(Guid orderId, decimal amount, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates order invoice PDF
    /// </summary>
    byte[] GenerateInvoicePdf(WebOrder order);

    /// <summary>
    /// Generates packing slip PDF
    /// </summary>
    byte[] GeneratePackingSlipPdf(WebOrder order);

    /// <summary>
    /// Gets order statistics for dashboard
    /// </summary>
    Task<OrderStatistics> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a unique order number
    /// </summary>
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
}

public class CreateOrderRequest
{
    public Guid CartId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid ShippingAddressId { get; set; }
    public Guid? BillingAddressId { get; set; }
    public Guid ShippingMethodId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string? Notes { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public class OrderListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public WebOrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public FulfillmentStatus? FulfillmentStatus { get; set; }
    public Guid? CustomerId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class OrderListResult
{
    public List<OrderListItem> Orders { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class OrderListItem
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public WebOrderStatus Status { get; set; }
    public PaymentStatus PaymentStatus { get; set; }
    public FulfillmentStatus FulfillmentStatus { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

public class FulfillmentDto
{
    public string? TrackingNumber { get; set; }
    public string? Carrier { get; set; }
    public string? TrackingUrl { get; set; }
    public DateTime? ShippedDate { get; set; }
    public List<FulfillmentLineDto>? Lines { get; set; }
}

public class FulfillmentLineDto
{
    public Guid OrderItemId { get; set; }
    public int Quantity { get; set; }
}

public class OrderStatistics
{
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public int ThisWeekOrders { get; set; }
    public decimal ThisWeekRevenue { get; set; }
    public int ThisMonthOrders { get; set; }
    public decimal ThisMonthRevenue { get; set; }
    public Dictionary<WebOrderStatus, int> ByStatus { get; set; } = new();
    public Dictionary<PaymentStatus, int> ByPaymentStatus { get; set; } = new();
    public List<DailyOrderSummary> DailyTrend { get; set; } = new();
}

public class DailyOrderSummary
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}
