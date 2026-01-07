using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Domain.Entities.Finance;
using Algora.Erp.Domain.Entities.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Algora.Erp.Web.Pages.Finance.Invoices;

public class DetailsModel : PageModel
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IInvoicePdfService _pdfService;
    private readonly ITaxConfigurationService _taxService;

    public DetailsModel(
        IApplicationDbContext context,
        IEmailService emailService,
        IInvoicePdfService pdfService,
        ITaxConfigurationService taxService)
    {
        _context = context;
        _emailService = emailService;
        _pdfService = pdfService;
        _taxService = taxService;
    }

    public Invoice Invoice { get; set; } = null!;
    public List<InvoicePayment> Payments { get; set; } = new();

    // Tax configuration for dynamic labels
    public TaxConfiguration? TaxConfig { get; set; }

    [BindProperty]
    public RecordPaymentInput PaymentInput { get; set; } = new();

    [BindProperty]
    public SendEmailInput EmailInput { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.Lines)
                .ThenInclude(l => l.GstSlab)
            .Include(i => i.Payments)
            .Include(i => i.FromLocation)
                .ThenInclude(l => l!.State)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (invoice == null)
            return NotFound();

        Invoice = invoice;
        Payments = invoice.Payments.OrderByDescending(p => p.PaymentDate).ToList();

        // Load tax configuration for dynamic labels
        TaxConfig = await _taxService.GetCurrentTaxConfigurationAsync();

        // Initialize payment input
        PaymentInput.InvoiceId = id;
        PaymentInput.Amount = invoice.BalanceDue;
        PaymentInput.PaymentDate = DateTime.Today;

        // Initialize email input
        EmailInput.InvoiceId = id;
        EmailInput.To = invoice.Customer?.Email ?? "";
        EmailInput.Subject = $"Invoice {invoice.InvoiceNumber} from Algora ERP";
        EmailInput.Message = GetDefaultEmailMessage(invoice);

        return Page();
    }

    private string GetDefaultEmailMessage(Invoice invoice)
    {
        return $@"Dear {invoice.Customer?.Name ?? "Customer"},

Please find attached invoice {invoice.InvoiceNumber} for {invoice.TotalAmount:C2}.

Invoice Details:
- Invoice Number: {invoice.InvoiceNumber}
- Invoice Date: {invoice.InvoiceDate:MMMM dd, yyyy}
- Due Date: {invoice.DueDate:MMMM dd, yyyy}
- Amount Due: {invoice.BalanceDue:C2}

Payment Terms: {invoice.PaymentTerms ?? "Net 30"}

Thank you for your business!

Best regards,
Algora ERP";
    }

    public async Task<IActionResult> OnPostSendAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound();

        invoice.Status = InvoiceStatus.Sent;
        invoice.SentAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Invoice sent successfully.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRecordPaymentAsync()
    {
        var invoice = await _context.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == PaymentInput.InvoiceId);

        if (invoice == null)
            return NotFound();

        // Generate payment number
        var paymentCount = await _context.InvoicePayments.CountAsync() + 1;
        var paymentNumber = $"PAY-{DateTime.Now.Year}-{paymentCount:D4}";

        var payment = new InvoicePayment
        {
            InvoiceId = invoice.Id,
            PaymentNumber = paymentNumber,
            PaymentDate = PaymentInput.PaymentDate,
            Amount = PaymentInput.Amount,
            PaymentMethod = PaymentInput.PaymentMethod,
            Reference = PaymentInput.Reference,
            Notes = PaymentInput.Notes
        };

        _context.InvoicePayments.Add(payment);

        // Update invoice
        invoice.PaidAmount += PaymentInput.Amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.BalanceDue <= 0)
        {
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidDate = DateTime.UtcNow;
            invoice.BalanceDue = 0;
        }
        else if (invoice.PaidAmount > 0)
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Payment of {PaymentInput.Amount:C2} recorded successfully.";
        return RedirectToPage(new { id = invoice.Id });
    }

    public async Task<IActionResult> OnPostMarkPaidAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound();

        // Record a full payment
        var paymentCount = await _context.InvoicePayments.CountAsync() + 1;
        var paymentNumber = $"PAY-{DateTime.Now.Year}-{paymentCount:D4}";

        var payment = new InvoicePayment
        {
            InvoiceId = invoice.Id,
            PaymentNumber = paymentNumber,
            PaymentDate = DateTime.Today,
            Amount = invoice.BalanceDue,
            PaymentMethod = PaymentMethod.Other,
            Notes = "Marked as paid"
        };

        _context.InvoicePayments.Add(payment);

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidDate = DateTime.UtcNow;
        invoice.PaidAmount = invoice.TotalAmount;
        invoice.BalanceDue = 0;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Invoice marked as paid.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostVoidAsync(Guid id)
    {
        var invoice = await _context.Invoices.FindAsync(id);
        if (invoice == null)
            return NotFound();

        invoice.Status = InvoiceStatus.Void;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Invoice voided.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostDeletePaymentAsync(Guid id, Guid paymentId)
    {
        var payment = await _context.InvoicePayments
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == paymentId);

        if (payment == null)
            return NotFound();

        var invoice = payment.Invoice;

        // Restore balance
        invoice.PaidAmount -= payment.Amount;
        invoice.BalanceDue = invoice.TotalAmount - invoice.PaidAmount;

        if (invoice.PaidAmount <= 0)
        {
            invoice.Status = invoice.DueDate < DateTime.UtcNow ? InvoiceStatus.Overdue : InvoiceStatus.Sent;
            invoice.PaidDate = null;
        }
        else
        {
            invoice.Status = InvoiceStatus.PartiallyPaid;
        }

        _context.InvoicePayments.Remove(payment);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Payment deleted.";
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostSendEmailAsync()
    {
        var invoice = await _context.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.Lines)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == EmailInput.InvoiceId);

        if (invoice == null)
            return NotFound();

        if (string.IsNullOrEmpty(EmailInput.To))
        {
            TempData["Error"] = "Email address is required.";
            return RedirectToPage(new { id = EmailInput.InvoiceId });
        }

        try
        {
            var emailMessage = new EmailMessage
            {
                To = EmailInput.To,
                Cc = EmailInput.Cc,
                Subject = EmailInput.Subject,
                Body = FormatEmailBody(EmailInput.Message),
                IsHtml = true
            };

            if (EmailInput.AttachPdf)
            {
                var pdfBytes = _pdfService.GenerateInvoicePdf(invoice);
                var fileName = $"Invoice_{invoice.InvoiceNumber.Replace("-", "_")}.pdf";

                await _emailService.SendEmailWithAttachmentAsync(
                    emailMessage,
                    pdfBytes,
                    fileName,
                    "application/pdf");
            }
            else
            {
                await _emailService.SendEmailAsync(emailMessage);
            }

            // Update invoice status if it was draft
            if (invoice.Status == InvoiceStatus.Draft)
            {
                invoice.Status = InvoiceStatus.Sent;
                invoice.SentAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"Invoice emailed successfully to {EmailInput.To}.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Failed to send email: {ex.Message}";
        }

        return RedirectToPage(new { id = EmailInput.InvoiceId });
    }

    private string FormatEmailBody(string plainTextMessage)
    {
        // Convert plain text message to HTML
        var htmlMessage = plainTextMessage
            .Replace("\r\n", "<br>")
            .Replace("\n", "<br>");

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
            <p>{htmlMessage}</p>
        </div>
        <div class='footer'>
            <p>This email was sent from Algora ERP. Please do not reply directly to this email.</p>
        </div>
    </div>
</body>
</html>";
    }
}

public class RecordPaymentInput
{
    public Guid InvoiceId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.Today;
    public decimal Amount { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.BankTransfer;
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}

public class SendEmailInput
{
    public Guid InvoiceId { get; set; }
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool AttachPdf { get; set; } = true;
}
