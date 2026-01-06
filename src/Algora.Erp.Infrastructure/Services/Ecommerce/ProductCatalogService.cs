using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Services.Ecommerce;

/// <summary>
/// Service for managing eCommerce product catalog
/// </summary>
public class ProductCatalogService : IProductCatalogService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;

    public ProductCatalogService(IApplicationDbContext context, IDateTime dateTime)
    {
        _context = context;
        _dateTime = dateTime;
    }

    #region Categories

    public async Task<List<WebCategory>> GetCategoriesAsync(bool includeProducts = false, CancellationToken cancellationToken = default)
    {
        var query = _context.WebCategories.AsQueryable();

        if (includeProducts)
            query = query.Include(c => c.Products);

        return await query
            .Include(c => c.Children)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<WebCategory?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.WebCategories
            .Include(c => c.Children)
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<WebCategory?> GetCategoryBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.WebCategories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Slug == slug, cancellationToken);
    }

    public async Task<WebCategory> SaveCategoryAsync(CategoryDto dto, CancellationToken cancellationToken = default)
    {
        WebCategory category;

        if (dto.Id.HasValue)
        {
            category = await _context.WebCategories.FindAsync(new object[] { dto.Id.Value }, cancellationToken)
                ?? throw new InvalidOperationException($"Category with ID {dto.Id} not found.");
        }
        else
        {
            category = new WebCategory();
            _context.WebCategories.Add(category);
        }

        category.Name = dto.Name;
        category.Slug = dto.Slug ?? GenerateSlug(dto.Name);
        category.Description = dto.Description;
        category.ImageUrl = dto.ImageUrl;
        category.ParentId = dto.ParentId;
        category.IsActive = dto.IsActive;
        category.SortOrder = dto.SortOrder;
        category.MetaTitle = dto.MetaTitle;
        category.MetaDescription = dto.MetaDescription;

        await _context.SaveChangesAsync(cancellationToken);
        return category;
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _context.WebCategories
            .Include(c => c.Children)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"Category with ID {id} not found.");

        if (category.Children.Any())
            throw new InvalidOperationException("Cannot delete category with child categories.");

        category.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Products

    public async Task<ProductListResult> GetProductsAsync(ProductListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.EcommerceProducts
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Category)
            .Include(p => p.Variants)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var term = request.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                p.Sku.ToLower().Contains(term) ||
                (p.Brand != null && p.Brand.ToLower().Contains(term)) ||
                (p.Tags != null && p.Tags.ToLower().Contains(term)));
        }

        if (request.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.IsFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);

        if (request.IsNewArrival.HasValue)
            query = query.Where(p => p.IsNewArrival == request.IsNewArrival.Value);

        if (request.MinPrice.HasValue)
            query = query.Where(p => p.Price >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= request.MaxPrice.Value);

        if (request.InStock.HasValue)
        {
            if (request.InStock.Value)
                query = query.Where(p => p.StockQuantity > 0 || p.AllowBackorder);
            else
                query = query.Where(p => p.StockQuantity <= 0 && !p.AllowBackorder);
        }

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy.ToLower() switch
        {
            "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => request.SortDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "sku" => request.SortDescending ? query.OrderByDescending(p => p.Sku) : query.OrderBy(p => p.Sku),
            "stock" => request.SortDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
            "rating" => request.SortDescending ? query.OrderByDescending(p => p.AverageRating) : query.OrderBy(p => p.AverageRating),
            _ => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
        };

        // Apply pagination
        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListItem
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug,
                Sku = p.Sku,
                Price = p.Price,
                CompareAtPrice = p.CompareAtPrice,
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary) != null
                    ? p.Images.FirstOrDefault(i => i.IsPrimary)!.Url
                    : p.Images.FirstOrDefault() != null ? p.Images.First().Url : null,
                Status = p.Status,
                StockQuantity = p.StockQuantity,
                IsFeatured = p.IsFeatured,
                CategoryName = p.Category != null ? p.Category.Name : null,
                VariantCount = p.Variants.Count,
                AverageRating = p.AverageRating,
                ReviewCount = p.ReviewCount,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new ProductListResult
        {
            Products = products,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<EcommerceProduct?> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.EcommerceProducts
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Variants)
            .Include(p => p.Category)
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved).OrderByDescending(r => r.CreatedAt).Take(10))
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<EcommerceProduct?> GetProductBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.EcommerceProducts
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Variants.Where(v => v.IsActive))
            .Include(p => p.Category)
            .Include(p => p.Reviews.Where(r => r.Status == ReviewStatus.Approved).OrderByDescending(r => r.CreatedAt).Take(10))
            .FirstOrDefaultAsync(p => p.Slug == slug, cancellationToken);
    }

    public async Task<EcommerceProduct> SaveProductAsync(ProductDto dto, CancellationToken cancellationToken = default)
    {
        EcommerceProduct product;

        if (dto.Id.HasValue)
        {
            product = await _context.EcommerceProducts.FindAsync(new object[] { dto.Id.Value }, cancellationToken)
                ?? throw new InvalidOperationException($"Product with ID {dto.Id} not found.");
        }
        else
        {
            product = new EcommerceProduct();
            _context.EcommerceProducts.Add(product);
        }

        product.Name = dto.Name;
        product.Slug = dto.Slug ?? GenerateSlug(dto.Name);
        product.Sku = dto.Sku;
        product.ShortDescription = dto.ShortDescription;
        product.Description = dto.Description;
        product.Price = dto.Price;
        product.CompareAtPrice = dto.CompareAtPrice;
        product.CostPrice = dto.CostPrice;
        product.InventoryProductId = dto.InventoryProductId;
        product.CategoryId = dto.CategoryId;
        product.Brand = dto.Brand;
        product.Vendor = dto.Vendor;
        product.Weight = dto.Weight;
        product.WeightUnit = dto.WeightUnit;
        product.Status = dto.Status;
        product.IsFeatured = dto.IsFeatured;
        product.IsNewArrival = dto.IsNewArrival;
        product.IsBestSeller = dto.IsBestSeller;
        product.TrackInventory = dto.TrackInventory;
        product.StockQuantity = dto.StockQuantity;
        product.LowStockThreshold = dto.LowStockThreshold;
        product.AllowBackorder = dto.AllowBackorder;
        product.MetaTitle = dto.MetaTitle;
        product.MetaDescription = dto.MetaDescription;
        product.Tags = dto.Tags;

        // Ensure unique slug
        var slugExists = await _context.EcommerceProducts
            .AnyAsync(p => p.Slug == product.Slug && p.Id != product.Id, cancellationToken);
        if (slugExists)
            product.Slug = $"{product.Slug}-{Guid.NewGuid().ToString("N")[..6]}";

        await _context.SaveChangesAsync(cancellationToken);
        return product;
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _context.EcommerceProducts.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {id} not found.");

        product.IsDeleted = true;
        product.Status = ProductStatus.Archived;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateProductStatusAsync(Guid id, ProductStatus status, CancellationToken cancellationToken = default)
    {
        var product = await _context.EcommerceProducts.FindAsync(new object[] { id }, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {id} not found.");

        product.Status = status;
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Product Images

    public async Task<ProductImage> AddProductImageAsync(Guid productId, ProductImageDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _context.EcommerceProducts
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

        // If this is marked as primary, unmark others
        if (dto.IsPrimary)
        {
            foreach (var img in product.Images)
                img.IsPrimary = false;
        }

        var image = new ProductImage
        {
            ProductId = productId,
            Url = dto.Url,
            AltText = dto.AltText,
            SortOrder = dto.SortOrder > 0 ? dto.SortOrder : product.Images.Count,
            IsPrimary = dto.IsPrimary || !product.Images.Any()
        };

        _context.ProductImages.Add(image);
        await _context.SaveChangesAsync(cancellationToken);
        return image;
    }

    public async Task DeleteProductImageAsync(Guid imageId, CancellationToken cancellationToken = default)
    {
        var image = await _context.ProductImages.FindAsync(new object[] { imageId }, cancellationToken)
            ?? throw new InvalidOperationException($"Image with ID {imageId} not found.");

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ReorderProductImagesAsync(Guid productId, List<Guid> imageIds, CancellationToken cancellationToken = default)
    {
        var images = await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .ToListAsync(cancellationToken);

        for (int i = 0; i < imageIds.Count; i++)
        {
            var image = images.FirstOrDefault(img => img.Id == imageIds[i]);
            if (image != null)
                image.SortOrder = i;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Product Variants

    public async Task<ProductVariant> SaveVariantAsync(Guid productId, VariantDto dto, CancellationToken cancellationToken = default)
    {
        ProductVariant variant;

        if (dto.Id.HasValue)
        {
            variant = await _context.ProductVariants.FindAsync(new object[] { dto.Id.Value }, cancellationToken)
                ?? throw new InvalidOperationException($"Variant with ID {dto.Id} not found.");
        }
        else
        {
            variant = new ProductVariant { ProductId = productId };
            _context.ProductVariants.Add(variant);
        }

        variant.Sku = dto.Sku;
        variant.Name = dto.Name;
        variant.Option1Name = dto.Option1Name;
        variant.Option1Value = dto.Option1Value;
        variant.Option2Name = dto.Option2Name;
        variant.Option2Value = dto.Option2Value;
        variant.Option3Name = dto.Option3Name;
        variant.Option3Value = dto.Option3Value;
        variant.Price = dto.Price;
        variant.CompareAtPrice = dto.CompareAtPrice;
        variant.StockQuantity = dto.StockQuantity;
        variant.Weight = dto.Weight;
        variant.ImageUrl = dto.ImageUrl;
        variant.IsActive = dto.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        return variant;
    }

    public async Task DeleteVariantAsync(Guid variantId, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants.FindAsync(new object[] { variantId }, cancellationToken)
            ?? throw new InvalidOperationException($"Variant with ID {variantId} not found.");

        _context.ProductVariants.Remove(variant);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateVariantStockAsync(Guid variantId, int quantity, CancellationToken cancellationToken = default)
    {
        var variant = await _context.ProductVariants.FindAsync(new object[] { variantId }, cancellationToken)
            ?? throw new InvalidOperationException($"Variant with ID {variantId} not found.");

        variant.StockQuantity = quantity;
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Reviews

    public async Task<ProductReviewListResult> GetReviewsAsync(ProductReviewListRequest request, CancellationToken cancellationToken = default)
    {
        var query = _context.ProductReviews
            .Include(r => r.Product)
            .AsQueryable();

        if (request.ProductId.HasValue)
            query = query.Where(r => r.ProductId == request.ProductId.Value);

        if (request.Rating.HasValue)
            query = query.Where(r => r.Rating == request.Rating.Value);

        if (request.IsApproved.HasValue)
            query = query.Where(r => (r.Status == ReviewStatus.Approved) == request.IsApproved.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        query = request.SortBy.ToLower() switch
        {
            "rating" => request.SortDescending ? query.OrderByDescending(r => r.Rating) : query.OrderBy(r => r.Rating),
            "helpful" => request.SortDescending ? query.OrderByDescending(r => r.HelpfulVotes) : query.OrderBy(r => r.HelpfulVotes),
            _ => request.SortDescending ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)
        };

        var reviews = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new ProductReviewItem
            {
                Id = r.Id,
                ProductId = r.ProductId,
                ProductName = r.Product.Name,
                CustomerName = r.CustomerName,
                CustomerEmail = r.CustomerEmail,
                Rating = r.Rating,
                Title = r.Title,
                Comment = r.Content,
                IsApproved = r.Status == ReviewStatus.Approved,
                IsVerifiedPurchase = r.IsVerifiedPurchase,
                HelpfulVotes = r.HelpfulVotes,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return new ProductReviewListResult
        {
            Reviews = reviews,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<ProductReview> CreateReviewAsync(CreateReviewDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _context.EcommerceProducts.FindAsync(new object[] { dto.ProductId }, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {dto.ProductId} not found.");

        var review = new ProductReview
        {
            ProductId = dto.ProductId,
            CustomerId = dto.CustomerId,
            OrderId = dto.OrderId,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Title = dto.Title,
            Content = dto.Comment,
            IsVerifiedPurchase = dto.OrderId.HasValue
        };

        _context.ProductReviews.Add(review);
        await _context.SaveChangesAsync(cancellationToken);

        // Update product rating
        await UpdateProductRatingAsync(dto.ProductId, cancellationToken);

        return review;
    }

    public async Task ApproveReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _context.ProductReviews.FindAsync(new object[] { reviewId }, cancellationToken)
            ?? throw new InvalidOperationException($"Review with ID {reviewId} not found.");

        review.Status = ReviewStatus.Approved;
        await _context.SaveChangesAsync(cancellationToken);

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId, cancellationToken);
    }

    public async Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default)
    {
        var review = await _context.ProductReviews.FindAsync(new object[] { reviewId }, cancellationToken)
            ?? throw new InvalidOperationException($"Review with ID {reviewId} not found.");

        var productId = review.ProductId;
        _context.ProductReviews.Remove(review);
        await _context.SaveChangesAsync(cancellationToken);

        // Update product rating
        await UpdateProductRatingAsync(productId, cancellationToken);
    }

    private async Task UpdateProductRatingAsync(Guid productId, CancellationToken cancellationToken)
    {
        var product = await _context.EcommerceProducts.FindAsync(new object[] { productId }, cancellationToken);
        if (product == null) return;

        var approvedReviews = await _context.ProductReviews
            .Where(r => r.ProductId == productId && r.Status == ReviewStatus.Approved)
            .ToListAsync(cancellationToken);

        product.ReviewCount = approvedReviews.Count;
        product.AverageRating = approvedReviews.Count > 0
            ? (decimal)approvedReviews.Average(r => r.Rating)
            : 0;

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    #region Inventory

    public async Task<List<LowStockProduct>> GetLowStockProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.EcommerceProducts
            .Where(p => p.Status == ProductStatus.Active && p.TrackInventory && p.StockQuantity <= p.LowStockThreshold)
            .OrderBy(p => p.StockQuantity)
            .Select(p => new LowStockProduct
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.Sku,
                StockQuantity = p.StockQuantity,
                LowStockThreshold = p.LowStockThreshold,
                ImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary) != null
                    ? p.Images.FirstOrDefault(i => i.IsPrimary)!.Url
                    : p.Images.FirstOrDefault() != null ? p.Images.First().Url : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateStockAsync(Guid productId, int quantity, string? reason = null, CancellationToken cancellationToken = default)
    {
        var product = await _context.EcommerceProducts.FindAsync(new object[] { productId }, cancellationToken)
            ?? throw new InvalidOperationException($"Product with ID {productId} not found.");

        product.StockQuantity = quantity;
        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "");

        // Remove special characters
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        // Remove multiple dashes
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        // Trim dashes
        slug = slug.Trim('-');

        return slug;
    }
}
