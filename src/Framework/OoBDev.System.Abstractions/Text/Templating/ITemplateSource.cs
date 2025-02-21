using System.Collections.Generic;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Represents a source of templates that can be used by a template engine.
/// </summary>
public interface ITemplateSource
{
    /// <summary>
    /// Gets the template contexts associated with the specified template name.
    /// </summary>
    /// <param name="templateName">The name of the template to retrieve.</param>
    /// <returns>An enumerable collection of template contexts.</returns>
    IEnumerable<ITemplateContext> Get(string templateName);
}
