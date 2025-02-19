using OpenSearch.Net;

namespace OoBDev.OpenSearch;

/// <summary>
/// Factory interface for creating instances of the <see cref="IOpenSearchLowLevelClient"/>.
/// </summary>
public interface IOpenSearchClientFactory
{
    /// <summary>
    /// Creates a new instance of the <see cref="IOpenSearchLowLevelClient"/>.
    /// </summary>
    /// <returns>A new instance of the <see cref="IOpenSearchLowLevelClient"/>.</returns>
    IOpenSearchLowLevelClient Create();
}
