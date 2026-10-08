namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// How token vectors are reduced to one sentence vector.
/// </summary>
public enum EmbeddingPooling
{
    /// <summary>Average of the token vectors that are not padding (attention mask is one).</summary>
    Mean = 0,

    /// <summary>The vector of the first (classifier) token.</summary>
    Cls = 1,
}
