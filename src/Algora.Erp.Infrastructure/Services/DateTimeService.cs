using Algora.Erp.Application.Common.Interfaces;

namespace Algora.Erp.Infrastructure.Services;

public class DateTimeService : IDateTime
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
}
