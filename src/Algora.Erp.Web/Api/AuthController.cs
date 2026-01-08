using System.Security.Claims;
using Algora.Erp.Auth.Interfaces;
using Algora.Erp.Auth.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Algora.Erp.Web.Api;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Login with email and password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.LoginAsync(request.Email, request.Password, ipAddress);

        if (!result.IsSuccess)
        {
            _logger.LogWarning("API login failed for {Email}", request.Email);
            return Unauthorized(new { error = result.ErrorMessage });
        }

        _logger.LogInformation("API login successful for {Email}", request.Email);

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                fullName = result.User.FullName,
                roles = result.User.Roles
            }
        });
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

        if (!result.IsSuccess)
        {
            return Unauthorized(new { error = result.ErrorMessage });
        }

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                fullName = result.User.FullName,
                roles = result.User.Roles
            }
        });
    }

    /// <summary>
    /// Logout (revoke refresh token)
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userId, out var userGuid))
        {
            await _authService.RevokeTokenAsync(userGuid);
            _logger.LogInformation("API logout for user {UserId}", userId);
        }

        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _authService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        _logger.LogInformation("API registration successful for {Email}", request.Email);

        return Ok(new
        {
            accessToken = result.AccessToken,
            refreshToken = result.RefreshToken,
            user = new
            {
                id = result.User!.Id,
                email = result.User.Email,
                fullName = result.User.FullName,
                roles = result.User.Roles
            }
        });
    }

    /// <summary>
    /// Get current user info
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var user = await _authService.GetUserInfoAsync(userGuid);

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            firstName = user.FirstName,
            lastName = user.LastName,
            roles = user.Roles
        });
    }

    /// <summary>
    /// Change password for authenticated user
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var userGuid))
        {
            return Unauthorized();
        }

        var success = await _authService.ChangePasswordAsync(userGuid, request.CurrentPassword, request.NewPassword);

        if (!success)
        {
            return BadRequest(new { error = "Current password is incorrect" });
        }

        _logger.LogInformation("Password changed via API for user {UserId}", userId);

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Forgot password - send reset email
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _authService.ForgotPasswordAsync(request.Email);

        // Always return success to prevent email enumeration
        return Ok(new { message = "If an account exists with that email, password reset instructions have been sent." });
    }

    /// <summary>
    /// Reset password using token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var success = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!success)
        {
            return BadRequest(new { error = "Invalid or expired reset token" });
        }

        return Ok(new { message = "Password has been reset successfully" });
    }
}
