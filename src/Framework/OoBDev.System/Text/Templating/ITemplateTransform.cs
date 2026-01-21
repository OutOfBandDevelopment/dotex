using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Provides methods for transforming templates by merging them with data sources.
/// </summary>
public interface ITemplateTransform
{
    /// <summary>
    /// Transforms a template by merging it with a source object.
    /// </summary>
    /// <param name="source">The data source to merge with the template.</param>
    /// <param name="template">The template string to transform.</param>
    /// <returns>The transformed result after merging the template with the source data.</returns>
    Task<string> Transform(object source, string template);
}
