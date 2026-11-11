namespace OoBDev.Microsoft.Azure.AISearch;

/// <summary>
/// Names used by the Azure AI Search adapter.
/// </summary>
public static class AzureAISearchGlobals
{
    /// <summary>The key field of the index.</summary>
    public const string IdField = "Id";

    /// <summary>The vector field of the index.</summary>
    public const string VectorField = "Vector";

    /// <summary>The field holding the metadata as JSON.</summary>
    public const string PayloadField = "Payload";

    /// <summary>The vector search profile name.</summary>
    public const string ProfileName = "oobdev-profile";

    /// <summary>The HNSW algorithm configuration name.</summary>
    public const string AlgorithmName = "oobdev-hnsw";
}
