using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OoBDev.Extensions.Linq;
using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.System.Reflection;
using OoBDev.System.ResponseModel;
using OoBDev.System.Text.Json.Serialization;
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

            operation.Tags ??= new HashSet<OpenApiTagReference>();

            if (string.Equals(context.MethodInfo.Name, "save", StringComparison.InvariantCultureIgnoreCase))
            {
                operation.Tags.Add(new OpenApiTagReference("Save"));
            }
            if (string.Equals(context.MethodInfo.Name, "get", StringComparison.InvariantCultureIgnoreCase))
            {
                operation.Tags.Add(new OpenApiTagReference("Getter"));
            }

            if (context.MethodInfo.ReturnType.IsAssignableTo(typeof(IQueryable)) && context.MethodInfo.ReturnType.IsGenericType)
            {
                operation.Tags.Add(new OpenApiTagReference(nameof(IQueryable)));

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

                    if (context.SchemaRepository.TryLookupByType(requestType, out var requestSchemaReference))
                    {
                        if (operation.RequestBody == null)
                        {
                            operation.RequestBody = new OpenApiRequestBody
                            {
                                Content = new Dictionary<string, OpenApiMediaType>()
                            };
                        }
                        ApplyContent(
                            operation.RequestBody.Content,
                            requestSchemaReference,
                            contentTypes
                            );
                    }

                    //TODO: add request type for form data
                    //var formDataTypes = new[] { "multipart/form-data", "multipart/form-data" };
                }
                else
                {
                    var request = UpdateRequestSchema(context, requestSchema, treeBuilder);

                    context.SchemaRepository.TryLookupByType(typeof(OrderDirections), out var orderSchema);

                    //Type getPropertyType(string propertyName) => elementType.GetProperty(propertyName)?.PropertyType;

                    //(OpenApiSchema item, OpenApiSchema array) getSchema(string propertyName) =>
                    //    (
                    //    context.SchemaRepository.TryLookupByType(elementType.GetProperty(propertyName)?.PropertyType, out var ps) ? ps : null,
                    //    context.SchemaRepository.TryLookupByType(elementType.GetProperty(propertyName)?.PropertyType, out var pas) ? pas : null
                    //    );

                    //TODO: build query request
                    if (request != null)
                    {
                        operation.Parameters ??= [];

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
                                //        operation.Parameters.Add(new OpenApiParameter()
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
                                    operation.Parameters.Add(new OpenApiParameter()
                                    {
                                        Name = $"{property.Key}.{sort}",
                                        Schema = orderSchema,
                                        In = ParameterLocation.Query,
                                    });
                                }
                            }
                            else
                            {
                                operation.Parameters.Add(new OpenApiParameter()
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

                if (context.SchemaRepository.TryLookupByType(pagedResponseType, out var pagedResponseSchemaReference))
                {
                    operation.Responses ??= new OpenApiResponses();
                    if (!operation.Responses.ContainsKey("200"))
                    {
                        operation.Responses["200"] = new OpenApiResponse
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                        };
                    }

                    ApplyContent(
                        operation.Responses["200"].Content,
                        pagedResponseSchemaReference,
                        context.ApiDescription.SupportedResponseTypes.SelectMany(m => m.ApiResponseFormats.Select(i => i.MediaType)).Distinct()
                        );
                }
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
        IOpenApiSchema requestSchema,
        IExpressionTreeBuilder treeBuilder
        )
    {
        if (requestSchema == null) return null;

        // Cast to actual OpenApiSchema if possible
        if (requestSchema is not OpenApiSchema schema) return null;

        if (schema.Properties.TryGetValue(nameof(ISearchQuery.PageSize), out var pageSize))
        {
            pageSize.Description = $"**Default size:** `{QueryBuilder.DefaultPageSize}`, `-1` will disable paging";
        }
        if (schema.Properties.TryGetValue(nameof(ISearchQuery.ExcludePageCount), out var excludePageCount))
        {
            excludePageCount.Description = "`true` will disable row/page counts and may decrease processing time without effecting paging functions";
        }

        if (schema.Properties.TryGetValue(nameof(ISearchQuery.Filter), out var filter))
        {
            var filterParameterSchema = context.SchemaGenerator.GenerateSchema(typeof(FilterParameter), context.SchemaRepository);
            // Nullable is handled through the schema type definition

            var filterName = context.MethodInfo.ReturnType.GenericTypeArguments[0].FullName + nameof(ISearchQuery.Filter);
            if (!context.SchemaRepository.Schemas.TryGetValue(filterName, out var filterSchema))
            {
                filterSchema = new OpenApiSchema()
                {
                    Type = JsonSchemaType.Object,
                    Description = $"**Filterable Properties:** {string.Join("; ", treeBuilder.GetFilterablePropertyNames())}",
                    Properties = new Dictionary<string, IOpenApiSchema>()
                };
                context.SchemaRepository.Schemas.Add(filterName, filterSchema);
                foreach (var propertyName in treeBuilder.GetFilterablePropertyNames())
                {
                    if (filterParameterSchema != null)
                    {
                        filterSchema.Properties[json.AsPropertyName(propertyName)] = filterParameterSchema;
                    }
                }
            }
        }

        if (schema.Properties.TryGetValue(nameof(ISearchQuery.OrderBy), out var orderBy))
        {
            var sortableProperties = treeBuilder.GetSortablePropertyNames();
            var defaultSort = from ordinal in treeBuilder.DefaultSortOrder()
                              select $"{ordinal.column} {ordinal.direction.AsString()}";

            var orderDirectionsSchema = context.SchemaGenerator.GenerateSchema(typeof(OrderDirections), context.SchemaRepository);
            // Nullable is handled through the schema type definition

            var orderByName = context.MethodInfo.ReturnType.GenericTypeArguments[0].FullName + nameof(ISearchQuery.OrderBy);
            if (!context.SchemaRepository.Schemas.TryGetValue(orderByName, out var orderBySchema))
            {
                orderBySchema = new OpenApiSchema()
                {
                    Type = JsonSchemaType.Object,
                    Description = $"**Sortable Properties:** {string.Join("; ", treeBuilder.GetSortablePropertyNames())}",
                    Properties = new Dictionary<string, IOpenApiSchema>()
                };
                context.SchemaRepository.Schemas.Add(orderByName, orderBySchema);
                foreach (var propertyName in treeBuilder.GetSortablePropertyNames())
                {
                    if (orderDirectionsSchema != null)
                    {
                        orderBySchema.Properties[json.AsPropertyName(propertyName)] = orderDirectionsSchema;
                    }
                }
            }
        }

        if (schema.Properties.TryGetValue(nameof(ISearchQuery.SearchTerm), out var searchTerm))
        {
            searchTerm.Description = $"**Searched Properties:** {string.Join("; ", treeBuilder.GetSearchablePropertyNames())}";
        }
        return schema;
    }

    private static void ApplyContent(
        IDictionary<string, OpenApiMediaType> content,
        IOpenApiSchema schemaReference,
        IEnumerable<string> contentTypes
        )
    {
        foreach (var contentType in contentTypes)
        {
            var mediaType = new OpenApiMediaType
            {
                Schema = schemaReference,
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
