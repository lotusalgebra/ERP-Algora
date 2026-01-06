using Algora.Erp.Domain.Entities.Ecommerce;

namespace Algora.Erp.Application.Common.Interfaces.Ecommerce;

/// <summary>
/// Service for managing eCommerce customers
/// </summary>
public interface IWebCustomerService
{
    /// <summary>
    /// Creates a new customer
    /// </summary>
    Task<WebCustomer> CreateCustomerAsync(CreateCustomerDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    Task<WebCustomer?> GetCustomerByIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by email
    /// </summary>
    Task<WebCustomer?> GetCustomerByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists customers with filtering and pagination
    /// </summary>
    Task<CustomerListResult> GetCustomersAsync(CustomerListRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates customer profile
    /// </summary>
    Task<WebCustomer> UpdateCustomerAsync(Guid customerId, UpdateCustomerDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates customer credentials
    /// </summary>
    Task<CustomerLoginResult> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes customer password
    /// </summary>
    Task ChangePasswordAsync(Guid customerId, string currentPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Initiates password reset
    /// </summary>
    Task<string> InitiatePasswordResetAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes password reset
    /// </summary>
    Task ResetPasswordAsync(string token, string newPassword, CancellationToken cancellationToken = default);

    // Addresses
    /// <summary>
    /// Gets customer addresses
    /// </summary>
    Task<List<CustomerAddress>> GetCustomerAddressesAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a customer address
    /// </summary>
    Task<CustomerAddress> AddAddressAsync(Guid customerId, AddressDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a customer address
    /// </summary>
    Task<CustomerAddress> UpdateAddressAsync(Guid addressId, AddressDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a customer address
    /// </summary>
    Task DeleteAddressAsync(Guid addressId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets default address
    /// </summary>
    Task SetDefaultAddressAsync(Guid customerId, Guid addressId, AddressType addressType, CancellationToken cancellationToken = default);

    // Wishlist
    /// <summary>
    /// Gets customer wishlist
    /// </summary>
    Task<List<WishlistItemInfo>> GetWishlistAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds item to wishlist
    /// </summary>
    Task<WishlistItem> AddToWishlistAsync(Guid customerId, Guid productId, Guid? variantId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes item from wishlist
    /// </summary>
    Task RemoveFromWishlistAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if product is in wishlist
    /// </summary>
    Task<bool> IsInWishlistAsync(Guid customerId, Guid productId, CancellationToken cancellationToken = default);

    // Statistics
    /// <summary>
    /// Gets customer statistics for dashboard
    /// </summary>
    Task<CustomerStatistics> GetCustomerStatisticsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customer lifetime value
    /// </summary>
    Task<decimal> GetCustomerLifetimeValueAsync(Guid customerId, CancellationToken cancellationToken = default);
}

public class CreateCustomerDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool AcceptsMarketing { get; set; }
}

public class UpdateCustomerDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public bool AcceptsMarketing { get; set; }
    public string? AvatarUrl { get; set; }
}

public class CustomerLoginResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public WebCustomer? Customer { get; set; }
}

public class AddressDto
{
    public string Label { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string City { get; set; } = string.Empty;
    public string? State { get; set; }
    public string PostalCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public bool IsDefaultShipping { get; set; }
    public bool IsDefaultBilling { get; set; }
}

public enum AddressType
{
    Shipping,
    Billing
}

public class CustomerListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public bool? IsActive { get; set; }
    public bool? AcceptsMarketing { get; set; }
    public DateTime? RegisteredFrom { get; set; }
    public DateTime? RegisteredTo { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class CustomerListResult
{
    public List<CustomerListItem> Customers { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class CustomerListItem
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string? Phone { get; set; }
    public bool IsActive { get; set; }
    public bool AcceptsMarketing { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastOrderAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class WishlistItemInfo
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? VariantName { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public bool InStock { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CustomerStatistics
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int NewCustomersToday { get; set; }
    public int NewCustomersThisWeek { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int MarketingOptIns { get; set; }
    public decimal AverageLifetimeValue { get; set; }
    public List<DailyCustomerSummary> DailyTrend { get; set; } = new();
}

public class DailyCustomerSummary
{
    public DateTime Date { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
}
