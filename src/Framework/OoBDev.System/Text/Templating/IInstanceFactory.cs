using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Provides factory methods for creating templating-related instances (path resolvers, formatters, and template transforms).
/// </summary>
public interface IInstanceFactory
{
    /// <summary>
    /// Gets a path resolver for the specified source object.
    /// </summary>
    /// <param name="source">The source object to create a path resolver for.</param>
    /// <returns>A path resolver instance for navigating the source object.</returns>
    Task<IPathResolver> GetPathResolver(object source);

    /// <summary>
    /// Gets a formatter for the specified source object.
    /// </summary>
    /// <param name="source">The source object to create a formatter for.</param>
    /// <returns>A formatter instance for formatting the source object.</returns>
    Task<IFormatter> GetFormatter(object source);

    /// <summary>
    /// Gets a template transform implementation for the specified media type.
    /// </summary>
    /// <param name="mediaType">The media type (e.g., "text/html", "application/json") to get a transform for.</param>
    /// <returns>A template transform instance for the specified media type.</returns>
    Task<ITemplateTransform> GetTemplateTransform(string mediaType);
}
