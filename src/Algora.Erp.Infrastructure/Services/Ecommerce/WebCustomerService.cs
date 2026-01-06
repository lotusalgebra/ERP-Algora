using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Algora.Erp.Infrastructure.Services.Ecommerce;

/// <summary>
/// Service for managing eCommerce customers
/// </summary>
public class WebCustomerService : IWebCustomerService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public WebCustomerService(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    public async Task<WebCustomer> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        // Check if email already exists
        var exists = await _context.WebCustomers.AnyAsync(c => c.Email == dto.Email.ToLower(), cancellationToken);
        if (exists)
            throw new InvalidOperationException($"A customer with email '{dto.Email}' already exists.");

        var customer = new WebCustomer
        {
            Email = dto.Email.ToLower(),
            PasswordHash = HashPassword(dto.Password),
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            DateOfBirth = dto.DateOfBirth,
            AcceptsMarketing = dto.AcceptsMarketing,
            IsActive = true
        };

        _context.WebCustomers.Add(customer);
        await _context.SaveChangesAsync(cancellationToken);

        return customer;
    }

    public async Task<WebCustomer?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.WebCustomers
            .Include(c => c.Addresses)
            .Include(c => c.Wishlist)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken);
    }

    public async Task<WebCustomer?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _context.WebCustomers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Email == email.ToLower(), cancellationToken);
    }

    public async Task<CustomerListResult> GetCustomersAsync(CustomerListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.WebCustomers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.Email.ToLower().Contains(term) ||
                c.FirstName.ToLower().Contains(term) ||
                c.LastName.ToLower().Contains(term) ||
                (c.Phone != null && c.Phone.Contains(term)));
        }

        if (request.IsActive.HasValue)
            query = query.Where(c => c.IsActive == request.IsActive.Value);

        if (request.AcceptsMarketing.HasValue)
            query = query.Where(c => c.AcceptsMarketing == request.AcceptsMarketing.Value);

        if (request.RegisteredFrom.HasValue)
            query = query.Where(c => c.CreatedAt >= request.RegisteredFrom.Value);

        if (request.RegisteredTo.HasValue)
            query = query.Where(c => c.CreatedAt <= request.RegisteredTo.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy.ToLower() switch
        {
            "email" => request.SortDescending ? query.OrderByDescending(c => c.Email) : query.OrderBy(c => c.Email),
            "name" => request.SortDescending
                ? query.OrderByDescending(c => c.FirstName).ThenByDescending(c => c.LastName)
                : query.OrderBy(c => c.FirstName).ThenBy(c => c.LastName),
            "ordercount" => request.SortDescending ? query.OrderByDescending(c => c.OrderCount) : query.OrderBy(c => c.OrderCount),
            "totalspent" => request.SortDescending ? query.OrderByDescending(c => c.TotalSpent) : query.OrderBy(c => c.TotalSpent),
            "lastorder" => request.SortDescending ? query.OrderByDescending(c => c.LastOrderAt) : query.OrderBy(c => c.LastOrderAt),
            _ => request.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var customers = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CustomerListItem
            {
                Id = c.Id,
                Email = c.Email,
                FirstName = c.FirstName,
                LastName = c.LastName,
                Phone = c.Phone,
                IsActive = c.IsActive,
                AcceptsMarketing = c.AcceptsMarketing,
                OrderCount = c.OrderCount,
                TotalSpent = c.TotalSpent,
                LastOrderAt = c.LastOrderAt,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new CustomerListResult
        {
            Customers = customers,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<WebCustomer> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await _context.WebCustomers.FindAsync(new object[] { customerId }, cancellationToken)
            ?? throw new InvalidOperationException($"Customer with ID {customerId} not found.");

        customer.FirstName = dto.FirstName;
        customer.LastName = dto.LastName;
        customer.Phone = dto.Phone;
        customer.DateOfBirth = dto.DateOfBirth;
        customer.AcceptsMarketing = dto.AcceptsMarketing;
        customer.AvatarUrl = dto.AvatarUrl;

        await _context.SaveChangesAsync(cancellationToken);
        return customer;
    }

    public async Task<CustomerLoginResult> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var customer = await _context.WebCustomers
            .FirstOrDefaultAsync(c => c.Email == email.ToLower(), cancellationToken);

        if (customer == null)
        {
            return new CustomerLoginResult
            {
                Success = false,
                ErrorMessage = "Invalid email or password."
            };
        }

        if (!customer.IsActive)
        {
            return new CustomerLoginResult
            {
                Success = false,
                ErrorMessage = "Your account has been deactivated. Please contact support."
            };
        }

        if (!VerifyPassword(password, customer.PasswordHash))
        {
            return new CustomerLoginResult
            {
                Success = false,
                ErrorMessage = "Invalid email or password."
            };
        }

        customer.LastLoginAt = _dateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return new CustomerLoginResult
        {
            Success = true,
            Customer = customer
        };
    }

    public async Task ChangePasswordAsync(Guid customerId, string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var customer = await _context.WebCustomers.FindAsync(new object[] { customerId }, cancellationToken)
            ?? throw new InvalidOperationException($"Customer with ID {customerId} not found.");

        if (!VerifyPassword(currentPassword, customer.PasswordHash))
            throw new InvalidOperationException("Current password is incorrect.");

        customer.PasswordHash = HashPassword(newPassword);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> InitiatePasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var customer = await _context.WebCustomers.FirstOrDefaultAsync(c => c.Email == email.ToLower(), cancellationToken)
            ?? throw new InvalidOperationException("No account found with that email address.");

        var token = GenerateResetToken();
        customer.PasswordResetToken = token;
        customer.PasswordResetExpiry = _dateTime.UtcNow.AddHours(24);

        await _context.SaveChangesAsync(cancellationToken);
        return token;
    }

    public async Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default)
    {
        var customer = await _context.WebCustomers
            .FirstOrDefaultAsync(c => c.PasswordResetToken == token, cancellationToken)
            ?? throw new InvalidOperationException("Invalid or expired reset token.");

        if (customer.PasswordResetExpiry < _dateTime.UtcNow)
            throw new InvalidOperationException("Reset token has expired.");

        customer.PasswordHash = HashPassword(newPassword);
        customer.PasswordResetToken = null;
        customer.PasswordResetExpiry = null;

        await _context.SaveChangesAsync(cancellationToken);
    }

    #region Addresses

    public async Task<List<CustomerAddress>> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.CustomerAddresses
            .Where(a => a.CustomerId == customerId)
            .OrderByDescending(a => a.IsDefaultShipping)
            .ThenByDescending(a => a.IsDefaultBilling)
            .ToListAsync(cancellationToken);
    }

    public async Task<CustomerAddress> AddAddressAsync(Guid customerId, AddressDto dto, CancellationToken cancellationToken = default)
    {
        var customer = await _context.WebCustomers
            .Include(c => c.Addresses)
            .FirstOrDefaultAsync(c => c.Id == customerId, cancellationToken)
            ?? throw new InvalidOperationException($"Customer with ID {customerId} not found.");

        // If setting as default, clear other defaults
        if (dto.IsDefaultShipping)
        {
            foreach (var addr in customer.Addresses)
                addr.IsDefaultShipping = false;
        }
        if (dto.IsDefaultBilling)
        {
            foreach (var addr in customer.Addresses)
                addr.IsDefaultBilling = false;
        }

        var address = new CustomerAddress
        {
            CustomerId = customerId,
            Label = dto.Label,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Company = dto.Company,
            Address1 = dto.Address1,
            Address2 = dto.Address2,
            City = dto.City,
            State = dto.State,
            PostalCode = dto.PostalCode,
            Country = dto.Country,
            Phone = dto.Phone,
            IsDefaultShipping = dto.IsDefaultShipping || !customer.Addresses.Any(),
            IsDefaultBilling = dto.IsDefaultBilling || !customer.Addresses.Any()
        };

        _context.CustomerAddresses.Add(address);
        await _context.SaveChangesAsync(cancellationToken);

        return address;
    }

    public async Task<CustomerAddress> UpdateAddressAsync(Guid addressId, AddressDto dto, CancellationToken cancellationToken = default)
    {
        var address = await _context.CustomerAddresses.FindAsync(new object[] { addressId }, cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {addressId} not found.");

        // If setting as default, clear other defaults
        if (dto.IsDefaultShipping && !address.IsDefaultShipping)
        {
            var otherAddresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == address.CustomerId && a.Id != addressId)
                .ToListAsync(cancellationToken);
            foreach (var addr in otherAddresses)
                addr.IsDefaultShipping = false;
        }
        if (dto.IsDefaultBilling && !address.IsDefaultBilling)
        {
            var otherAddresses = await _context.CustomerAddresses
                .Where(a => a.CustomerId == address.CustomerId && a.Id != addressId)
                .ToListAsync(cancellationToken);
            foreach (var addr in otherAddresses)
                addr.IsDefaultBilling = false;
        }

        address.Label = dto.Label;
        address.FirstName = dto.FirstName;
        address.LastName = dto.LastName;
        address.Company = dto.Company;
        address.Address1 = dto.Address1;
        address.Address2 = dto.Address2;
        address.City = dto.City;
        address.State = dto.State;
        address.PostalCode = dto.PostalCode;
        address.Country = dto.Country;
        address.Phone = dto.Phone;
        address.IsDefaultShipping = dto.IsDefaultShipping;
        address.IsDefaultBilling = dto.IsDefaultBilling;

        await _context.SaveChangesAsync(cancellationToken);
        return address;
    }

    public async Task DeleteAddressAsync(Guid addressId, CancellationToken cancellationToken = default)
    {
        var address = await _context.CustomerAddresses.FindAsync(new object[] { addressId }, cancellationToken)
            ?? throw new InvalidOperationException($"Address with ID {addressId} not found.");

        _context.CustomerAddresses.Remove(address);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SetDefaultAddressAsync(Guid customerId, Guid addressId, AddressType addressType, CancellationToken cancellationToken = default)
    {
        var addresses = await _context.CustomerAddresses
            .Where(a => a.CustomerId == customerId)
            .ToListAsync(cancellationToken);

        var targetAddress = addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new InvalidOperationException($"Address with ID {addressId} not found.");

        foreach (var addr in addresses)
        {
            if (addressType == AddressType.Shipping)
                addr.IsDefaultShipping = addr.Id == addressId;
            else
                addr.IsDefaultBilling = addr.Id == addressId;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Wishlist

    public async Task<List<WishlistItemInfo>> GetWishlistAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .Include(w => w.Product)
                .ThenInclude(p => p.Images.OrderBy(i => i.SortOrder).Take(1))
            .Include(w => w.Variant)
            .Where(w => w.CustomerId == customerId)
            .OrderByDescending(w => w.AddedAt)
            .Select(w => new WishlistItemInfo
            {
                Id = w.Id,
                ProductId = w.ProductId,
                VariantId = w.VariantId,
                ProductName = w.Product.Name,
                VariantName = w.Variant != null ? w.Variant.Name : null,
                ImageUrl = w.Variant != null && w.Variant.ImageUrl != null
                    ? w.Variant.ImageUrl
                    : w.Product.Images.FirstOrDefault() != null ? w.Product.Images.First().Url : null,
                Price = w.Variant != null ? w.Variant.Price : w.Product.Price,
                CompareAtPrice = w.Variant != null ? w.Variant.CompareAtPrice : w.Product.CompareAtPrice,
                InStock = w.Variant != null
                    ? w.Variant.StockQuantity > 0
                    : w.Product.StockQuantity > 0 || w.Product.AllowBackorder,
                AddedAt = w.AddedAt
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WishlistItem> AddToWishlistAsync(Guid customerId, Guid productId, Guid? variantId = null, CancellationToken cancellationToken = default)
    {
        // Check if already in wishlist
        var exists = await _context.WishlistItems
            .AnyAsync(w => w.CustomerId == customerId && w.ProductId == productId && w.VariantId == variantId, cancellationToken);

        if (exists)
            throw new InvalidOperationException("Item is already in your wishlist.");

        var item = new WishlistItem
        {
            CustomerId = customerId,
            ProductId = productId,
            VariantId = variantId,
            AddedAt = _dateTime.UtcNow
        };

        _context.WishlistItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return item;
    }

    public async Task RemoveFromWishlistAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default)
    {
        var item = await _context.WishlistItems
            .FirstOrDefaultAsync(w => w.CustomerId == customerId && w.ProductId == productId, cancellationToken);

        if (item != null)
        {
            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<bool> IsInWishlistAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default)
    {
        return await _context.WishlistItems
            .AnyAsync(w => w.CustomerId == customerId && w.ProductId == productId, cancellationToken);
    }

    #endregion

    #region Statistics

    public async Task<CustomerStatistics> GetCustomerStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var customers = await _context.WebCustomers.ToListAsync(cancellationToken);

        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var activeCustomers = customers.Where(c => c.IsActive && c.OrderCount > 0).ToList();

        return new CustomerStatistics
        {
            TotalCustomers = customers.Count,
            ActiveCustomers = activeCustomers.Count,
            NewCustomersToday = customers.Count(c => c.CreatedAt.Date == today),
            NewCustomersThisWeek = customers.Count(c => c.CreatedAt.Date >= startOfWeek),
            NewCustomersThisMonth = customers.Count(c => c.CreatedAt.Date >= startOfMonth),
            MarketingOptIns = customers.Count(c => c.AcceptsMarketing),
            AverageLifetimeValue = activeCustomers.Count > 0 ? activeCustomers.Average(c => c.TotalSpent) : 0,
            DailyTrend = customers
                .GroupBy(c => c.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .TakeLast(30)
                .Select(g => new DailyCustomerSummary
                {
                    Date = g.Key,
                    NewCustomers = g.Count(),
                    ReturningCustomers = g.Count(c => c.OrderCount > 1)
                })
                .ToList()
        };
    }

    public async Task<decimal> GetCustomerLifetimeValueAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _context.WebCustomers.FindAsync(new object[] { customerId }, cancellationToken);
        return customer?.TotalSpent ?? 0;
    }

    #endregion

    #region Password Helpers

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salt = Guid.NewGuid().ToString("N");
        var hash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));
        return $"{salt}:{hash}";
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;

        var salt = parts[0];
        var hash = parts[1];

        using var sha256 = SHA256.Create();
        var computedHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password + salt)));

        return hash == computedHash;
    }

    private static string GenerateResetToken()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
    }

    #endregion
}
