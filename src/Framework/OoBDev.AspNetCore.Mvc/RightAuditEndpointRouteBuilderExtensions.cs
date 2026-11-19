using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.AspNetCore.Authorization;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>Maps a read endpoint for the durable right audit trail.</summary>
public static class RightAuditEndpointRouteBuilderExtensions
{
    /// <summary>The most entries one call returns.</summary>
    public const int MaxPageSize = 1000;

    /// <summary>
    /// Maps <c>GET {path}?subject=&amp;result=&amp;from=&amp;to=&amp;maxCount=</c>, newest first. The caller must hold one of <paramref name="rights"/>
    /// (checked with <see cref="IApplicationRightChecker"/>, so the decision is the one every API surface uses). Needs an
    /// <see cref="IRightAuditQuery"/>, for example from <c>TryAddEntityFrameworkRightAudit</c>.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="rights">The rights, any one of which is enough; at least one is required so the trail is never public.</param>
    /// <param name="path">The route.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapRightAuditQuery(this IEndpointRouteBuilder endpoints, string[] rights, string path = "/api/v1/right-audit")
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(rights);
        if (rights.Length == 0)
        {
            throw new ArgumentException("At least one right is required.", nameof(rights));
        }

        endpoints.MapGet(path, (HttpContext context, CancellationToken cancellationToken) => QueryAsync(context, rights, cancellationToken));
        return endpoints;
    }

    /// <summary>Runs the check and the query for one request.</summary>
    /// <param name="context">The request.</param>
    /// <param name="rights">The required rights.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response.</returns>
    public static async Task<IResult> QueryAsync(HttpContext context, string[] rights, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        var checker = context.RequestServices.GetRequiredService<IApplicationRightChecker>();
        switch (checker.Check(context.User, rights))
        {
            case RightCheckResult.Unauthenticated:
                return Results.Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Authentication required");
            case RightCheckResult.Forbidden:
                return Results.Problem(statusCode: StatusCodes.Status403Forbidden, title: "Forbidden");
        }

        var query = context.Request.Query;
        if (!TryDate(query["from"], out var from) || !TryDate(query["to"], out var to))
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "from and to must be ISO 8601 dates");
        }

        var maxCount = 100;
        if (query.ContainsKey("maxCount") && (!int.TryParse(query["maxCount"], NumberStyles.None, CultureInfo.InvariantCulture, out maxCount) || maxCount < 1))
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "maxCount must be a positive number");
        }

        var filter = new RightAuditFilter(
            string.IsNullOrEmpty(query["subject"]) ? null : (string?)query["subject"],
            string.IsNullOrEmpty(query["result"]) ? null : (string?)query["result"],
            from,
            to,
            Math.Min(maxCount, MaxPageSize));
        var entries = await context.RequestServices.GetRequiredService<IRightAuditQuery>().QueryAsync(filter, cancellationToken).ConfigureAwait(false);
        return Results.Ok(entries);
    }

    private static bool TryDate(string? text, out DateTimeOffset? value)
    {
        value = null;
        if (string.IsNullOrEmpty(text))
        {
            return true;
        }

        if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }
}
