namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Abstraction for DateTime to enable testing
/// </summary>
public interface IDateTime
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
