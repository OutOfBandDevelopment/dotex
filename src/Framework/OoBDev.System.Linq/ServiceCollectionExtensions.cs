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
    /// <param name="stringCasing">Force ordinal casing for sort</param>
    /// <returns></returns>
    public static IServiceCollection TryAddSearchQueryExtensions(
        this IServiceCollection services,
#if DEBUG
        StringCasing stringCasing
#else
        StringCasing stringCasing = StringCasing.Default
#endif
        )
    {
        services.TryAddTransient(typeof(IQueryBuilder<>), typeof(QueryBuilder<>));
        services.TryAddTransient(typeof(ISortBuilder<>), typeof(SortBuilder<>));
        services.TryAddTransient(typeof(IExpressionTreeBuilder<>), typeof(ExpressionTreeBuilder<>));

        services.AddTransient<IPostBuildExpressionVisitor, StringComparisonReplacementExpressionVisitor>();

        if (stringCasing != StringCasing.Default)
            services.AddTransient<IPostBuildExpressionVisitor>(sp => ActivatorUtilities.CreateInstance<StringOrderReplacementExpressionVisitor>(sp, stringCasing));

        //services.AddTransient<IPostBuildExpressionVisitor, SkipInstanceMethodOnNullExpressionVisitor>();

        services.TryAddScoped<ICaptureResultMessage, CaptureResultMessage>();
        return services;
    }
}
