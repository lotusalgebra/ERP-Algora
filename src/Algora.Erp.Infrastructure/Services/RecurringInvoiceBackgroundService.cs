using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Infrastructure.Data;
using Algora.Erp.Infrastructure.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Algora.Erp.Infrastructure.Services;

/// <summary>
/// Background service that automatically generates recurring invoices
/// </summary>
public class RecurringInvoiceBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RecurringInvoiceBackgroundService> _logger;
    private readonly RecurringInvoiceSettings _settings;

    public RecurringInvoiceBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<RecurringInvoiceBackgroundService> logger,
        IOptions<RecurringInvoiceSettings> settings)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Recurring Invoice Background Service started");

        // Initial delay to let the application start up
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRecurringInvoicesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recurring invoices");
            }

            // Wait for the configured interval before next run
            var interval = TimeSpan.FromMinutes(_settings.IntervalMinutes);
            _logger.LogDebug("Next recurring invoice check in {Interval}", interval);
            await Task.Delay(interval, stoppingToken);
        }

        _logger.LogInformation("Recurring Invoice Background Service stopped");
    }

    private async Task ProcessRecurringInvoicesAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting recurring invoice generation check at {Time}", DateTime.UtcNow);

        using var scope = _serviceProvider.CreateScope();
        var masterContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();

        // Get all active tenants
        var tenants = await masterContext.Tenants
            .Where(t => t.IsActive)
            .ToListAsync(cancellationToken);

        _logger.LogInformation("Found {TenantCount} active tenants to process", tenants.Count);

        var totalGenerated = 0;

        foreach (var tenant in tenants)
        {
            try
            {
                var invoicesGenerated = await ProcessTenantRecurringInvoicesAsync(
                    tenant.ConnectionString,
                    tenant.Name,
                    cancellationToken);

                totalGenerated += invoicesGenerated;

                if (invoicesGenerated > 0)
                {
                    _logger.LogInformation(
                        "Generated {Count} recurring invoices for tenant {TenantName}",
                        invoicesGenerated,
                        tenant.Name);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error processing recurring invoices for tenant {TenantName}",
                    tenant.Name);
            }
        }

        _logger.LogInformation(
            "Completed recurring invoice generation. Total invoices generated: {Count}",
            totalGenerated);
    }

    private async Task<int> ProcessTenantRecurringInvoicesAsync(
        string connectionString,
        string tenantName,
        CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        // Create tenant-specific context
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        var dateTime = scope.ServiceProvider.GetRequiredService<IDateTime>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var pdfService = scope.ServiceProvider.GetRequiredService<IInvoicePdfService>();

        await using var context = new ApplicationDbContext(optionsBuilder.Options, currentUserService, dateTime);

        // Get due recurring invoices
        var today = DateTime.Today;
        var dueRecurringInvoices = await context.RecurringInvoices
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Where(r => r.Status == RecurringInvoiceStatus.Active
                && r.NextGenerationDate.HasValue
                && r.NextGenerationDate.Value.Date <= today
                && (!r.EndDate.HasValue || r.EndDate.Value >= today)
                && (!r.MaxOccurrences.HasValue || r.OccurrencesGenerated < r.MaxOccurrences.Value))
            .ToListAsync(cancellationToken);

        if (!dueRecurringInvoices.Any())
        {
            return 0;
        }

        _logger.LogDebug(
            "Found {Count} due recurring invoices for tenant {TenantName}",
            dueRecurringInvoices.Count,
            tenantName);

        var generatedCount = 0;

        foreach (var recurring in dueRecurringInvoices)
        {
            try
            {
                var invoice = await GenerateInvoiceFromTemplateAsync(
                    context,
                    recurring,
                    dateTime,
                    cancellationToken);

                generatedCount++;

                // Handle auto-send email
                if (recurring.AutoSendEmail && !string.IsNullOrEmpty(recurring.EmailRecipients))
                {
                    try
                    {
                        await SendInvoiceEmailAsync(context, invoice, recurring.EmailRecipients, pdfService, emailService);
                        _logger.LogDebug(
                            "Sent invoice {InvoiceNumber} to {Recipients}",
                            invoice.InvoiceNumber,
                            recurring.EmailRecipients);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex,
                            "Failed to send email for invoice {InvoiceNumber}",
                            invoice.InvoiceNumber);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to generate invoice from recurring template {RecurringName}",
                    recurring.Name);
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        return generatedCount;
    }

    private async Task<Invoice> GenerateInvoiceFromTemplateAsync(
        ApplicationDbContext context,
        RecurringInvoice recurring,
        IDateTime dateTime,
        CancellationToken cancellationToken)
    {
        // Generate invoice number
        var invoiceCount = await context.Invoices.CountAsync(cancellationToken) + 1;
        var invoiceNumber = $"INV-{dateTime.UtcNow.Year}-{invoiceCount:D5}";

        var invoiceDate = DateTime.Today;
        var dueDate = invoiceDate.AddDays(recurring.PaymentTermDays);

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            Type = recurring.InvoiceType,
            Status = recurring.AutoSend ? InvoiceStatus.Sent : InvoiceStatus.Draft,
            CustomerId = recurring.CustomerId,
            RecurringInvoiceId = recurring.Id,
            InvoiceDate = invoiceDate,
            DueDate = dueDate,
            Currency = recurring.Currency,
            PaymentTerms = recurring.PaymentTerms,
            PaymentTermDays = recurring.PaymentTermDays,
            BillingName = recurring.BillingName ?? recurring.Customer?.Name,
            BillingAddress = recurring.BillingAddress ?? recurring.Customer?.BillingAddress,
            BillingCity = recurring.BillingCity ?? recurring.Customer?.BillingCity,
            BillingState = recurring.BillingState ?? recurring.Customer?.BillingState,
            BillingPostalCode = recurring.BillingPostalCode ?? recurring.Customer?.BillingPostalCode,
            BillingCountry = recurring.BillingCountry ?? recurring.Customer?.BillingCountry,
            Reference = recurring.Reference,
            Notes = recurring.Notes,
            InternalNotes = $"Auto-generated from recurring invoice: {recurring.Name}"
        };

        if (recurring.AutoSend)
        {
            invoice.SentAt = dateTime.UtcNow;
        }

        // Copy lines
        var lineNumber = 1;
        decimal subtotal = 0;
        decimal totalTax = 0;

        foreach (var templateLine in recurring.Lines.OrderBy(l => l.LineNumber))
        {
            var lineSubtotal = templateLine.Quantity * templateLine.UnitPrice;
            var discountAmount = lineSubtotal * (templateLine.DiscountPercent / 100);
            var afterDiscount = lineSubtotal - discountAmount;
            var taxAmount = afterDiscount * (templateLine.TaxPercent / 100);
            var lineTotal = afterDiscount + taxAmount;

            var invoiceLine = new InvoiceLine
            {
                LineNumber = lineNumber++,
                ProductId = templateLine.ProductId,
                ProductCode = templateLine.ProductCode,
                Description = templateLine.Description,
                Quantity = templateLine.Quantity,
                UnitOfMeasure = templateLine.UnitOfMeasure,
                UnitPrice = templateLine.UnitPrice,
                DiscountPercent = templateLine.DiscountPercent,
                DiscountAmount = discountAmount,
                TaxPercent = templateLine.TaxPercent,
                TaxAmount = taxAmount,
                LineTotal = lineTotal,
                AccountId = templateLine.AccountId,
                Notes = templateLine.Notes
            };

            invoice.Lines.Add(invoiceLine);
            subtotal += afterDiscount;
            totalTax += taxAmount;
        }

        invoice.SubTotal = subtotal;
        invoice.TaxAmount = totalTax;
        invoice.TotalAmount = subtotal + totalTax;
        invoice.BalanceDue = invoice.TotalAmount;

        context.Invoices.Add(invoice);

        // Update recurring invoice stats
        recurring.OccurrencesGenerated++;
        recurring.LastGeneratedDate = dateTime.UtcNow;
        recurring.NextGenerationDate = recurring.CalculateNextGenerationDate();

        // Check if recurring invoice should be completed
        if (recurring.MaxOccurrences.HasValue && recurring.OccurrencesGenerated >= recurring.MaxOccurrences.Value)
        {
            recurring.Status = RecurringInvoiceStatus.Completed;
            recurring.NextGenerationDate = null;
            _logger.LogInformation(
                "Recurring invoice {Name} completed after {Count} occurrences",
                recurring.Name,
                recurring.OccurrencesGenerated);
        }
        else if (recurring.EndDate.HasValue && recurring.NextGenerationDate > recurring.EndDate)
        {
            recurring.Status = RecurringInvoiceStatus.Completed;
            recurring.NextGenerationDate = null;
            _logger.LogInformation(
                "Recurring invoice {Name} completed - end date reached",
                recurring.Name);
        }

        return invoice;
    }

    private async Task SendInvoiceEmailAsync(
        ApplicationDbContext context,
        Invoice invoice,
        string recipients,
        IInvoicePdfService pdfService,
        IEmailService emailService)
    {
        // Load full invoice data for PDF
        var fullInvoice = await context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == invoice.Id);

        if (fullInvoice == null) return;

        var pdfBytes = pdfService.GenerateInvoicePdf(fullInvoice);
        var fileName = $"Invoice_{fullInvoice.InvoiceNumber.Replace("-", "_")}.pdf";

        var emailMessage = new EmailMessage
        {
            To = recipients,
            Subject = $"Invoice {fullInvoice.InvoiceNumber}",
            Body = GetEmailBody(fullInvoice),
            IsHtml = true
        };

        await emailService.SendEmailWithAttachmentAsync(emailMessage, pdfBytes, fileName, "application/pdf");
    }

    private string GetEmailBody(Invoice invoice)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #1a1a2e; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background: #f9f9f9; }}
        .footer {{ padding: 20px; text-align: center; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Algora ERP</h1>
        </div>
        <div class='content'>
            <p>Dear {invoice.Customer?.Name ?? "Customer"},</p>
            <p>Please find attached invoice {invoice.InvoiceNumber} for {invoice.TotalAmount:C2}.</p>
            <p><strong>Invoice Details:</strong></p>
            <ul>
                <li>Invoice Number: {invoice.InvoiceNumber}</li>
                <li>Invoice Date: {invoice.InvoiceDate:MMMM dd, yyyy}</li>
                <li>Due Date: {invoice.DueDate:MMMM dd, yyyy}</li>
                <li>Amount Due: {invoice.BalanceDue:C2}</li>
            </ul>
            <p>Payment Terms: {invoice.PaymentTerms ?? "Net 30"}</p>
            <p>Thank you for your business!</p>
            <p>Best regards,<br>Algora ERP</p>
        </div>
        <div class='footer'>
            <p>This is an automatically generated invoice from your recurring billing schedule.</p>
        </div>
    </div>
</body>
</html>";
    }
}

/// <summary>
/// Settings for the recurring invoice background service
/// </summary>
public class RecurringInvoiceSettings
{
    /// <summary>
    /// Interval in minutes between recurring invoice checks (default: 60 = 1 hour)
    /// </summary>
    public int IntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Whether the background service is enabled
    /// </summary>
    public bool Enabled { get; set; } = true;
}
