namespace Algora.Erp.Auth.Configuration;

/// <summary>
/// Root authentication settings
/// </summary>
public class AuthSettings
{
    public const string SectionName = "Auth";

    public JwtSettings Jwt { get; set; } = new();
    public CookieSettings Cookie { get; set; } = new();
    public LockoutSettings Lockout { get; set; } = new();
}

/// <summary>
/// JWT token settings
/// </summary>
public class JwtSettings
{
    public string Key { get; set; } = "DefaultSecretKeyThatIsAtLeast32Characters!";
    public string Issuer { get; set; } = "AlgoraErp";
    public string Audience { get; set; } = "AlgoraErpWeb";
    public int AccessTokenExpiryMinutes { get; set; } = 60;
    public int RefreshTokenExpiryDays { get; set; } = 7;
}

/// <summary>
/// Cookie authentication settings
/// </summary>
public class CookieSettings
{
    public string CookieName { get; set; } = ".AlgoraErp.Auth";
    public int ExpireTimeSpanHours { get; set; } = 8;
    public int RememberMeExpireDays { get; set; } = 30;
}

/// <summary>
/// Account lockout settings
/// </summary>
public class LockoutSettings
{
    public int MaxFailedAttempts { get; set; } = 5;
    public int LockoutDurationMinutes { get; set; } = 15;
}
