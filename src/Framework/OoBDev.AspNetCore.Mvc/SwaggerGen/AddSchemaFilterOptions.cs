using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// Register additional IOperationFilters
/// </summary>
/// <typeparam name="T"></typeparam>
public class AddSchemaFilterOptions<T> : IConfigureOptions<SwaggerGenOptions>
    where T : ISchemaFilter
{
    /// <summary>
    /// Register additional IOperationFilters
    /// </summary>
    /// <param name="options"></param>
    public void Configure(SwaggerGenOptions options) => options.SchemaFilter<T>();
}
