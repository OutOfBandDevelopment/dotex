using OoBDev.Common.ApplicationInputs;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Extensions.Middleware;

public class OoBDevInternalMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public OoBDevInternalMiddleware(
        RequestDelegate next,
        ILogger<OoBDevInternalMiddleware> logger
        )
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext, IApplicationAccess applicationAccess, IConfiguration configuration)
    {
        var endpoint = httpContext.GetEndpoint();
        if (endpoint != null)
        {
            _logger.LogInformation("Invoking: {endpoint}", endpoint);

            if (endpoint.Metadata.Any(x => x is OoBDevInternalAttribute))
            {
                var appName = httpContext.Request.RouteValues["controller"]?.ToString();
                if (string.IsNullOrWhiteSpace(appName))
                {
                    _logger.LogWarning("Controller name not in allowed not found");
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var allowedAppNames = configuration.GetSection("AppSettings:ApplicationNames").Get<string[]>();
                if (!allowedAppNames?.Contains(appName) ?? false)
                {
                    _logger.LogWarning("\"{appName}\" not in allowed", appName);
                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                var applicationApiKey = await applicationAccess.GetApplicationApiKey(appName);
                if (!string.Equals(httpContext.Request.Headers["APPKEY"], applicationApiKey, StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("APPKEY not matched");

                    httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
            }
        }
        await _next(httpContext);
    }
}
