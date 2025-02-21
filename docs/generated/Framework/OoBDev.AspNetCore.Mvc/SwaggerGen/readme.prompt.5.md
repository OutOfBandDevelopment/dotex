Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```ApiNamespaceControllerModelConvention.cs
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// SwaggerGen extension to configure controller group as the related assembly name
/// </summary>
public class ApiNamespaceControllerModelConvention : IControllerModelConvention
{
    /// <summary>
    /// Applies the convention to the specified controller model.
    /// </summary>
    /// <param name="controller">The controller model to apply the convention to.</param>
    public void Apply(ControllerModel controller) => controller.ApiExplorer.GroupName = controller.ControllerType.Assembly.GetName().Name;
}

```

```HealthChecksDocumentFilter.cs
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// Represents a document filter for health checks in the OpenAPI document.
/// </summary>
public class HealthChecksDocumentFilter : IDocumentFilter
{
    /// <summary>
    /// The endpoint for health check.
    /// </summary>
    public const string HealthCheckEndpoint = @"/health"; //TODO: make so this can be looked up

    /// <summary>
    /// Applies the health check filter to the OpenAPI document.
    /// </summary>
    /// <param name="openApiDocument">The OpenAPI document to which the filter is applied.</param>
    /// <param name="context">The context for the document filter.</param>
    public void Apply(OpenApiDocument openApiDocument, DocumentFilterContext context)
    {
        var pathItem = new OpenApiPathItem();

        var operation = new OpenApiOperation();
        operation.Tags.Add(new OpenApiTag { Name = "ApiHealth" });

        var properties = new Dictionary<string, OpenApiSchema>
        {
            { "status", new OpenApiSchema() { Type = "string" } },
            { "errors", new OpenApiSchema() { Type = "array" } }
        };

        var response = new OpenApiResponse();
        response.Content.Add("application/json", new OpenApiMediaType
        {
            Schema = new OpenApiSchema
            {
                Type = "object",
                AdditionalPropertiesAllowed = true,
                Properties = properties,
            }
        });

        operation.Responses.Add("200", response);
        pathItem.AddOperation(OperationType.Get, operation);
        openApiDocument?.Paths.Add(HealthCheckEndpoint, pathItem);
    }
}

```

```HealthCheckSwaggerGenEndpointOptions.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// Represents configuration options for SwaggerGen related to health check endpoints.
/// </summary>
public class HealthCheckSwaggerGenEndpointOptions : IConfigureOptions<SwaggerGenOptions>
{
    /// <summary>
    /// Configures SwaggerGen options to use the <see cref="HealthChecksDocumentFilter"/> for filtering documents.
    /// </summary>
    /// <param name="options">The SwaggerGen options to configure.</param>
    public void Configure(SwaggerGenOptions options)
    {
        options.DocumentFilter<HealthChecksDocumentFilter>();
    }
}

```

