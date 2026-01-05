using System.Security.Claims;
using Algora.Erp.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Algora.Erp.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return null;
        }
    }

    public string? Email => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value;

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public IEnumerable<string> Roles
    {
        get
        {
            var roleClaims = _httpContextAccessor.HttpContext?.User?.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value);
            return roleClaims ?? Enumerable.Empty<string>();
        }
    }

    public IEnumerable<string> Permissions
    {
        get
        {
            var permissionClaims = _httpContextAccessor.HttpContext?.User?.Claims
                .Where(c => c.Type == "permission")
                .Select(c => c.Value);
            return permissionClaims ?? Enumerable.Empty<string>();
        }
    }

    public bool HasPermission(string permission)
    {
        return Permissions.Contains(permission);
    }

    public bool IsInRole(string role)
    {
        return Roles.Contains(role);
    }
}
