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

    /// <summary>
    /// Tests a rule (a comparison such as <c>@.Period == 2</c> or a path that must exist) against the item and its descendants.
    /// </summary>
    /// <param name="rule">The rule expression.</param>
    /// <returns><see langword="true"/> when the item or any descendant satisfies the rule.</returns>
    Task<bool> Test(string rule);
}
