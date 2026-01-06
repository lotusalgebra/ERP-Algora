namespace Algora.Erp.Integrations.Common.Exceptions;

public class CrmApiException : Exception
{
    public string CrmType { get; }
    public int? StatusCode { get; }
    public string? ErrorCode { get; }
    public string? ResponseBody { get; }

    public CrmApiException(string crmType, string message)
        : base(message)
    {
        CrmType = crmType;
    }

    public CrmApiException(string crmType, string message, int statusCode, string? responseBody = null)
        : base(message)
    {
        CrmType = crmType;
        StatusCode = statusCode;
        ResponseBody = responseBody;
    }

    public CrmApiException(string crmType, string message, Exception innerException)
        : base(message, innerException)
    {
        CrmType = crmType;
    }

    public CrmApiException(string crmType, string message, string errorCode, int? statusCode = null)
        : base(message)
    {
        CrmType = crmType;
        ErrorCode = errorCode;
        StatusCode = statusCode;
    }
}

public class CrmAuthenticationException : CrmApiException
{
    public CrmAuthenticationException(string crmType, string message)
        : base(crmType, message)
    {
    }

    public CrmAuthenticationException(string crmType, string message, Exception innerException)
        : base(crmType, message, innerException)
    {
    }
}

public class CrmRateLimitException : CrmApiException
{
    public int? RetryAfterSeconds { get; }

    public CrmRateLimitException(string crmType, string message, int? retryAfterSeconds = null)
        : base(crmType, message, 429, null)
    {
        RetryAfterSeconds = retryAfterSeconds;
    }
}

public class CrmEntityNotFoundException : CrmApiException
{
    public string EntityType { get; }
    public string EntityId { get; }

    public CrmEntityNotFoundException(string crmType, string entityType, string entityId)
        : base(crmType, $"{entityType} with ID '{entityId}' not found", 404, null)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}
