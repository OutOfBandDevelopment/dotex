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
