using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Inventory.Products;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public decimal TotalInventoryValue { get; set; }
    public int LowStockProducts { get; set; }

    public List<ProductCategory> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalProducts = await _context.Products.CountAsync();
        ActiveProducts = await _context.Products.CountAsync(p => p.IsActive);

        // Calculate total inventory value
        var productsWithStock = await _context.Products
            .Select(p => new { p.CostPrice, p.Id })
            .ToListAsync();

        var stockLevels = await _context.StockLevels
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, TotalOnHand = g.Sum(s => s.QuantityOnHand) })
            .ToListAsync();

        TotalInventoryValue = productsWithStock
            .Join(stockLevels, p => p.Id, s => s.ProductId, (p, s) => p.CostPrice * s.TotalOnHand)
            .Sum();

        LowStockProducts = await _context.Products
            .Where(p => _context.StockLevels
                .Where(s => s.ProductId == p.Id)
                .Sum(s => s.QuantityOnHand) <= p.ReorderLevel && p.ReorderLevel > 0)
            .CountAsync();

        Categories = await _context.ProductCategories.Where(c => c.IsActive).ToListAsync();
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, Guid? categoryFilter, string? typeFilter, int page = 1, int pageSize = 10)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(search) ||
                p.Sku.ToLower().Contains(search) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(search)));
        }

        if (categoryFilter.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(typeFilter) && Enum.TryParse<ProductType>(typeFilter, out var productType))
        {
            query = query.Where(p => p.ProductType == productType);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var products = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Get stock levels for products
        var productIds = products.Select(p => p.Id).ToList();
        var stockLevels = await _context.StockLevels
            .Where(s => productIds.Contains(s.ProductId))
            .GroupBy(s => s.ProductId)
            .Select(g => new { ProductId = g.Key, TotalOnHand = g.Sum(s => s.QuantityOnHand) })
            .ToDictionaryAsync(x => x.ProductId, x => x.TotalOnHand);

        return Partial("_ProductsTableRows", new ProductsTableViewModel
        {
            Products = products,
            StockLevels = stockLevels,
            Page = page,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var categories = await _context.ProductCategories.Where(c => c.IsActive).ToListAsync();

        return Partial("_ProductForm", new ProductFormViewModel
        {
            IsEdit = false,
            Categories = categories
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return NotFound();

        var categories = await _context.ProductCategories.Where(c => c.IsActive).ToListAsync();

        return Partial("_ProductForm", new ProductFormViewModel
        {
            IsEdit = true,
            Product = product,
            Categories = categories
        });
    }

    public async Task<IActionResult> OnPostAsync(ProductFormInput input)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        Product? product;

        if (input.Id.HasValue)
        {
            product = await _context.Products.FindAsync(input.Id.Value);
            if (product == null)
                return NotFound();
        }
        else
        {
            product = new Product
            {
                Id = Guid.NewGuid(),
                Sku = await GenerateSkuAsync()
            };
            _context.Products.Add(product);
        }

        product.Name = input.Name;
        product.Description = input.Description;
        product.Barcode = input.Barcode;
        product.CategoryId = input.CategoryId;
        product.ProductType = input.ProductType;
        product.UnitOfMeasure = input.UnitOfMeasure;
        product.Brand = input.Brand;
        product.Manufacturer = input.Manufacturer;
        product.CostPrice = input.CostPrice;
        product.SellingPrice = input.SellingPrice;
        product.TaxRate = input.TaxRate;
        product.ReorderLevel = input.ReorderLevel;
        product.MinimumStock = input.MinimumStock;
        product.MaximumStock = input.MaximumStock;
        product.Weight = input.Weight;
        product.IsSellable = input.IsSellable;
        product.IsPurchasable = input.IsPurchasable;
        product.IsActive = input.IsActive;

        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return NotFound();

        // Check if product has stock
        var hasStock = await _context.StockLevels.AnyAsync(s => s.ProductId == id && s.QuantityOnHand > 0);
        if (hasStock)
        {
            return BadRequest("Cannot delete product with existing stock. Adjust stock to zero first.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return await OnGetTableAsync(null, null, null);
    }

    private async Task<string> GenerateSkuAsync()
    {
        var lastProduct = await _context.Products
            .IgnoreQueryFilters()
            .OrderByDescending(p => p.Sku)
            .FirstOrDefaultAsync(p => p.Sku.StartsWith("PRD"));

        if (lastProduct == null)
            return "PRD00001";

        var lastNumber = int.Parse(lastProduct.Sku.Replace("PRD", ""));
        return $"PRD{(lastNumber + 1):D5}";
    }
}

public class ProductsTableViewModel
{
    public List<Product> Products { get; set; } = new();
    public Dictionary<Guid, decimal> StockLevels { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
}

public class ProductFormViewModel
{
    public bool IsEdit { get; set; }
    public Product? Product { get; set; }
    public List<ProductCategory> Categories { get; set; } = new();
}

public class ProductFormInput
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Barcode { get; set; }
    public Guid? CategoryId { get; set; }
    public ProductType ProductType { get; set; } = ProductType.Goods;
    public string? UnitOfMeasure { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellingPrice { get; set; }
    public decimal TaxRate { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MinimumStock { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal? Weight { get; set; }
    public bool IsSellable { get; set; } = true;
    public bool IsPurchasable { get; set; } = true;
    public bool IsActive { get; set; } = true;
}
