using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using System;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Middleware for handling correlation information in HTTP requests and responses.
/// </summary>
public class CorrelationInfoMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationInfoMiddleware"/> class.
    /// </summary>
    /// <param name="next">The delegate representing the next middleware in the pipeline.</param>
    public CorrelationInfoMiddleware(
        RequestDelegate next
        ) => _next = next;

    /// <summary>
    /// Invokes the middleware to handle correlation information in the HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="logger">The logger for logging correlation information.</param>
    /// <param name="correlationAccessor">The accessor for managing correlation information.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Invoke(
        HttpContext context,
        ILogger<CorrelationInfoMiddleware> logger,
        IAccessor<CorrelationInfo> correlationAccessor
        )
    {
        correlationAccessor.Value ??= new();

        {
            var correlationId = context.Request.Headers[DefinedHttpHeaders.CorrelationIdHeader];
            var requestId = context.Request.Headers[DefinedHttpHeaders.RequestIdHeader];

            correlationAccessor.Value.RequestId = string.IsNullOrWhiteSpace(requestId) ? (string?)null : requestId;

            if (string.IsNullOrWhiteSpace(correlationId))
                correlationId = correlationAccessor.Value.RequestId;

            correlationId = correlationAccessor.Value.CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString() : correlationId;

            logger.LogInformation($"Invoke: {{{nameof(correlationId)}}}/{{{nameof(requestId)}}}", correlationId, requestId);
        }

        context.Response.OnStarting(state =>
        {
            var accessor = state as IAccessor<CorrelationInfo>;

            var requestId = correlationAccessor.Value?.RequestId;
            var correlationId = correlationAccessor.Value?.CorrelationId;

            if (!string.IsNullOrWhiteSpace(correlationId)) context.Response.Headers[DefinedHttpHeaders.CorrelationIdHeader] = correlationId;
            if (!string.IsNullOrWhiteSpace(requestId)) context.Response.Headers[DefinedHttpHeaders.RequestIdHeader] = requestId;

            logger.LogInformation($"Return: {{{nameof(correlationId)}}}/{{{nameof(requestId)}}}", correlationId, requestId);

            return Task.CompletedTask;

        }, correlationAccessor);

        await _next.Invoke(context);
    }
}
