using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Product images for gallery
/// </summary>
public class ProductImage : BaseEntity
{
    public Guid ProductId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }

    public EcommerceProduct? Product { get; set; }
}
