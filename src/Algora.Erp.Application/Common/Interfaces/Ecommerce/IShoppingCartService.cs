using Algora.Erp.Domain.Entities.Ecommerce;

namespace Algora.Erp.Application.Common.Interfaces.Ecommerce;

/// <summary>
/// Service for managing shopping carts
/// </summary>
public interface IShoppingCartService
{
    /// <summary>
    /// Gets or creates a cart for the customer/session
    /// </summary>
    Task<ShoppingCart> GetOrCreateCartAsync(Guid? customerId, string? sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cart by ID with items
    /// </summary>
    Task<ShoppingCart?> GetCartByIdAsync(Guid cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cart by customer ID
    /// </summary>
    Task<ShoppingCart?> GetCartByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a cart by session ID
    /// </summary>
    Task<ShoppingCart?> GetCartBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds an item to the cart
    /// </summary>
    Task<CartItem> AddToCartAsync(AddToCartRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates cart item quantity
    /// </summary>
    Task<CartItem> UpdateCartItemAsync(Guid cartItemId, int quantity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes an item from the cart
    /// </summary>
    Task RemoveFromCartAsync(Guid cartItemId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears all items from the cart
    /// </summary>
    Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Applies a coupon to the cart
    /// </summary>
    Task<ApplyCouponResult> ApplyCouponAsync(Guid cartId, string couponCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a coupon from the cart
    /// </summary>
    Task RemoveCouponAsync(Guid cartId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates cart totals
    /// </summary>
    Task<CartTotals> CalculateTotalsAsync(Guid cartId, Guid? shippingMethodId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Merges a guest cart into a customer cart after login
    /// </summary>
    Task MergeCartsAsync(string sessionId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets abandoned carts for follow-up
    /// </summary>
    Task<List<AbandonedCartInfo>> GetAbandonedCartsAsync(TimeSpan abandonedThreshold, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets cart count for display
    /// </summary>
    Task<int> GetCartItemCountAsync(Guid? customerId, string? sessionId, CancellationToken cancellationToken = default);
}

public class AddToCartRequest
{
    public Guid CartId { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class ApplyCouponResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
}

public class CartTotals
{
    public decimal Subtotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CouponCode { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingAmount { get; set; }
    public string? ShippingMethodName { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
    public List<CartItemInfo> Items { get; set; } = new();
}

public class CartItemInfo
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }
    public string Sku { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
    public int AvailableStock { get; set; }
}

public class AbandonedCartInfo
{
    public Guid CartId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerName { get; set; }
    public decimal CartTotal { get; set; }
    public int ItemCount { get; set; }
    public DateTime UpdatedAt { get; set; }
    public TimeSpan AbandonedDuration { get; set; }
}
