using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Net.Http;

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

        var operation = new OpenApiOperation
        {
            Tags = new HashSet<OpenApiTagReference> { new OpenApiTagReference("ApiHealth") }
        };

        var schema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AdditionalPropertiesAllowed = true,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["status"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["errors"] = new OpenApiSchema { Type = JsonSchemaType.Array }
            }
        };

        var response = new OpenApiResponse
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/json"] = new OpenApiMediaType { Schema = schema }
            }
        };

        operation.Responses = new OpenApiResponses
        {
            ["200"] = response
        };

        pathItem.AddOperation(HttpMethod.Get, operation);
        openApiDocument?.Paths.Add(HealthCheckEndpoint, pathItem);
    }
}
