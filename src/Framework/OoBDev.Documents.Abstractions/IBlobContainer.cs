using OoBDev.Documents.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Documents;

/// <summary>
/// Interface for interacting with blob containers.
/// </summary>
public interface IBlobContainer
{
    /// <summary>
    /// Gets the content reference for the specified path.
    /// </summary>
    /// <param name="path">The path to the content.</param>
    /// <returns>The content reference, or null if not found.</returns>
    Task<ContentReference?> GetContentAsync(string path);

    /// <summary>
    /// Gets the content metadata reference for the specified path.
    /// </summary>
    /// <param name="path">The path to the content.</param>
    /// <returns>The content metadata reference, or null if not found.</returns>
    Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path);

    /// <summary>
    /// Stores the content with the specified reference, metadata, and overwrite option.
    /// </summary>
    /// <param name="reference">The content reference.</param>
    /// <param name="metadata">The metadata to store.</param>
    /// <param name="overwrite">Whether to overwrite the existing content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreContentAsync(ContentReference reference, Dictionary<string, string>? metadata = null, bool overwrite = false);

    /// <summary>
    /// Stores the content metadata with the specified reference.
    /// </summary>
    /// <param name="reference">The content metadata reference.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference);

    /// <summary>
    /// Queries the content metadata.
    /// </summary>
    /// <returns>A queryable collection of content metadata references.</returns>
    IQueryable<ContentMetaDataReference> QueryContent();

    /// <summary>
    /// Deletes the content at the specified path.
    /// </summary>
    /// <param name="path">The path to the content.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeleteContentAsync(string path);
}

/// <summary>
/// Interface for interacting with blob containers.
/// </summary>
public interface IBlobContainer<T> : IBlobContainer
{
}
