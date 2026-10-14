using OoBDev.System.Accessors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Custom middleware to enable detection of language/culture from HTTP 
/// request header as well as assignment for response header
/// </summary>
/// <param name="next">The next middleware in the pipeline.</param>
public partial class CultureInfoMiddleware(
    RequestDelegate next
        )
{

    /// <summary>
    /// Invokes the middleware to process the request and response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cultureInfo">The accessor for <see cref="CultureInfo"/>.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Invoke(
        HttpContext context,
        ILogger<CultureInfoMiddleware> logger,
        IAccessor<CultureInfo> cultureInfo
        )
    {
        try
        {
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language
            var fromHeader = (string?)context.Request.Headers.AcceptLanguage;
            if (!string.IsNullOrWhiteSpace(fromHeader))
            {
                var language = fromHeader.Split(',').Select(GetCultureInfo).FirstOrDefault();
                LogSetCulture(logger, fromHeader, language);

                cultureInfo.Value = language ?? CultureInfo.CurrentCulture;
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = cultureInfo.Value;
            }

            context.Response.OnStarting(cia =>
            {
                //https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Language
                var culture = cultureInfo.Value?.ToString();
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    LogReturnCulture(logger, culture);
                    context.Response.Headers.ContentLanguage = culture;
                }

                return Task.CompletedTask;

            }, cultureInfo);

            await next.Invoke(context);
        }
        catch (Exception ex)
        {
            LogFailure(logger, ex.Message);
            LogFailureDetails(logger, ex);
            throw;
        }
    }

    private CultureInfo? GetCultureInfo(string cultureInfo)
    {
        try
        {
            var culture = cultureInfo.Split(';').FirstOrDefault();
            return culture == null ? null : CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return default;
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Set CultureInfo to \"{fromHeader}\"::{language}")]
    private static partial void LogSetCulture(ILogger logger, string fromHeader, CultureInfo? language);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Return CultureInfo as {culture}")]
    private static partial void LogReturnCulture(ILogger logger, string culture);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "{Message}")]
    private static partial void LogFailure(ILogger logger, string message);

    [LoggerMessage(EventId = 4, Level = LogLevel.Debug, Message = "Culture handling failed")]
    private static partial void LogFailureDetails(ILogger logger, Exception exception);
}
