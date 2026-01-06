using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Algora.Erp.Web.Pages.Shop;

public class CheckoutModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public CheckoutModel(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public List<ShippingMethod> ShippingMethods { get; set; } = new();
    public List<WebPaymentMethod> PaymentMethods { get; set; } = new();

    public async Task OnGetAsync()
    {
        ShippingMethods = await _context.ShippingMethods
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();

        PaymentMethods = await _context.WebPaymentMethods
            .Where(m => m.IsActive)
            .OrderBy(m => m.SortOrder)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(
        string email, string? phone,
        string firstName, string lastName,
        string address, string? address2,
        string city, string state, string postalCode, string country,
        Guid? shippingMethodId, Guid? paymentMethodId,
        string? notes, string cartData)
    {
        if (string.IsNullOrWhiteSpace(cartData))
        {
            return RedirectToPage("/Shop/Cart");
        }

        List<CartItem>? cartItems;
        try
        {
            cartItems = JsonSerializer.Deserialize<List<CartItem>>(cartData, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch
        {
            return RedirectToPage("/Shop/Cart");
        }

        if (cartItems == null || !cartItems.Any())
        {
            return RedirectToPage("/Shop/Cart");
        }

        // Find or create customer
        var customer = await _context.WebCustomers
            .FirstOrDefaultAsync(c => c.Email.ToLower() == email.ToLower());

        if (customer == null)
        {
            customer = new WebCustomer
            {
                Id = Guid.NewGuid(),
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Phone = phone,
                Address = address + (!string.IsNullOrEmpty(address2) ? ", " + address2 : ""),
                City = city,
                State = state,
                PostalCode = postalCode,
                Country = country,
                CreatedAt = _dateTime.UtcNow,
                IsActive = true
            };
            _context.WebCustomers.Add(customer);
        }
        else
        {
            // Update customer info
            customer.FirstName = firstName;
            customer.LastName = lastName;
            customer.Phone = phone;
            customer.Address = address + (!string.IsNullOrEmpty(address2) ? ", " + address2 : "");
            customer.City = city;
            customer.State = state;
            customer.PostalCode = postalCode;
            customer.Country = country;
            customer.ModifiedAt = _dateTime.UtcNow;
        }

        // Calculate totals
        decimal subtotal = 0;
        var orderItems = new List<WebOrderItem>();

        foreach (var cartItem in cartItems)
        {
            var product = await _context.EcommerceProducts.FindAsync(Guid.Parse(cartItem.ProductId));
            if (product == null) continue;

            var itemTotal = product.Price * cartItem.Quantity;
            subtotal += itemTotal;

            orderItems.Add(new WebOrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                ProductName = product.Name,
                Sku = product.Sku,
                Quantity = cartItem.Quantity,
                UnitPrice = product.Price,
                LineTotal = itemTotal
            });

            // Reduce stock
            product.StockQuantity -= cartItem.Quantity;
        }

        // Get shipping rate
        decimal shippingCost = 0;
        if (shippingMethodId.HasValue)
        {
            var shippingMethod = await _context.ShippingMethods.FindAsync(shippingMethodId.Value);
            if (shippingMethod != null)
            {
                shippingCost = shippingMethod.Rate;
            }
        }

        // Create order
        var order = new WebOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = GenerateOrderNumber(),
            CustomerId = customer.Id,
            CustomerEmail = email,
            CustomerPhone = phone,
            Status = WebOrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,

            // Shipping address
            ShippingFirstName = firstName,
            ShippingLastName = lastName,
            ShippingAddress1 = address,
            ShippingAddress2 = address2,
            ShippingCity = city,
            ShippingState = state,
            ShippingPostalCode = postalCode,
            ShippingCountry = country,
            ShippingPhone = phone,

            // Billing address (same as shipping for now)
            BillingFirstName = firstName,
            BillingLastName = lastName,
            BillingAddress1 = address,
            BillingAddress2 = address2,
            BillingCity = city,
            BillingState = state,
            BillingPostalCode = postalCode,
            BillingCountry = country,

            Subtotal = subtotal,
            ShippingAmount = shippingCost,
            TaxAmount = 0, // Calculate tax if needed
            Total = subtotal + shippingCost,

            Notes = notes,
            OrderDate = _dateTime.UtcNow,
            CreatedAt = _dateTime.UtcNow,

            Items = orderItems
        };

        _context.WebOrders.Add(order);

        // Update customer stats
        customer.OrderCount++;
        customer.TotalSpent += order.Total;

        await _context.SaveChangesAsync();

        return RedirectToPage("/Shop/OrderConfirmation", new { id = order.Id });
    }

    private string GenerateOrderNumber()
    {
        var timestamp = _dateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }

    public class CartItem
    {
        public string ProductId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public int Quantity { get; set; }
    }
}
