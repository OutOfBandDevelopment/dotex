using System.Threading.Tasks;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Provides methods for resolving paths to items within data structures.
/// </summary>
public interface IPathResolver
{
    /// <summary>
    /// Selects an item from the data structure using the specified path expression.
    /// </summary>
    /// <param name="path">The path expression identifying the item to select.</param>
    /// <returns>The object at the specified path.</returns>
    Task<object> ItemSelector(string path);
}
