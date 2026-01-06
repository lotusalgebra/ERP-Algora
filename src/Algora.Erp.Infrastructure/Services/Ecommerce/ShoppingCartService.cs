using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Services.Ecommerce;

/// <summary>
/// Service for managing shopping carts
/// </summary>
public class ShoppingCartService : IShoppingCartService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly ICouponService _couponService;

    public ShoppingCartService(
        IApplicationDbContext context,
        IDateTime dateTime,
        ICouponService couponService)
    {
        _context = context;
        _dateTime = dateTime;
        _couponService = couponService;
    }

    public async Task<ShoppingCart> GetOrCreateCartAsync(Guid? customerId, string? sessionId, CancellationToken cancellationToken = default)
    {
        ShoppingCart? cart = null;

        if (customerId.HasValue)
        {
            cart = await GetCartByCustomerIdAsync(customerId.Value, cancellationToken);
        }
        else if (!string.IsNullOrEmpty(sessionId))
        {
            cart = await GetCartBySessionIdAsync(sessionId, cancellationToken);
        }

        if (cart == null)
        {
            cart = new ShoppingCart
            {
                CustomerId = customerId,
                SessionId = sessionId ?? Guid.NewGuid().ToString("N"),
                UpdatedAt = _dateTime.UtcNow
            };
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return cart;
    }

    public async Task<ShoppingCart?> GetCartByIdAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        return await _context.ShoppingCarts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.OrderBy(img => img.SortOrder).Take(1))
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken);
    }

    public async Task<ShoppingCart?> GetCartByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.ShoppingCarts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.OrderBy(img => img.SortOrder).Take(1))
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, cancellationToken);
    }

    public async Task<ShoppingCart?> GetCartBySessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        return await _context.ShoppingCarts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images.OrderBy(img => img.SortOrder).Take(1))
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.SessionId == sessionId && c.CustomerId == null, cancellationToken);
    }

    public async Task<CartItem> AddToCartAsync(AddToCartRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartByIdAsync(request.CartId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart with ID {request.CartId} not found.");

        var product = await _context.EcommerceProducts
            .Include(p => p.Images.OrderBy(i => i.SortOrder).Take(1))
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {request.ProductId} not found.");

        ProductVariant? variant = null;
        if (request.VariantId.HasValue)
        {
            variant = await _context.ProductVariants.FindAsync(new object[] { request.VariantId.Value }, cancellationToken)
                ?? throw new InvalidOperationException($"Variant with ID {request.VariantId} not found.");
        }

        // Check if item already exists in cart
        var existingItem = cart.Items.FirstOrDefault(i =>
            i.ProductId == request.ProductId &&
            i.VariantId == request.VariantId);

        if (existingItem != null)
        {
            existingItem.Quantity += request.Quantity;
            existingItem.LineTotal = existingItem.UnitPrice * existingItem.Quantity;
        }
        else
        {
            var unitPrice = variant?.Price ?? product.Price;
            var imageUrl = variant?.ImageUrl ?? product.Images.FirstOrDefault()?.Url;

            existingItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = product.Id,
                VariantId = variant?.Id,
                ProductName = product.Name,
                VariantName = variant?.Name,
                Sku = variant?.Sku ?? product.Sku,
                UnitPrice = unitPrice,
                Quantity = request.Quantity,
                LineTotal = unitPrice * request.Quantity,
                ImageUrl = imageUrl
            };
            _context.CartItems.Add(existingItem);
        }

        cart.UpdatedAt = _dateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return existingItem;
    }

    public async Task<CartItem> UpdateCartItemAsync(Guid cartItemId, int quantity, CancellationToken cancellationToken = default)
    {
        var item = await _context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == cartItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart item with ID {cartItemId} not found.");

        if (quantity <= 0)
        {
            _context.CartItems.Remove(item);
        }
        else
        {
            item.Quantity = quantity;
            item.LineTotal = item.UnitPrice * quantity;
        }

        item.Cart.UpdatedAt = _dateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return item;
    }

    public async Task RemoveFromCartAsync(Guid cartItemId, CancellationToken cancellationToken = default)
    {
        var item = await _context.CartItems
            .Include(i => i.Cart)
            .FirstOrDefaultAsync(i => i.Id == cartItemId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart item with ID {cartItemId} not found.");

        _context.CartItems.Remove(item);
        item.Cart.UpdatedAt = _dateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart with ID {cartId} not found.");

        foreach (var item in cart.Items.ToList())
        {
            _context.CartItems.Remove(item);
        }

        cart.CouponCode = null;
        cart.DiscountAmount = 0;
        cart.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<ApplyCouponResult> ApplyCouponAsync(Guid cartId, string couponCode, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartByIdAsync(cartId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart with ID {cartId} not found.");

        var subtotal = cart.Items.Sum(i => i.LineTotal);
        var validation = await _couponService.ValidateCouponAsync(couponCode, cart.CustomerId, subtotal, cancellationToken);

        if (!validation.IsValid)
        {
            return new ApplyCouponResult
            {
                Success = false,
                ErrorMessage = validation.ErrorMessage
            };
        }

        cart.CouponCode = couponCode;
        cart.DiscountAmount = validation.DiscountAmount;
        cart.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new ApplyCouponResult
        {
            Success = true,
            DiscountAmount = validation.DiscountAmount,
            CouponCode = couponCode
        };
    }

    public async Task RemoveCouponAsync(Guid cartId, CancellationToken cancellationToken = default)
    {
        var cart = await _context.ShoppingCarts.FindAsync(new object[] { cartId }, cancellationToken)
            ?? throw new InvalidOperationException($"Cart with ID {cartId} not found.");

        cart.CouponCode = null;
        cart.DiscountAmount = 0;
        cart.UpdatedAt = _dateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<CartTotals> CalculateTotalsAsync(Guid cartId, Guid? shippingMethodId = null, CancellationToken cancellationToken = default)
    {
        var cart = await GetCartByIdAsync(cartId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart with ID {cartId} not found.");

        var store = await _context.Stores.FirstOrDefaultAsync(cancellationToken);
        var taxRate = store?.TaxRate ?? 0;

        var items = cart.Items.Select(i => new CartItemInfo
        {
            Id = i.Id,
            ProductId = i.ProductId,
            VariantId = i.VariantId,
            ProductName = i.ProductName,
            VariantName = i.VariantName,
            ImageUrl = i.ImageUrl,
            Sku = i.Sku,
            UnitPrice = i.UnitPrice,
            Quantity = i.Quantity,
            LineTotal = i.LineTotal,
            AvailableStock = i.Variant?.StockQuantity ?? i.Product?.StockQuantity ?? 0
        }).ToList();

        var subtotal = items.Sum(i => i.LineTotal);
        var discountAmount = cart.DiscountAmount;

        // Calculate shipping
        decimal shippingAmount = 0;
        string? shippingMethodName = null;

        if (shippingMethodId.HasValue)
        {
            var shippingMethod = await _context.ShippingMethods.FindAsync(new object[] { shippingMethodId.Value }, cancellationToken);
            if (shippingMethod != null)
            {
                shippingMethodName = shippingMethod.Name;
                if (shippingMethod.FreeShippingThreshold.HasValue && subtotal >= shippingMethod.FreeShippingThreshold.Value)
                {
                    shippingAmount = 0;
                }
                else
                {
                    shippingAmount = shippingMethod.RateType switch
                    {
                        ShippingRateType.FlatRate => shippingMethod.Rate,
                        ShippingRateType.WeightBased => shippingMethod.RatePerKg.GetValueOrDefault() * items.Sum(i => i.Quantity), // Simplified
                        ShippingRateType.Free => 0,
                        _ => shippingMethod.Rate
                    };
                }
            }
        }

        var taxableAmount = subtotal - discountAmount;
        var taxAmount = Math.Round(taxableAmount * (taxRate / 100), 2);
        var total = taxableAmount + taxAmount + shippingAmount;

        return new CartTotals
        {
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            CouponCode = cart.CouponCode,
            TaxAmount = taxAmount,
            ShippingAmount = shippingAmount,
            ShippingMethodName = shippingMethodName,
            Total = total,
            ItemCount = items.Sum(i => i.Quantity),
            Items = items
        };
    }

    public async Task MergeCartsAsync(string sessionId, Guid customerId, CancellationToken cancellationToken = default)
    {
        var guestCart = await GetCartBySessionIdAsync(sessionId, cancellationToken);
        if (guestCart == null || !guestCart.Items.Any())
            return;

        var customerCart = await GetCartByCustomerIdAsync(customerId, cancellationToken);

        if (customerCart == null)
        {
            // Transfer guest cart to customer
            guestCart.CustomerId = customerId;
            guestCart.SessionId = null;
        }
        else
        {
            // Merge items
            foreach (var guestItem in guestCart.Items)
            {
                var existingItem = customerCart.Items.FirstOrDefault(i =>
                    i.ProductId == guestItem.ProductId &&
                    i.VariantId == guestItem.VariantId);

                if (existingItem != null)
                {
                    existingItem.Quantity += guestItem.Quantity;
                    existingItem.LineTotal = existingItem.UnitPrice * existingItem.Quantity;
                }
                else
                {
                    var newItem = new CartItem
                    {
                        CartId = customerCart.Id,
                        ProductId = guestItem.ProductId,
                        VariantId = guestItem.VariantId,
                        ProductName = guestItem.ProductName,
                        VariantName = guestItem.VariantName,
                        Sku = guestItem.Sku,
                        UnitPrice = guestItem.UnitPrice,
                        Quantity = guestItem.Quantity,
                        LineTotal = guestItem.LineTotal,
                        ImageUrl = guestItem.ImageUrl
                    };
                    _context.CartItems.Add(newItem);
                }
            }

            // Delete guest cart
            _context.ShoppingCarts.Remove(guestCart);
            customerCart.UpdatedAt = _dateTime.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<AbandonedCartInfo>> GetAbandonedCartsAsync(TimeSpan abandonedThreshold, CancellationToken cancellationToken = default)
    {
        var cutoffTime = _dateTime.UtcNow.Subtract(abandonedThreshold);

        var now = _dateTime.UtcNow;
        var abandonedCarts = await _context.ShoppingCarts
            .Include(c => c.Items)
            .Include(c => c.Customer)
            .Where(c => c.Items.Any() && c.UpdatedAt < cutoffTime)
            .Select(c => new
            {
                c.Id,
                c.CustomerId,
                CustomerEmail = c.Customer != null ? c.Customer.Email : null,
                CustomerName = c.Customer != null ? c.Customer.FirstName + " " + c.Customer.LastName : null,
                CartTotal = c.Items.Sum(i => i.LineTotal),
                ItemCount = c.Items.Sum(i => i.Quantity),
                c.UpdatedAt
            })
            .OrderByDescending(c => c.CartTotal)
            .ToListAsync(cancellationToken);

        return abandonedCarts.Select(c => new AbandonedCartInfo
        {
            CartId = c.Id,
            CustomerId = c.CustomerId,
            CustomerEmail = c.CustomerEmail,
            CustomerName = c.CustomerName?.Trim(),
            CartTotal = c.CartTotal,
            ItemCount = c.ItemCount,
            UpdatedAt = c.UpdatedAt ?? now,
            AbandonedDuration = now - (c.UpdatedAt ?? now)
        }).ToList();
    }

    public async Task<int> GetCartItemCountAsync(Guid? customerId, string? sessionId, CancellationToken cancellationToken = default)
    {
        ShoppingCart? cart = null;

        if (customerId.HasValue)
            cart = await GetCartByCustomerIdAsync(customerId.Value, cancellationToken);
        else if (!string.IsNullOrEmpty(sessionId))
            cart = await GetCartBySessionIdAsync(sessionId, cancellationToken);

        return cart?.Items.Sum(i => i.Quantity) ?? 0;
    }
}
