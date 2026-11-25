using System;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Options for <see cref="CultureInfoMiddleware"/>.
/// </summary>
public class CultureInfoMiddlewareOptions
{
    /// <summary>
    /// Gets or initializes the culture used when the request names none or none that is supported (for example <c>en-US</c>).
    /// When null, a request without a usable culture keeps the process culture.
    /// </summary>
    public string? DefaultCulture { get; init; }

    /// <summary>
    /// Gets or initializes the culture names the server accepts (a request for <c>fr-CA</c> also matches <c>fr</c>).
    /// Empty accepts any valid culture.
    /// </summary>
    public string[] SupportedCultures { get; init; } = [];
}
