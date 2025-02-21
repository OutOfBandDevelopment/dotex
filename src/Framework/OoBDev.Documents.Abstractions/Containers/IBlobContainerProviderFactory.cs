namespace OoBDev.Documents.Containers;

/// <summary>
/// Implementation factory for a blob store
/// </summary>
public interface IBlobContainerProviderFactory
{
    /// <summary>
    /// factory method to create a blob container by related name for a particular provider type
    /// </summary>
    /// <param name="containerName">name of container</param>
    /// <returns>instance of the blob provider</returns>
    IBlobContainerProvider? Create(string containerName);
}
