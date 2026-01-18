namespace Algora.Erp.E2E.Tests.Infrastructure;

public static class TestConfiguration
{
    public static string BaseUrl => Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://localhost:5000";
    public static string AdminEmail => Environment.GetEnvironmentVariable("E2E_ADMIN_EMAIL") ?? "admin@algora.com";
    public static string AdminPassword => Environment.GetEnvironmentVariable("E2E_ADMIN_PASSWORD") ?? "Admin@123";
    public static bool Headless => Environment.GetEnvironmentVariable("E2E_HEADLESS")?.ToLower() == "true";
    public static int DefaultTimeoutSeconds => 10;
}
