namespace OoBDev.Documents.Containers;

/// <summary>
/// Implementation of a blob container for a particular provider type
/// </summary>
public interface IBlobContainerProvider : IBlobContainer
{
    /// <summary>
    /// Name of the related container
    /// </summary>
    string ContainerName { get; set; }
}
