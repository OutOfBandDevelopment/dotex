using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace OoBDev.AspNetCore.Mvc.SwaggerGen;

/// <summary>
/// register additional ASP.Net MVC Filters
/// </summary>
/// <typeparam name="TFilter"></typeparam>
public class AddMvcFilterOptions<TFilter> : IConfigureOptions<MvcOptions>
    where TFilter : IFilterMetadata
{
    /// <summary>
    /// Configures MVC options to add the specified filter.
    /// </summary>
    /// <param name="options">The MVC options to be configured.</param>
    public void Configure(MvcOptions options) =>
        options.Filters.Add(typeof(TFilter));
}
