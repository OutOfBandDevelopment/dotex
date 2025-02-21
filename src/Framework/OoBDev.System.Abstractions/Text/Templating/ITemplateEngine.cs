using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Represents a template engine for generating content based on templates.
/// </summary>
public interface ITemplateEngine
{
    /// <summary>
    /// Gets the template context for the specified template name.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <returns>The template context for the specified template name or <c>null</c> if not found.</returns>
    ITemplateContext? Get(string templateName);

    /// <summary>
    /// Gets all template contexts associated with the specified template name.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <returns>An enumeration of all template contexts associated with the specified template name.</returns>
    IEnumerable<ITemplateContext> GetAll(string templateName);

    /// <summary>
    /// Applies the specified data to the template identified by its name and writes the result to the target stream asynchronously.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <param name="target">The stream where the result will be written.</param>
    /// <returns>A task representing the asynchronous operation, containing the template context for the applied template or <c>null</c> if not found.</returns>
    Task<ITemplateContext?> ApplyAsync(string templateName, object data, Stream target);

    /// <summary>
    /// Applies the specified data to the given template context and writes the result to the target stream asynchronously.
    /// </summary>
    /// <param name="context">The template context to apply.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <param name="target">The stream where the result will be written.</param>
    /// <returns>A task representing the asynchronous operation, indicating whether the application was successful.</returns>
    Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target);

    /// <summary>
    /// Applies the specified data to the template identified by its name and returns the result as a string asynchronously.
    /// </summary>
    /// <param name="templateName">The name of the template.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <returns>A task representing the asynchronous operation, containing the result as a string or <c>null</c> if not found.</returns>
    Task<string?> ApplyAsync(string templateName, object data);

    /// <summary>
    /// Applies the specified data to the given template context and returns the result as a string asynchronously.
    /// </summary>
    /// <param name="context">The template context to apply.</param>
    /// <param name="data">The data to apply to the template.</param>
    /// <returns>A task representing the asynchronous operation, containing the result as a string or <c>null</c> if not found.</returns>
    Task<string?> ApplyAsync(ITemplateContext context, object data);
}
