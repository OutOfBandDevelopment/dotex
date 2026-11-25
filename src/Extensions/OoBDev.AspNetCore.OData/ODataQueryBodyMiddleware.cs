using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using OoBDev.AspNetCore.Mvc.Middleware;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.OData;

/// <summary>
/// For a request <see cref="QueryVerbMiddleware"/> rewrote, moves the <c>$</c> fields of a form body into the query string
/// so the OData query options bind as they do for a <c>GET</c>. Add it after <c>UseHttpQueryVerb()</c> and before routing.
/// </summary>
public class ODataQueryBodyMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Copies the OData form fields to the query string, then continues the pipeline.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var request = context.Request;
        if (context.Features.Get<IQueryVerbFeature>() is not null && request.HasFormContentType)
        {
            var form = await request.ReadFormAsync(context.RequestAborted).ConfigureAwait(false);
            var query = request.Query.ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);
            var changed = false;
            foreach (var field in form.Where(f => f.Key.StartsWith('$') && !query.ContainsKey(f.Key)))
            {
                query[field.Key] = field.Value;
                changed = true;
            }

            if (changed)
            {
                var builder = new QueryBuilder(query.SelectMany(p => p.Value.Select(v => new global::System.Collections.Generic.KeyValuePair<string, string>(p.Key, v ?? string.Empty))));
                request.QueryString = builder.ToQueryString();
            }
        }

        await next(context).ConfigureAwait(false);
    }
}
