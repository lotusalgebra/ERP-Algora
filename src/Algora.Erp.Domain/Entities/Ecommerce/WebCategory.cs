using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Product category for eCommerce storefront
/// </summary>
public class WebCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public Guid? ParentId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool ShowInMenu { get; set; } = true;

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Navigation
    public WebCategory? Parent { get; set; }
    public ICollection<WebCategory> Children { get; set; } = new List<WebCategory>();
    public ICollection<EcommerceProduct> Products { get; set; } = new List<EcommerceProduct>();
}
