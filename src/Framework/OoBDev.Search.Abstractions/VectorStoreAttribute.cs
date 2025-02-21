using System;

namespace OoBDev.Search;

/// <summary>
/// Attribute for specifying the container name for a vector store.
/// </summary>
/// <remarks>
/// This attribute can be applied to a class that implements IVectorStore.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class VectorStoreAttribute : Attribute
{
    /// <summary>
    /// Gets or sets the name of the container.
    /// </summary>
    public string? CollectionName { get; set; }
}
