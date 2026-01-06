namespace Algora.Erp.Integrations.Common.Models;

public class CrmContact
{
    public string? Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? MobilePhone { get; set; }
    public string? Title { get; set; }
    public string? Department { get; set; }
    public string? AccountId { get; set; }
    public string? AccountName { get; set; }
    public string? MailingStreet { get; set; }
    public string? MailingCity { get; set; }
    public string? MailingState { get; set; }
    public string? MailingPostalCode { get; set; }
    public string? MailingCountry { get; set; }
    public string? Description { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastModifiedDate { get; set; }
    public bool IsDeleted { get; set; }
}
