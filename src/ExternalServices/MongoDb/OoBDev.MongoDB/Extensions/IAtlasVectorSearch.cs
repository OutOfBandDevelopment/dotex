
namespace OoBDev.MongoDB.Extensions;

/// <summary>
/// Provide a centralized means to create the index for Atlas Vector Search
/// </summary>
public interface IAtlasVectorSearch
{
    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    string Type { get; set; }

    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    string Path { get; set; }

    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    int NumDimensions { get; set; }

    /// <summary>
    /// provide a centralized means to create Atlas Vector Search
    /// </summary>
    int Similarity { get; set; }
}
