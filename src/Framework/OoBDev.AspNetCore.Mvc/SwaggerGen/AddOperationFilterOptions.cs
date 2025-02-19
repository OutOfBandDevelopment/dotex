using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// Register additional IOperationFilters
/// </summary>
/// <typeparam name="T"></typeparam>
public class AddOperationFilterOptions<T> : IConfigureOptions<SwaggerGenOptions>
    where T : IOperationFilter
{
    /// <summary>
    /// Configures SwaggerGen options to add the specified operation filter.
    /// </summary>
    /// <param name="options">The SwaggerGen options to be configured.</param>
    public void Configure(SwaggerGenOptions options) => options.OperationFilter<T>();
}
