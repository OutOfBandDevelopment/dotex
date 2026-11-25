using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// Marks a search action as accepting the HTTP <c>QUERY</c> verb: its responses carry
/// <c>Accept-Query: "application/json"</c> so clients can discover support.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class AcceptsQueryAttribute : ResultFilterAttribute
{
    /// <summary>The response header name.</summary>
    public const string HeaderName = "Accept-Query";

    /// <summary>The header value.</summary>
    public const string HeaderValue = "\"application/json\"";

    /// <inheritdoc />
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.HttpContext.Response.Headers[HeaderName] = HeaderValue;
    }
}
