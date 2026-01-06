using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Shopping cart
/// </summary>
public class ShoppingCart : BaseEntity
{
    public Guid? CustomerId { get; set; }
    public string? SessionId { get; set; }  // For guest carts

    public string? CouponCode { get; set; }
    public decimal DiscountAmount { get; set; }

    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public decimal Total { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? AbandonedAt { get; set; }
    public bool IsAbandoned { get; set; }

    // Recovery
    public int RemindersSent { get; set; }
    public DateTime? LastReminderAt { get; set; }

    public WebCustomer? Customer { get; set; }
    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}
