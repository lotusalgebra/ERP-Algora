using Algora.Erp.Domain.Entities.Finance;

namespace Algora.Erp.Application.Common.Interfaces;

public interface IRecurringInvoiceService
{
    /// <summary>
    /// Generates invoices for all recurring invoices that are due
    /// </summary>
    Task<List<Invoice>> GenerateDueInvoicesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates an invoice from a recurring invoice template
    /// </summary>
    Task<Invoice> GenerateInvoiceAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a recurring invoice
    /// </summary>
    Task PauseAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused recurring invoice
    /// </summary>
    Task ResumeAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a recurring invoice
    /// </summary>
    Task CancelAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the next generation date for a recurring invoice
    /// </summary>
    Task UpdateNextGenerationDateAsync(Guid recurringInvoiceId, CancellationToken cancellationToken = default);
}
