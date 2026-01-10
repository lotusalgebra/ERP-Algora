using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Application.Common.Interfaces.Ecommerce;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Ecommerce.Products;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IProductCatalogService _productService;

    public IndexModel(IApplicationDbContext context, IProductCatalogService productService)
    {
        _context = context;
        _productService = productService;
    }

    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int FeaturedProducts { get; set; }
    public int LowStockProducts { get; set; }
    public List<WebCategory> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalProducts = await _context.EcommerceProducts.CountAsync();
        ActiveProducts = await _context.EcommerceProducts.CountAsync(p => p.Status == ProductStatus.Active);
        FeaturedProducts = await _context.EcommerceProducts.CountAsync(p => p.IsFeatured);
        LowStockProducts = await _context.EcommerceProducts
            .CountAsync(p => p.TrackInventory && p.StockQuantity <= p.LowStockThreshold);
        Categories = await _context.WebCategories.Where(c => c.IsActive && !c.IsDeleted).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? categoryFilter, string? statusFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.EcommerceProducts
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder).Take(1))
            .Where(p => !p.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Sku.ToLower().Contains(search) ||
                (p.Brand != null && p.Brand.ToLower().Contains(search)));
        }

        if (categoryFilter.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ProductStatus>(statusFilter, out var status))
        {
            query = query.Where(p => p.Status == status);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_ProductsTableRows", new EcommerceProductsTableViewModel
        {
            Products = products,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var categories = await _context.WebCategories.Where(c => c.IsActive && !c.IsDeleted).ToListAsync();

        return Partial("_ProductForm", new EcommerceProductFormViewModel
        {
            IsEdit = false,
            Categories = categories
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var product = await _context.EcommerceProducts
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.SortOrder))
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        var categories = await _context.WebCategories.Where(c => c.IsActive && !c.IsDeleted).ToListAsync();

        return Partial("_ProductForm", new EcommerceProductFormViewModel
        {
            IsEdit = true,
            Product = product,
            Categories = categories
        });
    }

    public async Task<IActionResult> OnPostAsync(EcommerceProductFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        EcommerceProduct? product;

        if (input.Id.HasValue)
        {
            product = await _context.EcommerceProducts.FindAsync(input.Id.Value);
            if (product == null)
                return NotFound();
        }
        else
        {
            product = new EcommerceProduct
            {
                Id = Guid.NewGuid()
            };
            _context.EcommerceProducts.Add(product);
        }

        product.Name = input.Name;
        product.Slug = input.Slug ?? GenerateSlug(input.Name);
        product.Sku = input.Sku;
        product.ShortDescription = input.ShortDescription;
        product.Description = input.Description;
        product.Price = input.Price;
        product.CompareAtPrice = input.CompareAtPrice;
        product.CostPrice = input.CostPrice;
        product.CategoryId = input.CategoryId;
        product.Brand = input.Brand;
        product.Vendor = input.Vendor;
        product.Weight = input.Weight;
        product.WeightUnit = input.WeightUnit;
        product.Status = input.Status;
        product.IsFeatured = input.IsFeatured;
        product.IsNewArrival = input.IsNewArrival;
        product.IsBestSeller = input.IsBestSeller;
        product.TrackInventory = input.TrackInventory;
        product.StockQuantity = input.StockQuantity;
        product.LowStockThreshold = input.LowStockThreshold;
        product.AllowBackorder = input.AllowBackorder;
        product.MetaTitle = input.MetaTitle;
        product.MetaDescription = input.MetaDescription;
        product.Tags = input.Tags;

        // Ensure unique slug
        var slugExists = await _context.EcommerceProducts
            .AnyAsync(p => p.Slug == product.Slug && p.Id != product.Id);
        if (slugExists)
            product.Slug = $"{product.Slug}-{Guid.NewGuid().ToString("N")[..6]}";

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var product = await _context.EcommerceProducts.FindAsync(id);
        if (product == null)
            return NotFound();

        product.IsDeleted = true;
        product.Status = ProductStatus.Archived;
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "");

        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
        slug = slug.Trim('-');

        return slug;
    }
}

public class EcommerceProductsTableViewModel
{
    public List<EcommerceProduct> Products { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Ecommerce/Products",
        Handler = "Table",
        HxTarget = "#productsTableBody",
        HxInclude = "#searchInput,#categoryFilter,#statusFilter"
    };
}

public class EcommerceProductFormViewModel
{
    public bool IsEdit { get; set; }
    public EcommerceProduct? Product { get; set; }
    public List<WebCategory> Categories { get; set; } = new();
}

public class EcommerceProductFormInput
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
