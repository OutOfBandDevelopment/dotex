using OoBDev.Toolkit.Templating.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OoBDev.TextTemplating.Abstractions
{
    /// <summary>
    /// text resolver allows to lookup individual templates as well as list 
    /// all existing templates
    /// </summary>
    public interface ITemplateResolver
    {
        /// <summary>
        /// Lookup text template by name and culture (language and country)
        /// </summary>
        /// <param name="templateName"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        Task<string> GetTemplateAsync(string templateName, CultureInfo? culture);

        /// <summary>
        /// return a list of current templates
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<TemplateSummaryModel>> GetTemplateSummariesAsync();
    }
}
