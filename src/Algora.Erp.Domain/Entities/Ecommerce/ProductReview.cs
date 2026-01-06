using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Product review and rating
/// </summary>
public class ProductReview : AuditableEntity
{
    public Guid ProductId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OrderId { get; set; }

    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }

    public int Rating { get; set; }  // 1-5
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Pros { get; set; }
    public string? Cons { get; set; }

    // Images
    public string? ImageUrls { get; set; }  // Comma-separated

    // Moderation
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public DateTime? ApprovedAt { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime? AdminRespondedAt { get; set; }

    // Helpfulness
    public int HelpfulVotes { get; set; }
    public int UnhelpfulVotes { get; set; }

    public bool IsVerifiedPurchase { get; set; }

    public EcommerceProduct? Product { get; set; }
    public WebCustomer? Customer { get; set; }
}

public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected,
    Flagged
}
