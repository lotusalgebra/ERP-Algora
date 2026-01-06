using Algora.Erp.Domain.Entities.Ecommerce;

namespace Algora.Erp.Application.Common.Interfaces.Ecommerce;

/// <summary>
/// Service for managing eCommerce product catalog
/// </summary>
public interface IProductCatalogService
{
    // Categories
    Task<List<WebCategory>> GetCategoriesAsync(bool includeProducts = false, CancellationToken cancellationToken = default);
    Task<WebCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<WebCategory?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<WebCategory> SaveCategoryAsync(CategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    // Products
    Task<ProductListResult> GetProductsAsync(ProductListRequest request, CancellationToken cancellationToken = default);
    Task<EcommerceProduct?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EcommerceProduct?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<EcommerceProduct> SaveProductAsync(ProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
    Task UpdateProductStatusAsync(Guid id, ProductStatus status, CancellationToken cancellationToken = default);

    // Product Images
    Task<ProductImage> AddProductImageAsync(Guid productId, ProductImageDto dto, CancellationToken cancellationToken = default);
    Task DeleteProductImageAsync(Guid imageId, CancellationToken cancellationToken = default);
    Task ReorderProductImagesAsync(Guid productId, List<Guid> imageIds, CancellationToken cancellationToken = default);

    // Product Variants
    Task<ProductVariant> SaveVariantAsync(Guid productId, VariantDto dto, CancellationToken cancellationToken = default);
    Task DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default);
    Task UpdateVariantStockAsync(Guid variantId, int quantity, CancellationToken cancellationToken = default);

    // Reviews
    Task<ProductReviewListResult> GetReviewsAsync(ProductReviewListRequest request, CancellationToken cancellationToken = default);
    Task<ProductReview> CreateReviewAsync(CreateReviewDto dto, CancellationToken cancellationToken = default);
    Task ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);

    // Inventory
    Task<List<LowStockProduct>> GetLowStockProductsAsync(CancellationToken cancellationToken = default);
    Task UpdateStockAsync(Guid productId, int quantity, string? reason = null, CancellationToken cancellationToken = default);
}

public class CategoryDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
}

public class ProductDto
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public Guid? InventoryProductId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Brand { get; set; }
    public string? Vendor { get; set; }
    public decimal? Weight { get; set; }
    public string? WeightUnit { get; set; }
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsBestSeller { get; set; }
    public bool TrackInventory { get; set; } = true;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; } = 5;
    public bool AllowBackorder { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }
}

public class ProductImageDto
{
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class VariantDto
{
    public Guid? Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Option1Name { get; set; }
    public string? Option1Value { get; set; }
    public string? Option2Name { get; set; }
    public string? Option2Value { get; set; }
    public string? Option3Name { get; set; }
    public string? Option3Value { get; set; }
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public int StockQuantity { get; set; }
    public decimal? Weight { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public ProductStatus? Status { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsNewArrival { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class ProductListResult
{
    public List<ProductListItem> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ProductListItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? CompareAtPrice { get; set; }
    public string? ImageUrl { get; set; }
    public ProductStatus Status { get; set; }
    public int StockQuantity { get; set; }
    public bool IsFeatured { get; set; }
    public string? CategoryName { get; set; }
    public int VariantCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ProductReviewListRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ProductId { get; set; }
    public int? Rating { get; set; }
    public bool? IsApproved { get; set; }
    public string SortBy { get; set; } = "CreatedAt";
    public bool SortDescending { get; set; } = true;
}

public class ProductReviewListResult
{
    public List<ProductReviewItem> Reviews { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

public class ProductReviewItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public int HelpfulVotes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public Guid ProductId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OrderId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerEmail { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}

public class LowStockProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public int LowStockThreshold { get; set; }
    public string? ImageUrl { get; set; }
}
