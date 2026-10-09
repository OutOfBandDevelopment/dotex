using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using OoBDev.Extensions.Linq;
using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.System.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.OpenApi;

/// <summary>
/// Schema transformer that describes the paging, filter, sort and search term properties of <see cref="SearchQuery{TModel}"/>,
/// including which model properties can be filtered, sorted and searched.
/// </summary>
public class SearchQuerySchemaTransformer(
    IServiceProvider serviceProvider,
    ILogger<SearchQuerySchemaTransformer> logger
        ) : IOpenApiSchemaTransformer
{
    /// <summary>
    /// Adds the search query property descriptions to <see cref="SearchQuery{TModel}"/> schemas.
    /// </summary>
    /// <param name="schema">The schema to modify.</param>
    /// <param name="context">The transformer context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task.</returns>
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;
        if (context.JsonPropertyInfo != null || !type.IsGenericType || type.GetGenericTypeDefinition() != typeof(SearchQuery<>))
        {
            return Task.CompletedTask;
        }

        try
        {
            using var scope = serviceProvider.CreateScope();
            var treeBuilder = (IExpressionTreeBuilder)ActivatorUtilities.CreateInstance(
                scope.ServiceProvider, typeof(ExpressionTreeBuilder<>).MakeGenericType(type.GetGenericArguments()[0]));

            foreach (var property in type.GetProperties())
            {
                var key = schema.Properties?.Keys.FirstOrDefault(k => string.Equals(k, property.Name, StringComparison.OrdinalIgnoreCase));
                if (key != null && schema.Properties![key] is OpenApiSchema propertySchema)
                {
                    propertySchema.Description = DescribeProperty(property.Name, treeBuilder) ?? propertySchema.Description;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Unable to describe {SearchQuery}", type);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Describes a <see cref="ISearchQuery"/> property for the model that the <paramref name="treeBuilder"/> handles.
    /// </summary>
    /// <param name="propertyName">The <see cref="ISearchQuery"/> property name.</param>
    /// <param name="treeBuilder">The expression tree builder for the model.</param>
    /// <returns>The description, or <see langword="null"/> when the property has none.</returns>
    public static string? DescribeProperty(string propertyName, IExpressionTreeBuilder treeBuilder) => propertyName switch
    {
        nameof(ISearchQuery.PageSize) => $"**Default size:** `{QueryBuilder.DefaultPageSize}`, `-1` will disable paging",
        nameof(ISearchQuery.ExcludePageCount) => "`true` will disable row/page counts and may decrease processing time without effecting paging functions",
        nameof(ISearchQuery.SearchTerm) => $"**Searched Properties:** {string.Join("; ", treeBuilder.GetSearchablePropertyNames())}",
        nameof(ISearchQuery.Filter) => $"**Filterable Properties:** {string.Join("; ", treeBuilder.GetFilterablePropertyNames())}",
        nameof(ISearchQuery.OrderBy) => $"**Sortable Properties:** {string.Join("; ", treeBuilder.GetSortablePropertyNames())}" +
            DefaultSort(treeBuilder),
        _ => null,
    };

    private static string DefaultSort(IExpressionTreeBuilder treeBuilder)
    {
        var defaultSort = treeBuilder.DefaultSortOrder().Select(o => $"{o.column} {o.direction.AsString()}").ToList();
        return defaultSort.Count == 0 ? "" : $"; **Default order:** {string.Join(", ", defaultSort)}";
    }
}
