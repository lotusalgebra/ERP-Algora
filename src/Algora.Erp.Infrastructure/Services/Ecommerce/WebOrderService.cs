using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Algora.Erp.Infrastructure.Services.Ecommerce;

/// <summary>
/// Service for managing eCommerce orders
/// </summary>
public class WebOrderService : IWebOrderService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly IShoppingCartService _cartService;
    private readonly ICouponService _couponService;

    public WebOrderService(
        IApplicationDbContext context,
        IDateTime dateTime,
        IShoppingCartService cartService,
        ICouponService couponService)
    {
        _context = context;
        _dateTime = dateTime;
        _cartService = cartService;
        _couponService = couponService;
    }

    public async Task<WebOrder> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var cart = await _context.ShoppingCarts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .Include(c => c.Items)
                .ThenInclude(i => i.Variant)
            .FirstOrDefaultAsync(c => c.Id == request.CartId, cancellationToken)
            ?? throw new InvalidOperationException($"Cart with ID {request.CartId} not found.");

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cannot create order from empty cart.");

        var customer = await _context.WebCustomers.FindAsync(new object[] { request.CustomerId }, cancellationToken)
            ?? throw new InvalidOperationException($"Customer with ID {request.CustomerId} not found.");

        var shippingAddress = await _context.CustomerAddresses.FindAsync(new object[] { request.ShippingAddressId }, cancellationToken)
            ?? throw new InvalidOperationException($"Shipping address not found.");

        var billingAddress = request.BillingAddressId.HasValue
            ? await _context.CustomerAddresses.FindAsync(new object[] { request.BillingAddressId.Value }, cancellationToken)
            : shippingAddress;

        var shippingMethod = await _context.ShippingMethods.FindAsync(new object[] { request.ShippingMethodId }, cancellationToken)
            ?? throw new InvalidOperationException($"Shipping method not found.");

        var paymentMethod = await _context.WebPaymentMethods.FindAsync(new object[] { request.PaymentMethodId }, cancellationToken)
            ?? throw new InvalidOperationException($"Payment method not found.");

        var store = await _context.Stores.FirstOrDefaultAsync(cancellationToken);
        var taxRate = store?.TaxRate ?? 0;

        // Calculate totals
        var subtotal = cart.Items.Sum(i => i.LineTotal);
        var discountAmount = cart.DiscountAmount;

        // Calculate shipping
        decimal shippingCost = 0;
        if (shippingMethod.FreeShippingThreshold.HasValue && subtotal >= shippingMethod.FreeShippingThreshold.Value)
            shippingCost = 0;
        else
            shippingCost = shippingMethod.Rate;

        var taxableAmount = subtotal - discountAmount;
        var taxAmount = Math.Round(taxableAmount * (taxRate / 100), 2);
        var total = taxableAmount + taxAmount + shippingCost;

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);

        var order = new WebOrder
        {
            OrderNumber = orderNumber,
            OrderDate = _dateTime.UtcNow,
            CustomerId = customer.Id,
            CustomerEmail = customer.Email,
            CustomerPhone = customer.Phone,

            // Shipping Address
            ShippingFirstName = shippingAddress.FirstName,
            ShippingLastName = shippingAddress.LastName,
            ShippingCompany = shippingAddress.Company,
            ShippingAddress1 = shippingAddress.Address1,
            ShippingAddress2 = shippingAddress.Address2,
            ShippingCity = shippingAddress.City,
            ShippingState = shippingAddress.State,
            ShippingPostalCode = shippingAddress.PostalCode,
            ShippingCountry = shippingAddress.Country,
            ShippingPhone = shippingAddress.Phone,

            // Billing Address
            BillingFirstName = billingAddress!.FirstName,
            BillingLastName = billingAddress.LastName,
            BillingCompany = billingAddress.Company,
            BillingAddress1 = billingAddress.Address1,
            BillingAddress2 = billingAddress.Address2,
            BillingCity = billingAddress.City,
            BillingState = billingAddress.State,
            BillingPostalCode = billingAddress.PostalCode,
            BillingCountry = billingAddress.Country,

            // Totals
            Subtotal = subtotal,
            DiscountAmount = discountAmount,
            CouponCode = cart.CouponCode,
            TaxRate = taxRate,
            TaxAmount = taxAmount,
            ShippingAmount = shippingCost,
            Total = total,

            // Shipping & Payment
            ShippingMethod = shippingMethod.Name,
            PaymentMethod = paymentMethod.Name,

            // Status
            Status = WebOrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            FulfillmentStatus = FulfillmentStatus.Unfulfilled,

            // Meta
            Notes = request.Notes,
            IpAddress = request.IpAddress,
            UserAgent = request.UserAgent,
            Currency = store?.Currency ?? "USD"
        };

        _context.WebOrders.Add(order);

        // Create order items
        foreach (var cartItem in cart.Items)
        {
            var orderItem = new WebOrderItem
            {
                OrderId = order.Id,
                ProductId = cartItem.ProductId,
                VariantId = cartItem.VariantId,
                ProductName = cartItem.ProductName,
                VariantName = cartItem.VariantName,
                Sku = cartItem.Sku,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                LineTotal = cartItem.LineTotal,
                ImageUrl = cartItem.ImageUrl
            };
            _context.WebOrderItems.Add(orderItem);

            // Decrease stock
            if (cartItem.VariantId.HasValue && cartItem.Variant != null)
            {
                cartItem.Variant.StockQuantity -= cartItem.Quantity;
            }
            else if (cartItem.Product != null && cartItem.Product.TrackInventory)
            {
                cartItem.Product.StockQuantity -= cartItem.Quantity;
                cartItem.Product.SalesCount += cartItem.Quantity;
            }
        }

        // Record coupon usage
        if (!string.IsNullOrEmpty(cart.CouponCode))
        {
            var coupon = await _couponService.GetCouponByCodeAsync(cart.CouponCode, cancellationToken);
            if (coupon != null)
            {
                await _couponService.RecordUsageAsync(coupon.Id, order.Id, customer.Id, discountAmount, cancellationToken);
            }
        }

        // Update customer stats
        customer.OrderCount++;
        customer.TotalSpent += total;
        customer.LastOrderAt = _dateTime.UtcNow;

        // Clear the cart
        foreach (var item in cart.Items.ToList())
            _context.CartItems.Remove(item);
        cart.CouponCode = null;
        cart.DiscountAmount = 0;

        await _context.SaveChangesAsync(cancellationToken);

        return order;
    }

    public async Task<WebOrder?> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await _context.WebOrders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<WebOrder?> GetOrderByNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        return await _context.WebOrders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderNumber == orderNumber, cancellationToken);
    }

    public async Task<OrderListResult> GetOrdersAsync(OrderListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.WebOrders
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                (o.CustomerEmail != null && o.CustomerEmail.ToLower().Contains(term)) ||
                (o.Customer != null && (o.Customer.FirstName + " " + o.Customer.LastName).ToLower().Contains(term)));
        }

        if (request.Status.HasValue)
            query = query.Where(o => o.Status == request.Status.Value);

        if (request.PaymentStatus.HasValue)
            query = query.Where(o => o.PaymentStatus == request.PaymentStatus.Value);

        if (request.FulfillmentStatus.HasValue)
            query = query.Where(o => o.FulfillmentStatus == request.FulfillmentStatus.Value);

        if (request.CustomerId.HasValue)
            query = query.Where(o => o.CustomerId == request.CustomerId.Value);

        if (request.FromDate.HasValue)
            query = query.Where(o => o.OrderDate >= request.FromDate.Value);

        if (request.ToDate.HasValue)
            query = query.Where(o => o.OrderDate <= request.ToDate.Value.AddDays(1));

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy.ToLower() switch
        {
            "ordernumber" => request.SortDescending ? query.OrderByDescending(o => o.OrderNumber) : query.OrderBy(o => o.OrderNumber),
            "total" => request.SortDescending ? query.OrderByDescending(o => o.Total) : query.OrderBy(o => o.Total),
            "customer" => request.SortDescending
                ? query.OrderByDescending(o => o.Customer != null ? o.Customer.FirstName : "")
                : query.OrderBy(o => o.Customer != null ? o.Customer.FirstName : ""),
            _ => request.SortDescending ? query.OrderByDescending(o => o.OrderDate) : query.OrderBy(o => o.OrderDate)
        };

        var orders = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderListItem
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                CustomerName = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}" : null,
                CustomerEmail = o.CustomerEmail,
                Status = o.Status,
                PaymentStatus = o.PaymentStatus,
                FulfillmentStatus = o.FulfillmentStatus,
                Total = o.Total,
                ItemCount = o.Items.Sum(i => i.Quantity)
            })
            .ToListAsync(cancellationToken);

        return new OrderListResult
        {
            Orders = orders,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<List<WebOrder>> GetCustomerOrdersAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.WebOrders
            .Include(o => o.Items)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateWebOrderStatusAsync(Guid orderId, WebOrderStatus status, string? notes = null, CancellationToken cancellationToken = default)
    {
        var order = await _context.WebOrders.FindAsync(new object[] { orderId }, cancellationToken)
            ?? throw new InvalidOperationException($"Order with ID {orderId} not found.");

        order.Status = status;
        if (!string.IsNullOrEmpty(notes))
            order.Notes = string.IsNullOrEmpty(order.Notes) ? notes : $"{order.Notes}\n{notes}";

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdatePaymentStatusAsync(Guid orderId, PaymentStatus status, string? transactionId = null, CancellationToken cancellationToken = default)
    {
        var order = await _context.WebOrders.FindAsync(new object[] { orderId }, cancellationToken)
            ?? throw new InvalidOperationException($"Order with ID {orderId} not found.");

        order.PaymentStatus = status;

        if (!string.IsNullOrEmpty(transactionId))
            order.PaymentTransactionId = transactionId;

        if (status == PaymentStatus.Paid)
        {
            order.PaidAt = _dateTime.UtcNow;
            if (order.Status == WebOrderStatus.Pending)
                order.Status = WebOrderStatus.Processing;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AddFulfillmentAsync(Guid orderId, FulfillmentDto fulfillment, CancellationToken cancellationToken = default)
    {
        var order = await _context.WebOrders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order with ID {orderId} not found.");

        order.TrackingNumber = fulfillment.TrackingNumber;
        order.TrackingUrl = fulfillment.TrackingUrl;
        order.ShippedAt = fulfillment.ShippedDate ?? _dateTime.UtcNow;

        if (fulfillment.Lines != null && fulfillment.Lines.Any())
        {
            foreach (var line in fulfillment.Lines)
            {
                var orderItem = order.Items.FirstOrDefault(i => i.Id == line.OrderItemId);
                if (orderItem != null)
                {
                    orderItem.FulfilledQuantity += line.Quantity;
                }
            }
        }
        else
        {
            // Fulfill all items
            foreach (var item in order.Items)
            {
                item.FulfilledQuantity = item.Quantity;
            }
        }

        // Update fulfillment status
        var totalQuantity = order.Items.Sum(i => i.Quantity);
        var fulfilledQuantity = order.Items.Sum(i => i.FulfilledQuantity);

        if (fulfilledQuantity >= totalQuantity)
            order.FulfillmentStatus = FulfillmentStatus.Fulfilled;
        else if (fulfilledQuantity > 0)
            order.FulfillmentStatus = FulfillmentStatus.PartiallyFulfilled;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelOrderAsync(Guid orderId, string reason, CancellationToken cancellationToken = default)
    {
        var order = await _context.WebOrders
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
            .Include(o => o.Items)
                .ThenInclude(i => i.Variant)
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new InvalidOperationException($"Order with ID {orderId} not found.");

        if (order.Status == WebOrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        // Restore stock
        foreach (var item in order.Items)
        {
            if (item.VariantId.HasValue && item.Variant != null)
            {
                item.Variant.StockQuantity += item.Quantity;
            }
            else if (item.Product != null && item.Product.TrackInventory)
            {
                item.Product.StockQuantity += item.Quantity;
                item.Product.SalesCount -= item.Quantity;
            }
        }

        // Update customer stats
        if (order.Customer != null)
        {
            order.Customer.OrderCount--;
            order.Customer.TotalSpent -= order.Total;
        }

        order.Status = WebOrderStatus.Cancelled;
        order.CancelledAt = _dateTime.UtcNow;
        order.CancelReason = reason;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RefundOrderAsync(Guid orderId, decimal amount, string reason, CancellationToken cancellationToken = default)
    {
        var order = await _context.WebOrders.FindAsync(new object[] { orderId }, cancellationToken)
            ?? throw new InvalidOperationException($"Order with ID {orderId} not found.");

        if (order.PaymentStatus != PaymentStatus.Paid)
            throw new InvalidOperationException("Cannot refund unpaid order.");

        order.RefundedAmount += amount;
        order.Notes = string.IsNullOrEmpty(order.Notes)
            ? $"Refunded {amount:C2}: {reason}"
            : $"{order.Notes}\nRefunded {amount:C2}: {reason}";

        if (order.RefundedAmount >= order.Total)
            order.PaymentStatus = PaymentStatus.Refunded;
        else
            order.PaymentStatus = PaymentStatus.PartiallyRefunded;

        await _context.SaveChangesAsync(cancellationToken);
    }

    public byte[] GenerateInvoicePdf(WebOrder order)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("INVOICE").Bold().FontSize(24);
                            c.Item().Text($"Order #{order.OrderNumber}").FontSize(12);
                            c.Item().Text($"Date: {order.OrderDate:MMMM dd, yyyy}");
                        });
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text($"Status: {order.Status}").Bold();
                            c.Item().Text($"Payment: {order.PaymentStatus}");
                        });
                    });
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    // Addresses
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Ship To:").Bold();
                            c.Item().Text($"{order.ShippingFirstName} {order.ShippingLastName}");
                            if (!string.IsNullOrEmpty(order.ShippingCompany))
                                c.Item().Text(order.ShippingCompany);
                            c.Item().Text(order.ShippingAddress1);
                            if (!string.IsNullOrEmpty(order.ShippingAddress2))
                                c.Item().Text(order.ShippingAddress2);
                            c.Item().Text($"{order.ShippingCity}, {order.ShippingState} {order.ShippingPostalCode}");
                            c.Item().Text(order.ShippingCountry);
                        });
                        row.RelativeItem().Column(c =>
                        {
                            c.Item().Text("Bill To:").Bold();
                            c.Item().Text($"{order.BillingFirstName} {order.BillingLastName}");
                            if (!string.IsNullOrEmpty(order.BillingCompany))
                                c.Item().Text(order.BillingCompany);
                            c.Item().Text(order.BillingAddress1);
                            if (!string.IsNullOrEmpty(order.BillingAddress2))
                                c.Item().Text(order.BillingAddress2);
                            c.Item().Text($"{order.BillingCity}, {order.BillingState} {order.BillingPostalCode}");
                            c.Item().Text(order.BillingCountry);
                        });
                    });

                    col.Item().PaddingVertical(15);

                    // Items table
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Item").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Qty").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Price").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(5).AlignRight().Text("Total").Bold();
                        });

                        foreach (var item in order.Items)
                        {
                            table.Cell().Padding(5).Column(c =>
                            {
                                c.Item().Text(item.ProductName);
                                if (!string.IsNullOrEmpty(item.VariantName))
                                    c.Item().Text(item.VariantName).FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"SKU: {item.Sku}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                            table.Cell().Padding(5).AlignRight().Text(item.Quantity.ToString());
                            table.Cell().Padding(5).AlignRight().Text($"{order.Currency} {item.UnitPrice:N2}");
                            table.Cell().Padding(5).AlignRight().Text($"{order.Currency} {item.LineTotal:N2}");
                        }
                    });

                    col.Item().PaddingVertical(10);

                    // Totals
                    col.Item().AlignRight().Width(200).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Padding(3).Text("Subtotal:");
                        table.Cell().Padding(3).AlignRight().Text($"{order.Currency} {order.Subtotal:N2}");

                        if (order.DiscountAmount > 0)
                        {
                            table.Cell().Padding(3).Text($"Discount ({order.CouponCode}):");
                            table.Cell().Padding(3).AlignRight().Text($"-{order.Currency} {order.DiscountAmount:N2}");
                        }

                        table.Cell().Padding(3).Text($"Tax ({order.TaxRate}%):");
                        table.Cell().Padding(3).AlignRight().Text($"{order.Currency} {order.TaxAmount:N2}");

                        table.Cell().Padding(3).Text($"Shipping ({order.ShippingMethod}):");
                        table.Cell().Padding(3).AlignRight().Text($"{order.Currency} {order.ShippingAmount:N2}");

                        table.Cell().Padding(3).BorderTop(1).Text("Total:").Bold();
                        table.Cell().Padding(3).BorderTop(1).AlignRight().Text($"{order.Currency} {order.Total:N2}").Bold();
                    });
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("Generated on ").FontSize(8);
                    text.Span(DateTime.Now.ToString("MMMM dd, yyyy HH:mm")).FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }

    public byte[] GeneratePackingSlipPdf(WebOrder order)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().Text("PACKING SLIP").Bold().FontSize(24);
                    col.Item().Text($"Order #{order.OrderNumber}").FontSize(14);
                    col.Item().PaddingVertical(10).LineHorizontal(1);
                });

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Text("Ship To:").Bold();
                    col.Item().Text($"{order.ShippingFirstName} {order.ShippingLastName}").FontSize(14);
                    if (!string.IsNullOrEmpty(order.ShippingCompany))
                        col.Item().Text(order.ShippingCompany);
                    col.Item().Text(order.ShippingAddress1);
                    if (!string.IsNullOrEmpty(order.ShippingAddress2))
                        col.Item().Text(order.ShippingAddress2);
                    col.Item().Text($"{order.ShippingCity}, {order.ShippingState} {order.ShippingPostalCode}");
                    col.Item().Text(order.ShippingCountry);
                    if (!string.IsNullOrEmpty(order.ShippingPhone))
                        col.Item().Text($"Phone: {order.ShippingPhone}");

                    col.Item().PaddingVertical(20);

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(1);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(8).Text("Item").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text("Qty").Bold();
                            header.Cell().Background(Colors.Grey.Lighten3).Padding(8).AlignCenter().Text("Packed").Bold();
                        });

                        foreach (var item in order.Items)
                        {
                            table.Cell().Padding(8).Column(c =>
                            {
                                c.Item().Text(item.ProductName).Bold();
                                if (!string.IsNullOrEmpty(item.VariantName))
                                    c.Item().Text(item.VariantName);
                                c.Item().Text($"SKU: {item.Sku}").FontSize(9).FontColor(Colors.Grey.Darken1);
                            });
                            table.Cell().Padding(8).AlignCenter().Text(item.Quantity.ToString()).FontSize(14);
                            table.Cell().Padding(8).AlignCenter().Text("☐").FontSize(18);
                        }
                    });
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1);
                    col.Item().PaddingVertical(5).Row(row =>
                    {
                        row.RelativeItem().Text($"Total Items: {order.Items.Sum(i => i.Quantity)}").Bold();
                        row.RelativeItem().AlignRight().Text($"Order Date: {order.OrderDate:MMM dd, yyyy}");
                    });
                });
            });
        });

        return document.GeneratePdf();
    }

    public async Task<OrderStatistics> GetOrderStatisticsAsync(DateTime? fromDate = null, DateTime? toDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.WebOrders.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        var orders = await query.ToListAsync(cancellationToken);

        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var paidOrders = orders.Where(o => o.PaymentStatus == PaymentStatus.Paid || o.PaymentStatus == PaymentStatus.PartiallyRefunded).ToList();

        return new OrderStatistics
        {
            TotalOrders = orders.Count,
            TotalRevenue = paidOrders.Sum(o => o.Total - o.RefundedAmount),
            AverageOrderValue = paidOrders.Count > 0 ? paidOrders.Average(o => o.Total) : 0,
            TodayOrders = orders.Count(o => o.OrderDate.Date == today),
            TodayRevenue = paidOrders.Where(o => o.OrderDate.Date == today).Sum(o => o.Total - o.RefundedAmount),
            ThisWeekOrders = orders.Count(o => o.OrderDate.Date >= startOfWeek),
            ThisWeekRevenue = paidOrders.Where(o => o.OrderDate.Date >= startOfWeek).Sum(o => o.Total - o.RefundedAmount),
            ThisMonthOrders = orders.Count(o => o.OrderDate.Date >= startOfMonth),
            ThisMonthRevenue = paidOrders.Where(o => o.OrderDate.Date >= startOfMonth).Sum(o => o.Total - o.RefundedAmount),
            ByStatus = orders.GroupBy(o => o.Status).ToDictionary(g => g.Key, g => g.Count()),
            ByPaymentStatus = orders.GroupBy(o => o.PaymentStatus).ToDictionary(g => g.Key, g => g.Count()),
            DailyTrend = orders
                .GroupBy(o => o.OrderDate.Date)
                .OrderBy(g => g.Key)
                .TakeLast(30)
                .Select(g => new DailyOrderSummary
                {
                    Date = g.Key,
                    OrderCount = g.Count(),
                    Revenue = g.Where(o => o.PaymentStatus == PaymentStatus.Paid).Sum(o => o.Total - o.RefundedAmount)
                })
                .ToList()
        };
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = _dateTime.UtcNow.Year;
        var count = await _context.WebOrders
            .Where(o => o.OrderDate.Year == year)
            .CountAsync(cancellationToken) + 1;

        return $"ORD-{year}-{count:D6}";
    }
}
