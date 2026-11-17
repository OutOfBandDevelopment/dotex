namespace OoBDev.Data.Vectors.Postgres;

/// <summary>
/// The pgvector distance a search orders by.
/// </summary>
public enum VectorMetric
{
    /// <summary>Cosine distance (<c>&lt;=&gt;</c>); the score is the cosine similarity.</summary>
    Cosine,

    /// <summary>Euclidean distance (<c>&lt;-&gt;</c>); the score is <c>1 / (1 + distance)</c>.</summary>
    L2,
}
