namespace Algora.Erp.Integrations.Common.Models;

public class CrmOpportunity
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? ContactId { get; set; }
    public string? ContactName { get; set; }
    public decimal? Amount { get; set; }
    public string? CurrencyCode { get; set; }
    public string? StageName { get; set; }
    public int? Probability { get; set; }
    public DateTime? CloseDate { get; set; }
    public string? Type { get; set; }
    public string? LeadSource { get; set; }
    public string? NextStep { get; set; }
    public string? Description { get; set; }
    public bool IsClosed { get; set; }
    public bool IsWon { get; set; }
    public string? ForecastCategory { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}
