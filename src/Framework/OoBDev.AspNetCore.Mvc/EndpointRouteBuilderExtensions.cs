using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.AspNetCore.Mvc.OpenApi;
using Scalar.AspNetCore;
using System;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc;

/// <summary>
/// Extension methods that publish the OpenAPI documents and the Scalar API reference.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the OpenAPI documents (<c>/openapi/{document}.json</c>) and the Scalar API reference (<c>/scalar</c>) for every document in the
    /// <see cref="OpenApiDocumentCatalog"/>. Every registered <see cref="IApiReferenceConfigurator"/> can adjust the Scalar options.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="configure">Optional additional Scalar configuration.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder MapApiReference(this IEndpointRouteBuilder endpoints, Action<ScalarOptions>? configure = null)
    {
        var services = endpoints.ServiceProvider;
        var catalog = services.GetRequiredService<OpenApiDocumentCatalog>();
        var configurators = services.GetServices<IApiReferenceConfigurator>().ToList();

        endpoints.MapOpenApi();
        endpoints.MapScalarApiReference(options =>
        {
            options.Title = catalog.Names.Count > 1 ? "API reference" : catalog.Names.FirstOrDefault();
            options.AddDocuments(catalog.Names);
            foreach (var configurator in configurators)
            {
                configurator.Configure(options);
            }
            configure?.Invoke(options);
        });

        return endpoints;
    }
}
