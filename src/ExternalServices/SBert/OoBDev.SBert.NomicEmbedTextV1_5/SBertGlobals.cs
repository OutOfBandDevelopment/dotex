namespace OoBDev.SBert.NomicEmbedTextV1_5;

/// <summary>
/// Provider keys and configuration names of the in-process nomic-embed-text-v1.5 embedding adapter.
/// </summary>
public static class SBertGlobals
{
    /// <summary>
    /// The keyed-service key of the nomic-embed-text-v1.5 <see cref="OoBDev.AI.IEmbeddingProvider"/> and generator.
    /// </summary>
    public const string NomicEmbedTextV1_5Key = "nomic-embed-text-v1.5";

    /// <summary>
    /// The default configuration section.
    /// </summary>
    public const string DefaultSection = "NomicEmbedTextV1_5";
}
