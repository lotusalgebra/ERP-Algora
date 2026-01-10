using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Ecommerce;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Ecommerce.Categories;

[IgnoreAntiforgeryToken]
public class IndexModel : PageModel
{
    private readonly IApplicationDbContext _context;

    public IndexModel(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetTableAsync(string? search, string? statusFilter, int pageNumber = 1, int pageSize = 10)
    {
        var query = _context.WebCategories
            .Include(c => c.Parent)
            .Include(c => c.Products)
            .Where(c => !c.IsDeleted)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(search) ||
                c.Slug.ToLower().Contains(search) ||
                (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(statusFilter) && bool.TryParse(statusFilter, out var isActive))
        {
            query = query.Where(c => c.IsActive == isActive);
        }

        var totalRecords = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

        var categories = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Partial("_CategoriesTableRows", new WebCategoriesTableViewModel
        {
            Categories = categories,
            Page = pageNumber,
            PageSize = pageSize,
            TotalRecords = totalRecords,
            TotalPages = totalPages
        });
    }

    public async Task<IActionResult> OnGetCreateFormAsync()
    {
        var categories = await _context.WebCategories
            .Where(c => c.IsActive && !c.IsDeleted)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Partial("_CategoryForm", new CategoryFormViewModel
        {
            IsEdit = false,
            ParentCategories = categories
        });
    }

    public async Task<IActionResult> OnGetEditFormAsync(Guid id)
    {
        var category = await _context.WebCategories
            .Include(c => c.Parent)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        var categories = await _context.WebCategories
            .Where(c => c.IsActive && !c.IsDeleted && c.Id != id)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Partial("_CategoryForm", new CategoryFormViewModel
        {
            IsEdit = true,
            Category = category,
            ParentCategories = categories
        });
    }

    public async Task<IActionResult> OnPostAsync(CategoryFormInput input)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        WebCategory? category;

        if (input.Id.HasValue)
        {
            category = await _context.WebCategories.FindAsync(input.Id.Value);
            if (category == null) return NotFound();
        }
        else
        {
            category = new WebCategory { Id = Guid.NewGuid() };
            _context.WebCategories.Add(category);
        }

        category.Name = input.Name;
        category.Slug = input.Slug ?? GenerateSlug(input.Name);
        category.Description = input.Description;
        category.ImageUrl = input.ImageUrl;
        category.ParentId = input.ParentId;
        category.IsActive = input.IsActive;
        category.SortOrder = input.SortOrder;
        category.MetaTitle = input.MetaTitle;
        category.MetaDescription = input.MetaDescription;

        await _context.SaveChangesAsync();
        return await OnGetTableAsync(null, null);
    }

    public async Task<IActionResult> OnDeleteAsync(Guid id)
    {
        var category = await _context.WebCategories
            .Include(c => c.Children)
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null) return NotFound();

        if (category.Children.Any())
            return BadRequest("Cannot delete category with subcategories.");

        if (category.Products.Any())
            return BadRequest("Cannot delete category with products. Reassign products first.");

        category.IsDeleted = true;
        await _context.SaveChangesAsync();
        return await OnGetTableAsync(null, null);
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower().Replace(" ", "-").Replace("&", "and").Replace("'", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug;
    }
}

public class CategoryFormViewModel
{
    public bool IsEdit { get; set; }
    public WebCategory? Category { get; set; }
    public List<WebCategory> ParentCategories { get; set; } = new();
}

public class CategoryFormInput
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

public class WebCategoriesTableViewModel
{
    public List<WebCategory> Categories { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public Shared.PaginationViewModel Pagination => new()
    {
        Page = Page,
        PageSize = PageSize,
        TotalRecords = TotalRecords,
        PageUrl = "/Ecommerce/Categories",
        Handler = "Table",
        HxTarget = "#categoriesTableBody",
        HxInclude = "#searchInput,#statusFilter"
    };
}
