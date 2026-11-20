using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.Metrics;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>Maps the endpoint a front end calls once per screen view, counted by screen name.</summary>
public static class ScreenViewEndpointRouteBuilderExtensions
{
    /// <summary>The meter name.</summary>
    public const string MeterName = "OoBDev.AspNetCore.Mvc.Ui";

    /// <summary>
    /// Maps <c>POST {path}</c> with the body <c>{ "screen": "name" }</c>. A name outside <paramref name="screens"/> is rejected with problem details,
    /// so a caller cannot inflate the tag values. Counts <c>oobdev.ui.screen_views</c> tagged with the screen and whether the caller is authenticated.
    /// Requires authentication or not as the host decides (chain <c>RequireAuthorization()</c> on the result); apply a rate limit to the route.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="screens">The known screen names (the front end route table).</param>
    /// <param name="path">The route.</param>
    /// <returns>The route handler builder.</returns>
    public static RouteHandlerBuilder MapScreenViews(this IEndpointRouteBuilder endpoints, IEnumerable<string> screens, string path = "/api/v1/analytics/screen-views")
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(screens);
        var known = screens.ToFrozenSet(StringComparer.Ordinal);
        if (known.Count == 0)
        {
            throw new ArgumentException("At least one screen name is required.", nameof(screens));
        }

        var counter = endpoints.ServiceProvider.GetRequiredService<IMeterFactory>()
            .Create(MeterName)
            .CreateCounter<long>("oobdev.ui.screen_views", description: "Screen views by screen name");
        return endpoints.MapPost(path, (ScreenViewRequest request, HttpContext context) => Record(request, context, known, counter));
    }

    /// <summary>Validates and counts one screen view.</summary>
    /// <param name="request">The request body.</param>
    /// <param name="context">The request.</param>
    /// <param name="known">The allowed screen names.</param>
    /// <param name="counter">The counter.</param>
    /// <returns>204, or 400 problem details for an unknown screen.</returns>
    public static IResult Record(ScreenViewRequest request, HttpContext context, FrozenSet<string> known, Counter<long> counter)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(known);
        ArgumentNullException.ThrowIfNull(counter);
        if (request.Screen is null || !known.Contains(request.Screen))
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Unknown screen");
        }

        counter.Add(1,
            new KeyValuePair<string, object?>("screen", request.Screen),
            new KeyValuePair<string, object?>("authenticated", context.User.Identity?.IsAuthenticated == true));
        return Results.NoContent();
    }
}

