namespace OoBDev.Search.Semantic;

/// <summary>
/// Interface for creating instances of IVectorStoreProvider.
/// </summary>
public interface IVectorStoreProviderFactory
{
    /// <summary>
    /// Creates a new instance of IVectorStoreProvider with the specified container name.
    /// </summary>
    /// <param name="containerName">The name of the container.</param>
    /// <returns>A new instance of IVectorStoreProvider.</returns>
    IVectorStoreProvider Create(string containerName);
}
