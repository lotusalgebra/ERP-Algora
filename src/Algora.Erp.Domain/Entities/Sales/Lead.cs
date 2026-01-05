using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Sales;

public class Lead : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Website { get; set; }

    public LeadSource Source { get; set; } = LeadSource.Website;
    public LeadStatus Status { get; set; } = LeadStatus.New;
    public LeadRating Rating { get; set; } = LeadRating.Cold;

    public decimal? EstimatedValue { get; set; }
    public int? EstimatedCloseInDays { get; set; }

    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }

    public DateTime? LastContactDate { get; set; }
    public DateTime? NextFollowUpDate { get; set; }

    public string? Notes { get; set; }
    public string? Tags { get; set; }

    // Conversion to Customer
    public Guid? ConvertedCustomerId { get; set; }
    public DateTime? ConvertedAt { get; set; }
}

public enum LeadSource
{
    Website = 0,
    Referral = 1,
    SocialMedia = 2,
    Advertisement = 3,
    TradeShow = 4,
    ColdCall = 5,
    Email = 6,
    Partner = 7,
    Other = 8
}

public enum LeadStatus
{
    New = 0,
    Contacted = 1,
    Qualified = 2,
    Proposal = 3,
    Negotiation = 4,
    Won = 5,
    Lost = 6,
    Unqualified = 7
}

public enum LeadRating
{
    Cold = 0,
    Warm = 1,
    Hot = 2
}
