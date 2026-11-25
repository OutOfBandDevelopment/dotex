using OoBDev.System.Accessors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    /// <param name="options">The supported cultures and the default culture.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task Invoke(
        HttpContext context,
        ILogger<CultureInfoMiddleware> logger,
        IAccessor<CultureInfo> cultureInfo,
        IOptions<CultureInfoMiddlewareOptions> options
        )
    {
        try
        {
            // https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Accept-Language
            var fromHeader = (string?)context.Request.Headers.AcceptLanguage;
            var hasHeader = !string.IsNullOrWhiteSpace(fromHeader);
            var language = (hasHeader ? Negotiate(fromHeader!, options.Value) : null) ?? GetDefault(options.Value);
            if (hasHeader || language is not null)
            {
                LogSetCulture(logger, fromHeader ?? string.Empty, language);

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

    private static CultureInfo? GetDefault(CultureInfoMiddlewareOptions options) =>
        string.IsNullOrWhiteSpace(options.DefaultCulture) ? null : GetCultureInfo(options.DefaultCulture);

    private static CultureInfo? Negotiate(string header, CultureInfoMiddlewareOptions options)
    {
        var candidates = header.Split(',')
            .Select((entry, index) => (culture: GetCultureInfo(entry), quality: GetQuality(entry), index))
            .Where(c => c.culture is not null && c.quality > 0)
            .OrderByDescending(c => c.quality)
            .ThenBy(c => c.index);

        foreach (var (culture, _, _) in candidates)
        {
            if (options.SupportedCultures.Length == 0)
                return culture;

            var match = options.SupportedCultures.FirstOrDefault(s => string.Equals(s, culture!.Name, StringComparison.OrdinalIgnoreCase))
                ?? options.SupportedCultures.FirstOrDefault(s => string.Equals(s, culture!.TwoLetterISOLanguageName, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
                return GetCultureInfo(match);
        }

        return null;
    }

    private static double GetQuality(string entry)
    {
        var q = entry.Split(';').Skip(1).Select(p => p.Trim()).FirstOrDefault(p => p.StartsWith("q=", StringComparison.OrdinalIgnoreCase));
        return q is not null && double.TryParse(q[2..], NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ? value : 1;
    }

    private static CultureInfo? GetCultureInfo(string cultureInfo)
    {
        try
        {
            var culture = cultureInfo.Split(';').FirstOrDefault()?.Trim();
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
