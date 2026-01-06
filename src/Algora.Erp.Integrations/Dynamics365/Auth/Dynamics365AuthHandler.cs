using Algora.Erp.Integrations.Common.Exceptions;
using Algora.Erp.Integrations.Common.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

namespace Algora.Erp.Integrations.Dynamics365.Auth;

public interface IDynamics365AuthHandler
{
    Task<string> GetAccessTokenAsync(CancellationToken ct = default);
    void InvalidateToken();
}

public class Dynamics365AuthHandler : IDynamics365AuthHandler
{
    private readonly Dynamics365Settings _settings;
    private IConfidentialClientApplication? _app;
    private string? _cachedToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public Dynamics365AuthHandler(IOptions<CrmIntegrationsSettings> options)
    {
        _settings = options.Value.Dynamics365;
    }

    public async Task<string> GetAccessTokenAsync(CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry.AddMinutes(-5))
        {
            return _cachedToken;
        }

        _app ??= ConfidentialClientApplicationBuilder
            .Create(_settings.ClientId)
            .WithClientSecret(_settings.ClientSecret)
            .WithAuthority(new Uri($"https://login.microsoftonline.com/{_settings.TenantId}"))
            .Build();

        var scope = $"{_settings.InstanceUrl}/.default";

        try
        {
            var result = await _app.AcquireTokenForClient(new[] { scope })
                .ExecuteAsync(ct);

            _cachedToken = result.AccessToken;
            _tokenExpiry = result.ExpiresOn.UtcDateTime;

            return _cachedToken;
        }
        catch (MsalServiceException ex)
        {
            throw new CrmAuthenticationException("Dynamics365",
                $"Authentication failed: {ex.Message}", ex);
        }
        catch (MsalClientException ex)
        {
            throw new CrmAuthenticationException("Dynamics365",
                $"Client authentication error: {ex.Message}", ex);
        }
    }

    public void InvalidateToken()
    {
        _cachedToken = null;
        _tokenExpiry = DateTime.MinValue;
    }
}
