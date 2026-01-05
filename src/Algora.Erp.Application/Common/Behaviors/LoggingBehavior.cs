using Algora.Erp.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Algora.Erp.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantService _tenantService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService,
        ITenantService tenantService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
        _tenantService = tenantService;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;
        var tenantId = _tenantService.GetCurrentTenantId();

        _logger.LogInformation("ERP Request: {Name} {@UserId} {@TenantId} {@Request}",
            requestName, userId, tenantId, request);

        var response = await next();

        _logger.LogInformation("ERP Response: {Name} {@UserId} {@TenantId}",
            requestName, userId, tenantId);

        return response;
    }
}
