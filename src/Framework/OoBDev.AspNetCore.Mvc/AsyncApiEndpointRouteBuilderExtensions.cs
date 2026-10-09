using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.AsyncApi;
using System;
using System.Net;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>
/// Extension methods that publish the AsyncAPI documents and a viewer page.
/// </summary>
public static class AsyncApiEndpointRouteBuilderExtensions
{
    /// <summary>The default viewer script (AsyncAPI web component, pinned).</summary>
    public const string DefaultViewerScriptUrl = "https://unpkg.com/@asyncapi/web-component@2.6.5/lib/asyncapi-web-component.js";

    /// <summary>
    /// Maps <c>/asyncapi/{document}.json</c> and the viewer page <c>/asyncapi/{document}</c>. Requires <c>TryAddAsyncApiServices()</c>.
    /// The viewer script comes from <c>AsyncApi:ViewerScriptUrl</c> (default: a pinned unpkg build).
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapAsyncApi(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/asyncapi/{document}.json", (string document, HttpContext context) =>
        {
            var builder = context.RequestServices.GetRequiredService<AsyncApiDocumentBuilder>();
            var built = builder.Build(document);
            return built == null
                ? Results.NotFound()
                : Results.Content(AsyncApiJsonWriter.Write(built), "application/json");
        });

        endpoints.MapGet("/asyncapi/{document}", (string document, HttpContext context) =>
        {
            var builder = context.RequestServices.GetRequiredService<AsyncApiDocumentBuilder>();
            if (builder.Build(document) == null)
            {
                return Results.NotFound();
            }

            var script = context.RequestServices.GetRequiredService<IConfiguration>()["AsyncApi:ViewerScriptUrl"] ?? DefaultViewerScriptUrl;
            var html = $$"""
                <!doctype html>
                <html lang="en">
                <head>
                  <meta charset="utf-8">
                  <meta name="viewport" content="width=device-width, initial-scale=1">
                  <title>AsyncAPI: {{WebUtility.HtmlEncode(document)}}</title>
                </head>
                <body>
                  <asyncapi-component schemaUrl="/asyncapi/{{Uri.EscapeDataString(document)}}.json" cssImportPath="https://unpkg.com/@asyncapi/react-component@2.6.5/styles/default.min.css"></asyncapi-component>
                  <script src="{{WebUtility.HtmlEncode(script)}}"></script>
                </body>
                </html>
                """;
            return Results.Content(html, "text/html");
        });

        return endpoints;
    }
}
