Create documentation for the follow source code. 

The files should be in markdown.
When reasonable include class diagrams, component models and sequence diagrams in Plant UML.
PlantUML blocks should all be identified with "```plantuml" and closed with "```"

## Source Files

```ServiceCollectionExtensions.cs
using OoBDev.System.Linq.Expressions;
using OoBDev.System.Linq.Search;
using OoBDev.System.ResponseModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.System.Linq;

/// <summary>
/// Suggested IOC configurations
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add support for shared SearchQuery Extensions
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddSearchQueryExtensions(this IServiceCollection services)
    {
        services.TryAddTransient(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
        services.TryAddTransient(typeof(ISortBuilder<>), typeof(SortBuilder<>));
        services.TryAddTransient(typeof(IExpressionTreeBuilder<>), typeof(ExpressionTreeBuilder<>));

        services.AddTransient<IPostBuildExpressionVisitor, StringComparisonReplacementExpressionVisitor>();

        //services.AddTransient<IPostBuildExpressionVisitor, SkipInstanceMethodOnNullExpressionVisitor>();

        services.TryAddScoped<ICaptureResultMessage, CaptureResultMessage>();
        return services;
    }
}

```

