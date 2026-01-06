using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop;

public class DealsModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public DealsModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public List<EcommerceProduct> DealsProducts { get; set; } = new();

    public async Task OnGetAsync()
    {
        // Get products with discounts (CompareAtPrice > Price)
        DealsProducts = await _context.EcommerceProducts
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Where(p => p.Status == ProductStatus.Active &&
                       p.CompareAtPrice.HasValue &&
                       p.CompareAtPrice > p.Price)
            .OrderByDescending(p => (p.CompareAtPrice!.Value - p.Price) / p.CompareAtPrice.Value) // Sort by discount %
            .Take(20)
            .ToListAsync();
    }
}
