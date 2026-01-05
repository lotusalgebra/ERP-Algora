namespace Algora.Erp.Application.Common.Interfaces;

/// <summary>
/// Service to access information about the current user
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    IEnumerable<string> Roles { get; }
    IEnumerable<string> Permissions { get; }

    bool HasPermission(string permission);
    bool IsInRole(string role);
}
