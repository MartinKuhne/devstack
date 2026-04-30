using Microsoft.AspNetCore.Http;

using Serilog.Events;

namespace DevStack.Api.Logging;

/// <summary>
/// Pure function for resolving log levels based on HTTP response status and exceptions.
/// </summary>
public static class LogLevelResolver
{
    /// <summary>
    /// Determines the Serilog log level based on exception, elapsed time, and HTTP status code.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="elapsed">The elapsed time in seconds.</param>
    /// <param name="ex">The exception, if any.</param>
    /// <returns>The appropriate log level.</returns>
    public static LogEventLevel ResolveLevel(HttpContext httpContext, double elapsed, Exception? ex)
    {
        if (ex != null) return LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 500) return LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 400 && httpContext.Response.StatusCode < 500) return LogEventLevel.Warning;
        return LogEventLevel.Information;
    }
}

/// <summary>
/// Pure function for enriching the diagnostic context with HTTP request information.
/// </summary>
public static class RequestEnricher
{
    /// <summary>
    /// Enriches the diagnostic context with HTTP request properties.
    /// </summary>
    /// <param name="diagnosticContext">The diagnostic context to enrich.</param>
    /// <param name="httpContext">The HTTP context to extract information from.</param>
    public static void EnrichDiagnosticContext(dynamic diagnosticContext, HttpContext httpContext)
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? "");
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme ?? "");
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        diagnosticContext.Set("ContentType", httpContext.Request.ContentType ?? "");
    }
}
