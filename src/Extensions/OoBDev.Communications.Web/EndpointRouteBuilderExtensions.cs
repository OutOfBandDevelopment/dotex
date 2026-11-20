using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OoBDev.Communications.Unsubscribe;

namespace OoBDev.Communications.Web;

/// <summary>Maps the RFC 8058 one-click unsubscribe endpoint.</summary>
public static class EndpointRouteBuilderExtensions
{
    private const string OneClick = "List-Unsubscribe=One-Click";

    /// <summary>
    /// Maps <c>POST {pattern}?t={token}</c>, the mail client's one-click request (form body <c>List-Unsubscribe=One-Click</c>), which suppresses the address in the token and answers 200.
    /// A bad token or a body that is not the one-click form answers 400 problem details. Also maps <c>GET</c> to a small page with a button that posts the same request, so a link
    /// prefetch never unsubscribes anyone. The endpoint is anonymous by design: the signed token is the credential; do not chain <c>RequireAuthorization</c>.
    /// </summary>
    /// <param name="endpoints">The route builder.</param>
    /// <param name="pattern">The route, <c>/unsubscribe</c> by default; it must match <see cref="UnsubscribeOptions.EndpointUrl"/>.</param>
    public static IEndpointConventionBuilder MapUnsubscribe(this IEndpointRouteBuilder endpoints, string pattern = "/unsubscribe")
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        var group = endpoints.MapGroup(string.Empty);
        group.MapPost(pattern, PostAsync).WithName("PostUnsubscribe").Produces(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status400BadRequest).DisableAntiforgery();
        group.MapGet(pattern, Page).WithName("GetUnsubscribe").ExcludeFromDescription();
        return group;
    }

    private static async Task<IResult> PostAsync(HttpRequest request, UnsubscribeLinks links, CancellationToken cancellationToken)
    {
        if (!request.HasFormContentType || request.Form["List-Unsubscribe"] != "One-Click")
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Not a one-click request", detail: $"Post the form value {OneClick}.");
        }

        var address = await links.UnsubscribeAsync(request.Query["t"], cancellationToken).ConfigureAwait(false);
        return address is null
            ? Results.Problem(statusCode: StatusCodes.Status400BadRequest, title: "Invalid unsubscribe token", detail: "The link is not valid.")
            : Results.Ok();
    }

    private static IResult Page(HttpRequest request)
    {
        var token = global::System.Net.WebUtility.HtmlEncode(request.Query["t"].ToString());
        var html = $"<!doctype html><title>Unsubscribe</title><form method=\"post\" action=\"?t={token}\"><input type=\"hidden\" name=\"List-Unsubscribe\" value=\"One-Click\"><button>Unsubscribe</button></form>";
        return Results.Content(html, "text/html");
    }
}
