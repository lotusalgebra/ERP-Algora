using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Algora.Erp.Admin.Data;
using Algora.Erp.Admin.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Algora.Erp.Admin.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password, string? ipAddress = null);
    Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task<AdminUser?> RegisterAsync(RegisterRequest request);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    Task<bool> ConfirmEmailAsync(string token);
    Task<AdminUser?> GetUserByIdAsync(Guid userId);
    Task<AdminUser?> GetUserByEmailAsync(string email);
    Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword);
}

public class AuthService : IAuthService
{
    private readonly AdminDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(AdminDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResult> LoginAsync(string email, string password, string? ipAddress = null)
    {
        var user = await _context.AdminUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            return AuthResult.Failed("Invalid email or password");
        }

        if (!user.IsActive)
        {
            return AuthResult.Failed("Account is disabled");
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            return AuthResult.Failed($"Account is locked. Try again after {user.LockoutEnd.Value:g}");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                user.AccessFailedCount = 0;
            }
            await _context.SaveChangesAsync();
            return AuthResult.Failed("Invalid email or password");
        }

        // Reset lockout on successful login
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.LastLoginIp = ipAddress;

        // Generate tokens
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken(user.Id, ipAddress);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();

        return AuthResult.Success(accessToken, refreshToken.Token, user);
    }

    public async Task<AuthResult> RefreshTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var token = await _context.RefreshTokens
            .Include(t => t.User)
            .ThenInclude(u => u!.Role)
            .FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null)
        {
            return AuthResult.Failed("Invalid refresh token");
        }

        if (!token.IsActive)
        {
            return AuthResult.Failed("Refresh token is expired or revoked");
        }

        // Revoke old token and create new one
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.RevokeReason = "Replaced by new token";

        var newRefreshToken = GenerateRefreshToken(token.UserId, ipAddress);
        token.ReplacedByToken = newRefreshToken.Token;

        _context.RefreshTokens.Add(newRefreshToken);

        var accessToken = GenerateAccessToken(token.User!);

        await _context.SaveChangesAsync();

        return AuthResult.Success(accessToken, newRefreshToken.Token, token.User!);
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshToken);

        if (token == null || !token.IsActive)
        {
            return false;
        }

        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        token.RevokeReason = "Revoked by user";

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AdminUser?> RegisterAsync(RegisterRequest request)
    {
        if (await _context.AdminUsers.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
        {
            return null;
        }

        var defaultRole = await _context.AdminRoles.FirstOrDefaultAsync(r => r.Name == "User");
        if (defaultRole == null)
        {
            defaultRole = new AdminRole
            {
                Name = "User",
                Description = "Standard user role",
                Permissions = "[]"
            };
            _context.AdminRoles.Add(defaultRole);
            await _context.SaveChangesAsync();
        }

        var user = new AdminUser
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            RoleId = defaultRole.Id,
            SecurityStamp = Guid.NewGuid().ToString(),
            EmailConfirmationToken = GenerateRandomToken(),
            EmailConfirmationTokenExpiry = DateTime.UtcNow.AddHours(24)
        };

        _context.AdminUsers.Add(user);
        await _context.SaveChangesAsync();

        // TODO: Send confirmation email

        return user;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            return true; // Don't reveal if email exists
        }

        user.PasswordResetToken = GenerateRandomToken();
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

        await _context.SaveChangesAsync();

        // TODO: Send password reset email

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u =>
            u.PasswordResetToken == token &&
            u.PasswordResetTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmEmailAsync(string token)
    {
        var user = await _context.AdminUsers.FirstOrDefaultAsync(u =>
            u.EmailConfirmationToken == token &&
            u.EmailConfirmationTokenExpiry > DateTime.UtcNow);

        if (user == null)
        {
            return false;
        }

        user.EmailConfirmed = true;
        user.EmailConfirmedAt = DateTime.UtcNow;
        user.EmailConfirmationToken = null;
        user.EmailConfirmationTokenExpiry = null;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<AdminUser?> GetUserByIdAsync(Guid userId)
    {
        return await _context.AdminUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<AdminUser?> GetUserByEmailAsync(string email)
    {
        return await _context.AdminUsers
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.AdminUsers.FindAsync(userId);

        if (user == null || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.SecurityStamp = Guid.NewGuid().ToString();

        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateAccessToken(AdminUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "DefaultSecretKey123456789012345678901234";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "AlgoraErpAdmin";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "AlgoraErpAdmin";
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"] ?? "60");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var permissions = user.Role != null ? user.Role.Permissions : "[]";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role?.Name ?? "User"),
            new("permissions", permissions)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private RefreshToken GenerateRefreshToken(Guid userId, string? ipAddress)
    {
        var expiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");

        return new RefreshToken
        {
            UserId = userId,
            Token = GenerateRandomToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedByIp = ipAddress
        };
    }

    private static string GenerateRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

public class AuthResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public AdminUser? User { get; set; }

    public static AuthResult Success(string accessToken, string refreshToken, AdminUser user)
    {
        return new AuthResult
        {
            IsSuccess = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = user
        };
    }

    public static AuthResult Failed(string errorMessage)
    {
        return new AuthResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
