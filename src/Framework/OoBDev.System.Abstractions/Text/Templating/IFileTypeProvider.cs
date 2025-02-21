using System.Collections.Generic;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Provides a collection of file types.
/// </summary>
public interface IFileTypeProvider
{
    /// <summary>
    /// Gets a collection of file types.
    /// </summary>
    IReadOnlyCollection<IFileType> Types { get; }
}
