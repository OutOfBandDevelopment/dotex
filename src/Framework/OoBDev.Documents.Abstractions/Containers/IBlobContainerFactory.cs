namespace OoBDev.Documents.Containers;

/// <summary>
/// this factory is used to build blob containers by name or type reference
/// </summary>
public interface IBlobContainerFactory
{
    /// <summary>
    /// This is used to create a blob container with the provided name
    /// </summary>
    /// <param name="containerName">name of the collection within the blob store</param>
    /// <returns></returns>
    IBlobContainer Create(string containerName);

    /// <summary>
    /// This is used to create a blob container by resolving the container name based on the referenced type
    /// </summary>
    /// <typeparam name="T">Owning type reference</typeparam>
    /// <returns></returns>
    IBlobContainer<T> Create<T>();
}
