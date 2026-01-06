using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Shop;

public class CategoriesModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public CategoriesModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public List<WebCategory> Categories { get; set; } = new();
    public Dictionary<Guid, int> ProductCounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        Categories = await _context.WebCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .ToListAsync();

        // Get product counts per category
        var counts = await _context.EcommerceProducts
            .Where(p => p.Status == ProductStatus.Active && p.CategoryId.HasValue)
            .GroupBy(p => p.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Count = g.Count() })
            .ToListAsync();

        ProductCounts = counts.ToDictionary(x => x.CategoryId, x => x.Count);
    }
}
