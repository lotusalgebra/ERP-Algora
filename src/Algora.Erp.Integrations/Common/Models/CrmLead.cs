namespace Algora.Erp.Integrations.Common.Models;

public class CrmLead
{
    public string? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Title { get; set; }
    public string? Industry { get; set; }
    public string? LeadSource { get; set; }
    public string? Status { get; set; }
    public string? Rating { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public string? Website { get; set; }
    public bool IsConverted { get; set; }
    public DateTime? ConvertedDate { get; set; }
    public string? ConvertedAccountId { get; set; }
    public string? ConvertedContactId { get; set; }
    public string? ConvertedOpportunityId { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}
