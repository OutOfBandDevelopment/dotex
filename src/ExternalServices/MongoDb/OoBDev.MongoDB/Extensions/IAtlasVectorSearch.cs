
namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Provide a centralized means to create the index for Atlas Vector Search
/// </summary>
public interface IAtlasVectorSearch
{
    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    public string Path { get; set; }

    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    public int NumDimensions { get; set; }

    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    public int Similarity { get; set; }
}
