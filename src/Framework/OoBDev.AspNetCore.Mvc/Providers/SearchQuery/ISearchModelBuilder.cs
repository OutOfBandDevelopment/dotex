using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Providers.SearchQuery;

/// <summary>
/// Builds search models based on the incoming HTTP context.
/// </summary>
public interface ISearchModelBuilder
{
    /// <summary>
    /// Gets the search model asynchronously from the provided HTTP context.
    /// </summary>
    /// <typeparam name="T">The type of the model.</typeparam>
    /// <param name="context">The HTTP context containing the model data.</param>
    /// <param name="modelName">The name of the model in the context.</param>
    /// <returns>A tuple indicating whether the search is successful and the retrieved model.</returns>
    Task<(bool isSearch, T? model)> GetModelAsync<T>(HttpContext context, string modelName = "") where T : class;
}
