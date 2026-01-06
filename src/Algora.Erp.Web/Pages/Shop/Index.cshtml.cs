using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop;

public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private const int PageSize = 12;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public List<EcommerceProduct> Products { get; set; } = new();
    public List<EcommerceProduct> FeaturedProducts { get; set; } = new();
    public List<WebCategory> Categories { get; set; } = new();
    public string? CategorySlug { get; set; }
    public string SortBy { get; set; } = "newest";
    public new int Page { get; set; } = 1;
    public int TotalProducts { get; set; }
    public int TotalPages { get; set; }

    public async Task OnGetAsync(string? category, string? sort, int page = 1, string? search = null)
    {
        CategorySlug = category;
        SortBy = sort ?? "newest";
        Page = page;

        // Get categories for navigation
        Categories = await _context.WebCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        // Build products query
        var query = _context.EcommerceProducts
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.Status == ProductStatus.Active && p.StockQuantity > 0)
            .AsQueryable();

        // Filter by category
        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(p => p.Category != null && p.Category.Slug == category);
        }

        // Search
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(searchLower) ||
                (p.Description != null && p.Description.ToLower().Contains(searchLower)));
        }

        // Get total count
        TotalProducts = await query.CountAsync();
        TotalPages = (int)Math.Ceiling(TotalProducts / (double)PageSize);

        // Apply sorting
        query = SortBy switch
        {
            "price-asc" => query.OrderBy(p => p.Price),
            "price-desc" => query.OrderByDescending(p => p.Price),
            "name" => query.OrderBy(p => p.Name),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        // Paginate
        Products = await query
            .Skip((Page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        // Get featured products (only on main page)
        if (string.IsNullOrEmpty(category))
        {
            FeaturedProducts = await _context.EcommerceProducts
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Active && p.IsFeatured && p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();
        }
    }
}
