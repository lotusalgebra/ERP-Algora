using Algora.Erp.Domain.Entities.Common;

namespace Algora.Erp.Domain.Entities.Ecommerce;

/// <summary>
/// Payment method configuration for eCommerce
/// </summary>
public class WebPaymentMethod : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;  // stripe, paypal, cod, bank_transfer
    public string? Description { get; set; }
    public string? Instructions { get; set; }  // For manual methods like bank transfer

    public PaymentGateway Gateway { get; set; } = PaymentGateway.Manual;

    // Gateway config (encrypted)
    public string? ApiKey { get; set; }
    public string? SecretKey { get; set; }
    public string? WebhookSecret { get; set; }
    public bool IsSandbox { get; set; }

    // Fees
    public decimal? TransactionFeePercent { get; set; }
    public decimal? TransactionFeeFixed { get; set; }

    // Restrictions
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? AllowedCountries { get; set; }

    public string? IconUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public enum PaymentGateway
{
    Manual,
    Stripe,
    PayPal,
    Square,
    Razorpay,
    COD  // Cash on Delivery
}
