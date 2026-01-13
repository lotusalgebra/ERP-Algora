using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Auth.Configuration;
using Algora.Erp.Auth.Interfaces;
using Algora.Erp.Auth.Models;
using Algora.Erp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Algora.Erp.Auth.Services;

/// <summary>
/// Authentication service implementation for tenant users
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _context;
    private readonly ITenantService _tenantService;
    private readonly AuthSettings _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IApplicationDbContext context,
        ITenantService tenantService,
        IOptions<AuthSettings> settings,
        ILogger<AuthService> logger)
    {
        _context = context;
        _tenantService = tenantService;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, string? ipAddress = null)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return AuthResult.Failed("Tenant context not established");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            return AuthResult.Failed("Invalid email or password");
        }

        if (user.Status != UserStatus.Active)
        {
            return AuthResult.Failed("Account is disabled");
        }

        // Check lockout
        if (user.LockoutEndAt.HasValue && user.LockoutEndAt > DateTime.UtcNow)
        {
            return AuthResult.Failed($"Account is locked. Try again after {user.LockoutEndAt.Value:g}");
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.FailedLoginAttempts++;
            if (user.FailedLoginAttempts >= _settings.Lockout.MaxFailedAttempts)
            {
                user.LockoutEndAt = DateTime.UtcNow.AddMinutes(_settings.Lockout.LockoutDurationMinutes);
                user.FailedLoginAttempts = 0;
                _logger.LogWarning("User {Email} locked out after {Attempts} failed attempts",
                    email, _settings.Lockout.MaxFailedAttempts);
            }
            await _context.SaveChangesAsync();
            return AuthResult.Failed("Invalid email or password");
        }

        // Reset lockout on successful login
        user.FailedLoginAttempts = 0;
        user.LockoutEndAt = null;
        user.LastLoginAt = DateTime.UtcNow;

        // Get roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "User").ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToListAsync();

        var accessToken = GenerateAccessToken(user.Id, tenantId.Value, user.Email, user.FullName, roles, permissions);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryAt = DateTime.UtcNow.AddDays(_settings.Jwt.RefreshTokenExpiryDays);

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {Email} logged in successfully", email);

        return AuthResult.Success(accessToken, refreshToken, new AuthUserInfo
        {
            Id = user.Id,
            TenantId = tenantId.Value,
            Email = user.Email,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            Permissions = permissions
        });
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return AuthResult.Failed("Tenant context not established");
        }

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

        if (user == null)
        {
            return AuthResult.Failed("Invalid refresh token");
        }

        if (user.RefreshTokenExpiryAt < DateTime.UtcNow)
        {
            return AuthResult.Failed("Refresh token has expired");
        }

        if (user.Status != UserStatus.Active)
        {
            return AuthResult.Failed("Account is disabled");
        }

        // Get roles and permissions
        var roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "User").ToList();
        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToListAsync();

        var newAccessToken = GenerateAccessToken(user.Id, tenantId.Value, user.Email, user.FullName, roles, permissions);
        var newRefreshToken = GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryAt = DateTime.UtcNow.AddDays(_settings.Jwt.RefreshTokenExpiryDays);

        await _context.SaveChangesAsync();

        return AuthResult.Success(newAccessToken, newRefreshToken, new AuthUserInfo
        {
            Id = user.Id,
            TenantId = tenantId.Value,
            Email = user.Email,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = roles,
            Permissions = permissions
        });
    }

    public async Task<bool> RevokeTokenAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryAt = null;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for user {UserId}", userId);
        return true;
    }

    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        var tenantId = _tenantService.GetCurrentTenantId();
        if (!tenantId.HasValue)
        {
            return AuthResult.Failed("Tenant context not established");
        }

        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            return AuthResult.Failed("A user with this email already exists");
        }

        // Get default role or create one
        var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "User");
        if (defaultRole == null)
        {
            return AuthResult.Failed("Default role not configured");
        }

        var user = new Domain.Entities.Administration.User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Status = UserStatus.Active,
            EmailConfirmed = false
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign default role
        var userRole = new Domain.Entities.Administration.UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id,
            AssignedAt = DateTime.UtcNow
        };
        _context.UserRoles.Add(userRole);
        await _context.SaveChangesAsync();

        _logger.LogInformation("New user {Email} registered", request.Email);

        // Load permissions for default role
        var permissions = await _context.RolePermissions
            .Where(rp => rp.RoleId == defaultRole.Id)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission!.Code)
            .ToListAsync();

        // Generate tokens for immediate login
        var accessToken = GenerateAccessToken(user.Id, tenantId.Value, user.Email, user.FullName, new[] { "User" }, permissions);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryAt = DateTime.UtcNow.AddDays(_settings.Jwt.RefreshTokenExpiryDays);
        await _context.SaveChangesAsync();

        return AuthResult.Success(accessToken, refreshToken, new AuthUserInfo
        {
            Id = user.Id,
            TenantId = tenantId.Value,
            Email = user.Email,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = new List<string> { "User" },
            Permissions = permissions
        });
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            // Don't reveal if email exists - always return success
            return true;
        }

        // Generate password reset token
        var resetToken = GenerateRefreshToken();
        // Store token (you might want to add PasswordResetToken fields to User entity)
        // For now, we'll use the RefreshToken field with a special prefix
        // In production, add proper PasswordResetToken and PasswordResetTokenExpiry fields

        _logger.LogInformation("Password reset requested for {Email}", email);
        // TODO: Send email with reset link

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        // Find user with matching reset token
        // This is a simplified implementation - in production, use dedicated reset token fields
        var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == token);

        if (user == null)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiryAt = null;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Password reset completed for user {UserId}", user.Id);
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);

        if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return true;
    }

    public async Task<AuthUserInfo?> GetUserInfoAsync(Guid userId)
    {
        var tenantId = _tenantService.GetCurrentTenantId();

        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return null;
        }

        var roleIds = user.UserRoles.Select(ur => ur.RoleId).ToList();
        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission!.Code)
            .Distinct()
            .ToListAsync();

        return new AuthUserInfo
        {
            Id = user.Id,
            TenantId = tenantId ?? Guid.Empty,
            Email = user.Email,
            FullName = user.FullName,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "User").ToList(),
            Permissions = permissions
        };
    }

    private string GenerateAccessToken(Guid userId, Guid tenantId, string email, string fullName, IEnumerable<string> roles, IEnumerable<string> permissions)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Jwt.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, fullName),
            new("tenant_id", tenantId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Jwt.Issuer,
            audience: _settings.Jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.Jwt.AccessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
