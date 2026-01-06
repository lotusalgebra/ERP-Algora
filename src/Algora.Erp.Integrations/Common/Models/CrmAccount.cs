namespace Algora.Erp.Integrations.Common.Models;

public class CrmAccount
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Type { get; set; }
    public string? Industry { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public int? NumberOfEmployees { get; set; }
    public string? Phone { get; set; }
    public string? Fax { get; set; }
    public string? Website { get; set; }
    public string? BillingStreet { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingPostalCode { get; set; }
    public string? BillingCountry { get; set; }
    public string? ShippingStreet { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingState { get; set; }
    public string? ShippingPostalCode { get; set; }
    public string? ShippingCountry { get; set; }
    public string? Description { get; set; }
    public string? ParentAccountId { get; set; }
    public string? OwnerId { get; set; }
    public string? Rating { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}
