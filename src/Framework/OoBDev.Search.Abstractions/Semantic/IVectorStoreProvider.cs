namespace OoBDev.Search.Semantic;

/// <summary>
/// Interface for a vector store provider that implements IVectorStore.
/// </summary>
public interface IVectorStoreProvider : IVectorStore
{
    /// <summary>
    /// Gets or sets the name of the container.
    /// </summary>
    string CollectionName { get; set; }
}
