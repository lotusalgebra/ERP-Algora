using System.Text;
using Algora.Erp.Application.Common.Interfaces;
using Algora.Erp.Auth.Configuration;
using Algora.Erp.Auth.Interfaces;
using Algora.Erp.Auth.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Algora.Erp.Auth;

/// <summary>
/// Extension methods for configuring authentication services
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds authentication services with dual cookie/JWT support
    /// </summary>
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind configuration
        var authSettings = new AuthSettings();
        configuration.GetSection(AuthSettings.SectionName).Bind(authSettings);
        services.Configure<AuthSettings>(configuration.GetSection(AuthSettings.SectionName));

        // Register services
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IAuthService, AuthService>();

        // Configure authentication with multiple schemes
        services.AddAuthentication(options =>
        {
            // Default to cookie for browser requests
            options.DefaultScheme = "MultiAuth";
            options.DefaultChallengeScheme = "MultiAuth";
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = authSettings.Cookie.CookieName;
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.ExpireTimeSpan = TimeSpan.FromHours(authSettings.Cookie.ExpireTimeSpanHours);
            options.SlidingExpiration = true;
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
            options.Events = new CookieAuthenticationEvents
            {
                OnRedirectToLogin = context =>
                {
                    // For API requests, return 401 instead of redirect
                    if (IsApiRequest(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                },
                OnRedirectToAccessDenied = context =>
                {
                    // For API requests, return 403 instead of redirect
                    if (IsApiRequest(context.Request))
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                }
            };
        })
        .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authSettings.Jwt.Key)),
                ValidateIssuer = true,
                ValidIssuer = authSettings.Jwt.Issuer,
                ValidateAudience = true,
                ValidAudience = authSettings.Jwt.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    // Allow JWT from cookie as fallback
                    if (string.IsNullOrEmpty(context.Token))
                    {
                        context.Token = context.Request.Cookies["access_token"];
                    }
                    return Task.CompletedTask;
                }
            };
        })
        .AddPolicyScheme("MultiAuth", "Cookie or JWT", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                // Use JWT if Authorization header is present
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    return JwtBearerDefaults.AuthenticationScheme;
                }

                // Use JWT for API requests without auth header if access_token cookie exists
                if (IsApiRequest(context.Request) && context.Request.Cookies.ContainsKey("access_token"))
                {
                    return JwtBearerDefaults.AuthenticationScheme;
                }

                // Default to cookie authentication for browser requests
                return CookieAuthenticationDefaults.AuthenticationScheme;
            };
        });

        return services;
    }

    private static bool IsApiRequest(HttpRequest request)
    {
        return request.Path.StartsWithSegments("/api") ||
               request.Headers["Accept"].Any(h => h?.Contains("application/json") == true) ||
               request.Headers["X-Requested-With"].Any(h => h == "XMLHttpRequest");
    }
}
