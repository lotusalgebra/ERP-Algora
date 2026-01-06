using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Promotional banner for storefront
/// </summary>
public class Banner : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? MobileImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public string? ButtonText { get; set; }

    public BannerPosition Position { get; set; } = BannerPosition.HomeSlider;

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Stats
    public int Impressions { get; set; }
    public int Clicks { get; set; }
}

public enum BannerPosition
{
    HomeSlider,
    HomeMiddle,
    HomeBanner,
    CategoryTop,
    Sidebar,
    Popup
}
