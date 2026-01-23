using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Provides methods for formatting objects to strings based on format specifications.
/// </summary>
public interface IFormatter
{
    /// <summary>
    /// Determines whether the formatter can format the specified source object.
    /// </summary>
    /// <param name="source">The object to check for formatting support.</param>
    /// <returns>True if the formatter can format the source object, otherwise false.</returns>
    bool CanFormat(object source);

    /// <summary>
    /// Formats the specified source object according to the given format specification.
    /// </summary>
    /// <param name="source">The object to format.</param>
    /// <param name="format">The format specification to apply.</param>
    /// <returns>The formatted string representation of the source object, or null if formatting fails.</returns>
    Task<string?> Format(object source, string format);
}
