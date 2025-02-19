using OoBDev.Extensions.Linq;
using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.System.Reflection;
using OoBDev.System.ResponseModel;
using OoBDev.System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// Search Query Operation filter extends Swagger/OpenAPI to provide details on IQueryable{T} endpoints.
/// </summary>
public class SearchQueryOperationFilter(
     ILogger<SearchQueryOperationFilter> logger,
     IServiceProvider serviceProvider,
     IJsonSerializer json
        ) : IOperationFilter
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Applies the Search Query Operation filter to Swagger/OpenAPI.
    /// </summary>
    /// <param name="operation">The OpenApiOperation to apply the filter to.</param>
    /// <param name="context">The OperationFilterContext containing information about the operation.</param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        try
        {
            using var scopedServiceProvider = serviceProvider.CreateScope();
            //if (context.MethodInfo.ReturnType.IsAssignableTo(typeof(IPagedQueryResult)) )
            //{
            //    var requestType = new
            //    {
            //        methods = context.MethodInfo.GetCustomAttributes(true).OfType<HttpMethodAttribute>(),
            //    };
            //}

            if (context.MethodInfo.ReturnType.IsAssignableTo(typeof(IQueryable)) && context.MethodInfo.ReturnType.IsGenericType)
            {
                var elementType = context.MethodInfo.ReturnType.GetGenericArguments()[0];
                var treeBuilder = (IExpressionTreeBuilder)ActivatorUtilities.CreateInstance(scopedServiceProvider.ServiceProvider, typeof(ExpressionTreeBuilder<>).MakeGenericType(elementType));

                var requestType = typeof(SearchQuery<>).MakeGenericType(elementType);
                //var responseType = typeof(QueryResult<>).MakeGenericType(elementType);
                var pagedResponseType = typeof(PagedQueryResult<>).MakeGenericType(elementType);

                _logger.LogInformation(
                    $"{{{nameof(context.MethodInfo.DeclaringType)}}}::{{{nameof(context.MethodInfo)}}}:>{{{nameof(elementType)}}}",
                    context.MethodInfo.DeclaringType?.Name ?? "[Lambda]",
                    context.MethodInfo.Name,
                    elementType
                    );

                //var elementSchema = context.SchemaGenerator.GenerateSchema(elementType, context.SchemaRepository);
                var requestSchema = context.SchemaGenerator.GenerateSchema(requestType, context.SchemaRepository);
                //var responseSchema = _schemaGenerator.GenerateSchema(responseType, context.SchemaRepository);
                var pagedResponseSchema = context.SchemaGenerator.GenerateSchema(pagedResponseType, context.SchemaRepository);
                var contentTypes = (
                    from responseType in context.ApiDescription.SupportedResponseTypes
                    from format in responseType.ApiResponseFormats
                    where format.MediaType.EndsWith("/json")
                    select format.MediaType
                    ).Distinct();

                if (context.ApiDescription.HttpMethod == "POST")
                {
                    var schema = UpdateRequestSchema(context, requestSchema, treeBuilder);

                    ApplyContent(
                        (operation.RequestBody ??= new OpenApiRequestBody()).Content,
                        requestSchema.Reference,
                        contentTypes
                        );

                    //TODO: add request type for form data
                    //var formDataTypes = new[] { "multipart/form-data", "multipart/form-data" };
                }
                else
                {
                    var request = UpdateRequestSchema(context, requestSchema, treeBuilder);

                    context.SchemaRepository.TryLookupByType(typeof(FilterParameter), out var filterSchemaReference);
                    var filterSchema = UpdateRequestSchema(context, filterSchemaReference, treeBuilder);

                    context.SchemaRepository.TryLookupByType(typeof(OrderDirections), out var orderSchema);

                    //Type getPropertyType(string propertyName) => elementType.GetProperty(propertyName)?.PropertyType;

                    //(OpenApiSchema item, OpenApiSchema array) getSchema(string propertyName) =>
                    //    (
                    //    context.SchemaRepository.TryLookupByType(elementType.GetProperty(propertyName)?.PropertyType, out var ps) ? ps : null,
                    //    context.SchemaRepository.TryLookupByType(elementType.GetProperty(propertyName)?.PropertyType, out var pas) ? pas : null
                    //    );

                    var parameters = operation.Parameters ??= new List<OpenApiParameter>();
                    //TODO: build query request
                    if (request != null)
                    {
                        foreach (var property in request.Properties)
                        {
                            if (property.Key.Equals(nameof(ISearchQuery.Filter), StringComparison.InvariantCultureIgnoreCase))
                            {
                                //TODO: ignore filter support for now.
                                //var filterableProperties = ExpressionTreeBuilder.GetFilterablePropertyNames(elementType);
                                //foreach (var filter in filterableProperties)
                                //{
                                //    var localFilterSchema = getSchema(filter);
                                //    foreach (var filterType in filterSchema.Properties)
                                //    {
                                //        parameters.Add(new OpenApiParameter()
                                //        {
                                //            Name = $"{property.Key}.{filter}.{filterType.Key}",
                                //            Schema = (filterType.Key == "in") ? localFilterSchema.array : localFilterSchema.item,
                                //            In = ParameterLocation.Query,
                                //        });
                                //    }
                                //}
                            }
                            else if (property.Key.Equals(nameof(ISearchQuery.OrderBy), StringComparison.InvariantCultureIgnoreCase))
                            {
                                var sortableProperties = treeBuilder.GetSortablePropertyNames();
                                foreach (var sort in sortableProperties)
                                {
                                    parameters.Add(new OpenApiParameter()
                                    {
                                        Name = $"{property.Key}.{sort}",
                                        Schema = orderSchema,
                                        In = ParameterLocation.Query,
                                    });
                                }
                            }
                            else
                            {
                                parameters.Add(new OpenApiParameter()
                                {
                                    Name = property.Key,
                                    Description = property.Value.Description,
                                    Schema = property.Value,
                                    In = ParameterLocation.Query,
                                });
                            }
                        }
                    }

                }
                ApplyContent(
                    (operation.Responses["200"] ??= new OpenApiResponse()).Content,
                    pagedResponseSchema.Reference,
                    context.ApiDescription.SupportedResponseTypes.SelectMany(m => m.ApiResponseFormats.Select(i => i.MediaType)).Distinct()
                    );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error: {{{nameof(Exception)}}}", ex.Message);
            _logger.LogDebug($"Error: {{{nameof(Exception)}}}", ex);
            throw;
        }
    }

    //private OpenApiSchema GetSchema(SchemaRepository repository, OpenApiSchema schema) =>
    //    repository.Schemas[schema.Reference.Id];

    private OpenApiSchema? UpdateRequestSchema(
        OperationFilterContext context,
        OpenApiSchema requestSchema,
        IExpressionTreeBuilder treeBuilder
        )
    {
        var schema = context.SchemaRepository.Schemas[requestSchema.Reference.Id];

        if (schema == null) return null;

        var properties = schema.Properties.ChangeComparer(StringComparer.InvariantCultureIgnoreCase);

        if (properties.TryGetValue(nameof(ISearchQuery.PageSize), out var pageSize))
        {
            pageSize.Description = $"**Default size:** `{QueryBuilder.DefaultPageSize}`, `-1` will disable paging";
        }
        if (properties.TryGetValue(nameof(ISearchQuery.ExcludePageCount), out var excludePageCount))
        {
            excludePageCount.Description = "`true` will disable row/page counts and may decrease processing time without effecting paging functions";
        }

        if (properties.TryGetValue(nameof(ISearchQuery.Filter), out var filter))
        {
            var filterSchema = context.SchemaGenerator.GenerateSchema(typeof(FilterParameter), context.SchemaRepository);
            filterSchema.Nullable = true;

            filter = properties[nameof(ISearchQuery.Filter)] = new OpenApiSchema()
            {
                Description = $"**Filterable Properties:** {string.Join("; ", treeBuilder.GetFilterablePropertyNames())}",
                Nullable = true,
            };

            foreach (var propertyName in treeBuilder.GetFilterablePropertyNames())
            {
                filter.Properties.Add(json.AsPropertyName(propertyName), filterSchema);
            }
        }

        if (properties.TryGetValue(nameof(ISearchQuery.OrderBy), out var orderBy))
        {
            var sortableProperties = treeBuilder.GetSortablePropertyNames();
            var defaultSort = from ordinal in treeBuilder.DefaultSortOrder()
                              select $"{ordinal.column} {ordinal.direction.AsString()}";

            var orderBySchema = context.SchemaGenerator.GenerateSchema(typeof(OrderDirections), context.SchemaRepository);
            orderBySchema.Nullable = true;

            orderBy = properties[nameof(ISearchQuery.OrderBy)] = new OpenApiSchema()
            {
                Description =
                        $"**Sortable Properties:** {string.Join(", ", sortableProperties)}  " +
                        $"**Default Order:** {string.Join(", ", defaultSort)}",
                Nullable = true,
            };

            foreach (var propertyName in treeBuilder.GetSortablePropertyNames())
            {
                orderBy.Properties.Add(json.AsPropertyName(propertyName), orderBySchema);
            }
        }

        if (properties.TryGetValue(nameof(ISearchQuery.SearchTerm), out var searchTerm))
        {
            searchTerm.Description = $"**Searched Properties:** {string.Join("; ", treeBuilder.GetSearchablePropertyNames())}";
        }
        schema.Properties = properties;
        return schema;
    }

    private static void ApplyContent(
        IDictionary<string, OpenApiMediaType> content,
        OpenApiReference reference,
        IEnumerable<string> contentTypes
        )
    {
        foreach (var contentType in contentTypes)
        {
            var mediaType = new OpenApiMediaType
            {
                Schema = new OpenApiSchema
                {
                    Reference = reference,
                },
            };
            if (content.ContainsKey(contentType))
            {
                content[contentType] = mediaType;
            }
            else
            {
                content.Add(contentType, mediaType);
            }
        }
    }
}
