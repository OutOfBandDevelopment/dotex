using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Document transformer that describes the health check endpoint in the OpenAPI document.
/// </summary>
public class HealthChecksDocumentTransformer : IOpenApiDocumentTransformer
{
    /// <summary>
    /// The endpoint for health check.
    /// </summary>
    public const string HealthCheckEndpoint = @"/health"; //TODO: make so this can be looked up

    /// <summary>
    /// Adds the health check path to the document.
    /// </summary>
    /// <param name="document">The OpenAPI document to modify.</param>
    /// <param name="context">The transformer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
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

        var operation = new OpenApiOperation
        {
            Tags = new HashSet<OpenApiTagReference> { new("ApiHealth") },
            Responses = new OpenApiResponses
            {
                ["200"] = new OpenApiResponse
                {
                    Description = "Health report",
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["application/json"] = new OpenApiMediaType { Schema = schema }
                    }
                }
            }
        };

        var pathItem = new OpenApiPathItem();
        pathItem.AddOperation(HttpMethod.Get, operation);
        document.Paths ??= [];
        document.Paths[HealthCheckEndpoint] = pathItem;
        return Task.CompletedTask;
    }
}
