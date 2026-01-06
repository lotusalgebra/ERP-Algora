using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop;

public class ProductModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public ProductModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public EcommerceProduct? Product { get; set; }
    public List<EcommerceProduct> RelatedProducts { get; set; } = new();

    public async Task OnGetAsync(string slug)
    {
        Product = await _context.EcommerceProducts
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.Status == ProductStatus.Active);

        if (Product != null && Product.CategoryId.HasValue)
        {
            // Get related products from same category
            RelatedProducts = await _context.EcommerceProducts
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => p.Status == ProductStatus.Active &&
                           p.Id != Product.Id &&
                           p.CategoryId == Product.CategoryId &&
                           p.StockQuantity > 0)
                .OrderByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();
        }
    }
}
