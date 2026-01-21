using System.Threading.Tasks;

namespace OoBDev.TextTemplating.Abstractions
{
    /// <summary>
    /// Text template engine
    /// </summary>
    public interface IGenerateText
    {
        /// <summary>
        /// Text template engine based on HTML and JSON Paths.  see Readme.IGenerateText.md for examples
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="template"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<string?> GenerateAsync<TModel>(string? template, TModel model);
    }
}
