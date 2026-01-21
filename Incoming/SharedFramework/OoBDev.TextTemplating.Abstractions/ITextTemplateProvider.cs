using OoBDev.Toolkit.Templating.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.TextTemplating.Abstractions
{
    /// <summary>
    /// Persistence provider for text templates
    /// </summary>
    public interface ITextTemplateProvider
    {
        /// <summary>
        /// lookup a text template by name, language and country
        /// </summary>
        /// <param name="name"></param>
        /// <param name="language"></param>
        /// <param name="country"></param>
        /// <returns></returns>
        Task<string> GetAsync(string name, string language, string? country);

        /// <summary>
        /// list all templates
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TemplateSummaryModel>> GetSummaryAsync();

        /// <summary>
        /// allow composable queries for existing templates
        /// </summary>
        /// <returns></returns>
        IQueryable<TextTemplateModel> Query();

        /// <summary>
        /// store template changes
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<Guid> SaveAsync(TextTemplateModel model);
    }
}
