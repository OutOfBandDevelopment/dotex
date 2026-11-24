using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Turns an unhandled exception into an RFC 9457 <c>application/problem+json</c> 500 response that carries the trace and correlation identifiers
/// (and, when enabled, the original error location), and logs the exception once with the same identifiers.
/// </summary>
public partial class ProblemDetailsExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ProblemDetailsExceptionHandler> _logger;
    private readonly ProblemDetailsExceptionHandlerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProblemDetailsExceptionHandler"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The handler options.</param>
    public ProblemDetailsExceptionHandler(
        ILogger<ProblemDetailsExceptionHandler> logger,
        IOptions<ProblemDetailsExceptionHandlerOptions> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    /// <inheritdoc />
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var correlation = httpContext.RequestServices.GetService<IAccessor<CorrelationInfo>>()?.Value;
        var traceId = Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier;

        LogUnhandled(_logger, exception, traceId, correlation?.CorrelationId);

        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            Instance = httpContext.Request.Path,
        };
        problem.Extensions["traceId"] = traceId;
        if (!string.IsNullOrWhiteSpace(correlation?.CorrelationId)) problem.Extensions["correlationId"] = correlation.CorrelationId;
        if (!string.IsNullOrWhiteSpace(correlation?.RequestId)) problem.Extensions["requestId"] = correlation.RequestId;
        if (_options.IncludeErrorLocation) problem.Extensions["errorLocation"] = Locate(exception);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await httpContext.Response.WriteAsJsonAsync(problem, options: null, contentType: "application/problem+json", cancellationToken);
        return true;
    }

    private static Dictionary<string, object?> Locate(Exception exception)
    {
        var frame = new StackTrace(exception, fNeedFileInfo: true).GetFrame(0);
        var method = frame?.GetMethod();
        return new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["exception"] = exception.GetType().FullName,
            ["method"] = method is null ? null : $"{method.DeclaringType?.FullName}.{method.Name}",
            ["file"] = frame?.GetFileName(),
            ["line"] = frame is { } f && f.GetFileLineNumber() > 0 ? f.GetFileLineNumber() : null,
        };
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Unhandled exception: trace {traceId}, correlation {correlationId}")]
    private static partial void LogUnhandled(ILogger logger, Exception exception, string traceId, string? correlationId);
}
