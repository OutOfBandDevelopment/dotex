using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.System.ResponseModel;
using OoBDev.System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Operation transformer that documents <c>IQueryable{T}</c> endpoints: tags, the sortable and search query parameters,
/// the <see cref="SearchQuery{TModel}"/> request body and the <see cref="PagedQueryResult{T}"/> response.
/// </summary>
public class SearchQueryOperationTransformer(
     ILogger<SearchQueryOperationTransformer> logger,
     IServiceProvider serviceProvider,
     IJsonSerializer json
        ) : IOpenApiOperationTransformer
{
    /// <summary>
    /// Applies the search query documentation to the operation.
    /// </summary>
    /// <param name="operation">The operation to modify.</param>
    /// <param name="context">The transformer context describing the endpoint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the operation has been updated.</returns>
    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is not ControllerActionDescriptor action) return;
        var method = action.MethodInfo;

        operation.Tags ??= new HashSet<OpenApiTagReference>();
        if (string.Equals(method.Name, "save", StringComparison.OrdinalIgnoreCase))
        {
            operation.Tags.Add(new OpenApiTagReference("Save"));
        }
        if (string.Equals(method.Name, "get", StringComparison.OrdinalIgnoreCase))
        {
            operation.Tags.Add(new OpenApiTagReference("Getter"));
        }

        if (!method.ReturnType.IsAssignableTo(typeof(IQueryable)) || !method.ReturnType.IsGenericType) return;

        operation.Tags.Add(new OpenApiTagReference(nameof(IQueryable)));

        var elementType = method.ReturnType.GetGenericArguments()[0];
        var requestType = typeof(SearchQuery<>).MakeGenericType(elementType);
        var pagedResponseType = typeof(PagedQueryResult<>).MakeGenericType(elementType);

        logger.LogInformation("{DeclaringType}::{Method}:>{ElementType}", method.DeclaringType?.Name ?? "[Lambda]", method.Name, elementType);

        var jsonContentTypes = (
            from responseType in context.Description.SupportedResponseTypes
            from format in responseType.ApiResponseFormats
            where format.MediaType.EndsWith("/json", StringComparison.Ordinal)
            select format.MediaType
            ).Distinct().ToList();

        if (context.Description.HttpMethod == "POST")
        {
            var requestSchema = await context.GetOrCreateSchemaAsync(requestType, null, cancellationToken);
            var body = operation.RequestBody as OpenApiRequestBody ?? new OpenApiRequestBody();
            body.Content ??= new Dictionary<string, OpenApiMediaType>();
            ApplyContent(body.Content, requestSchema, jsonContentTypes);
            operation.RequestBody = body;
        }
        else
        {
            using var scope = serviceProvider.CreateScope();
            var treeBuilder = (IExpressionTreeBuilder)ActivatorUtilities.CreateInstance(
                scope.ServiceProvider, typeof(ExpressionTreeBuilder<>).MakeGenericType(elementType));
            await AddQueryParametersAsync(operation, context, requestType, treeBuilder, cancellationToken);
        }

        var pagedResponseSchema = await context.GetOrCreateSchemaAsync(pagedResponseType, null, cancellationToken);
        operation.Responses ??= [];
        var ok = operation.Responses.TryGetValue("200", out var existingOk) ? existingOk as OpenApiResponse : null;
        ok ??= new OpenApiResponse { Description = "OK" };
        ok.Content ??= new Dictionary<string, OpenApiMediaType>();
        operation.Responses["200"] = ok;
        ApplyContent(
            ok.Content,
            pagedResponseSchema,
            context.Description.SupportedResponseTypes.SelectMany(m => m.ApiResponseFormats.Select(i => i.MediaType)).Distinct());
    }

    private async Task AddQueryParametersAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        Type requestType,
        IExpressionTreeBuilder treeBuilder,
        CancellationToken cancellationToken)
    {
        operation.Parameters ??= [];
        var existing = operation.Parameters.Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var property in requestType.GetProperties().Where(p => p.CanRead))
        {
            var name = json.AsPropertyName(property.Name);

            if (property.Name.Equals(nameof(ISearchQuery.Filter), StringComparison.OrdinalIgnoreCase))
            {
                //TODO: ignore filter support for now.
                continue;
            }

            if (property.Name.Equals(nameof(ISearchQuery.OrderBy), StringComparison.OrdinalIgnoreCase))
            {
                var orderSchema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Enum =
                    [
                        OrderDirectionsConstants.AscendingShort,
                        OrderDirectionsConstants.DescendingShort,
                    ],
                };
                foreach (var sort in treeBuilder.GetSortablePropertyNames())
                {
                    var sortName = $"{name}.{sort}";
                    if (existing.Add(sortName))
                    {
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = sortName,
                            Schema = orderSchema,
                            In = ParameterLocation.Query,
                        });
                    }
                }
                continue;
            }

            if (existing.Add(name))
            {
                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = name,
                    Description = SearchQuerySchemaTransformer.DescribeProperty(property.Name, treeBuilder),
                    Schema = await context.GetOrCreateSchemaAsync(property.PropertyType, null, cancellationToken),
                    In = ParameterLocation.Query,
                });
            }
        }
    }

    private static void ApplyContent(
        IDictionary<string, OpenApiMediaType> content,
        IOpenApiSchema schema,
        IEnumerable<string> contentTypes)
    {
        foreach (var contentType in contentTypes)
        {
            content[contentType] = new OpenApiMediaType { Schema = schema };
        }
    }
}
