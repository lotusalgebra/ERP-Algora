using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Infrastructure.Services;

public class RecurringInvoiceService : IRecurringInvoiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _dateTime;
    private readonly IEmailService _emailService;
    private readonly IInvoicePdfService _pdfService;

    public RecurringInvoiceService(
        IApplicationDbContext context,
        IDateTime dateTime,
        IEmailService emailService,
        IInvoicePdfService pdfService)
    {
        _context = context;
        _dateTime = dateTime;
        _emailService = emailService;
        _pdfService = pdfService;
    }

    public async Task<List<Invoice>> GenerateDueInvoicesAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var dueRecurringInvoices = await _context.RecurringInvoices
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .Where(r => r.Status == RecurringInvoiceStatus.Active
                && r.NextGenerationDate.HasValue
                && r.NextGenerationDate.Value.Date <= today
                && (!r.EndDate.HasValue || r.EndDate.Value >= today)
                && (!r.MaxOccurrences.HasValue || r.OccurrencesGenerated < r.MaxOccurrences.Value))
            .ToListAsync(cancellationToken);

        var generatedInvoices = new List<Invoice>();

        foreach (var recurring in dueRecurringInvoices)
        {
            var invoice = await GenerateInvoiceFromTemplateAsync(recurring, cancellationToken);
            generatedInvoices.Add(invoice);

            // Handle auto-send
            if (recurring.AutoSendEmail && !string.IsNullOrEmpty(recurring.EmailRecipients))
            {
                await SendInvoiceEmailAsync(invoice, recurring.EmailRecipients);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return generatedInvoices;
    }

    public async Task<Invoice> GenerateInvoiceAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default)
    {
        var recurring = await _context.RecurringInvoices
            .Include(r => r.Customer)
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == recurringInvoiceId, cancellationToken);

        if (recurring == null)
            throw new InvalidOperationException($"Recurring invoice with ID {recurringInvoiceId} not found.");

        var invoice = await GenerateInvoiceFromTemplateAsync(recurring, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return invoice;
    }

    public async Task PauseAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default)
    {
        var recurring = await _context.RecurringInvoices.FindAsync(new object[] { recurringInvoiceId }, cancellationToken);
        if (recurring == null)
            throw new InvalidOperationException($"Recurring invoice with ID {recurringInvoiceId} not found.");

        if (recurring.Status != RecurringInvoiceStatus.Active)
            throw new InvalidOperationException("Only active recurring invoices can be paused.");

        recurring.Status = RecurringInvoiceStatus.Paused;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task ResumeAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default)
    {
        var recurring = await _context.RecurringInvoices.FindAsync(new object[] { recurringInvoiceId }, cancellationToken);
        if (recurring == null)
            throw new InvalidOperationException($"Recurring invoice with ID {recurringInvoiceId} not found.");

        if (recurring.Status != RecurringInvoiceStatus.Paused)
            throw new InvalidOperationException("Only paused recurring invoices can be resumed.");

        recurring.Status = RecurringInvoiceStatus.Active;
        recurring.NextGenerationDate = recurring.CalculateNextGenerationDate();
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task CancelAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default)
    {
        var recurring = await _context.RecurringInvoices.FindAsync(new object[] { recurringInvoiceId }, cancellationToken);
        if (recurring == null)
            throw new InvalidOperationException($"Recurring invoice with ID {recurringInvoiceId} not found.");

        if (recurring.Status == RecurringInvoiceStatus.Cancelled)
            throw new InvalidOperationException("Recurring invoice is already cancelled.");

        recurring.Status = RecurringInvoiceStatus.Cancelled;
        recurring.NextGenerationDate = null;
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateNextGenerationDateAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default)
    {
        var recurring = await _context.RecurringInvoices.FindAsync(new object[] { recurringInvoiceId }, cancellationToken);
        if (recurring == null)
            throw new InvalidOperationException($"Recurring invoice with ID {recurringInvoiceId} not found.");

        if (recurring.Status == RecurringInvoiceStatus.Active)
        {
            recurring.NextGenerationDate = recurring.CalculateNextGenerationDate();
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<Invoice> GenerateInvoiceFromTemplateAsync(RecurringInvoice recurring, CancellationToken cancellationToken)
    {
        // Generate invoice number
        var invoiceCount = await _context.Invoices.CountAsync(cancellationToken) + 1;
        var invoiceNumber = $"INV-{_dateTime.UtcNow.Year}-{invoiceCount:D5}";

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
            InternalNotes = recurring.InternalNotes
        };

        if (recurring.AutoSend)
        {
            invoice.SentAt = _dateTime.UtcNow;
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

        _context.Invoices.Add(invoice);

        // Update recurring invoice stats
        recurring.OccurrencesGenerated++;
        recurring.LastGeneratedDate = _dateTime.UtcNow;
        recurring.NextGenerationDate = recurring.CalculateNextGenerationDate();

        // Check if recurring invoice should be completed
        if (recurring.MaxOccurrences.HasValue && recurring.OccurrencesGenerated >= recurring.MaxOccurrences.Value)
        {
            recurring.Status = RecurringInvoiceStatus.Completed;
            recurring.NextGenerationDate = null;
        }
        else if (recurring.EndDate.HasValue && recurring.NextGenerationDate > recurring.EndDate)
        {
            recurring.Status = RecurringInvoiceStatus.Completed;
            recurring.NextGenerationDate = null;
        }

        return invoice;
    }

    private async Task SendInvoiceEmailAsync(Invoice invoice, string recipients)
    {
        try
        {
            // Load full invoice data for PDF
            var fullInvoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Lines)
                .FirstOrDefaultAsync(i => i.Id == invoice.Id);

            if (fullInvoice == null) return;

            var pdfBytes = _pdfService.GenerateInvoicePdf(fullInvoice);
            var fileName = $"Invoice_{fullInvoice.InvoiceNumber.Replace("-", "_")}.pdf";

            var emailMessage = new EmailMessage
            {
                To = recipients,
                Subject = $"Invoice {fullInvoice.InvoiceNumber}",
                Body = GetEmailBody(fullInvoice),
                IsHtml = true
            };

            await _emailService.SendEmailWithAttachmentAsync(emailMessage, pdfBytes, fileName, "application/pdf");
        }
        catch
        {
            // Log error but don't fail the invoice generation
        }
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
