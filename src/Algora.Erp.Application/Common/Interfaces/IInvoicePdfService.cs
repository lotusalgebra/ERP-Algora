using Algora.Erp.Domain.Entities.Finance;

namespace Algora.Erp.Application.Common.Interfaces;

public interface IInvoicePdfService
{
    byte[] GenerateInvoicePdf(Invoice invoice);
}
