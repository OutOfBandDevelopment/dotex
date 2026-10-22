using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Routes the HTTP <c>QUERY</c> verb (or <c>POST</c> with <c>X-HTTP-Method-Override: QUERY</c>) as <c>GET</c>,
/// so the collection endpoint and its rights serve it unchanged. Add it before routing.
/// </summary>
public class QueryVerbMiddleware(RequestDelegate next)
{
    /// <summary>The method override header.</summary>
    public const string OverrideHeader = "X-HTTP-Method-Override";

    private const string Query = "QUERY";

    /// <summary>
    /// Rewrites the method when the request is a query, then continues the pipeline.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public Task InvokeAsync(HttpContext context)
    {
        var request = context.Request;
        var isVerb = request.Method.Equals(Query, StringComparison.OrdinalIgnoreCase);
        var isFallback = request.Method.Equals(HttpMethods.Post, StringComparison.OrdinalIgnoreCase)
            && string.Equals(request.Headers[OverrideHeader].ToString(), Query, StringComparison.OrdinalIgnoreCase);
        if (!isVerb && !isFallback)
            return next(context);

        if (!request.HasJsonContentType() && !request.HasFormContentType)
            return WriteUnsupportedAsync(context);

        context.Features.Set<IQueryVerbFeature>(new QueryVerbFeature(isVerb ? Query : HttpMethods.Post));
        request.Method = HttpMethods.Get;
        return next(context);
    }

    private static Task WriteUnsupportedAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status415UnsupportedMediaType;
        context.Response.ContentType = "application/problem+json";
        return context.Response.WriteAsync("""{"title":"A QUERY request needs a JSON or form body.","status":415}""");
    }

    private sealed class QueryVerbFeature(string originalMethod) : IQueryVerbFeature
    {
        public string OriginalMethod { get; } = originalMethod;
    }
}
