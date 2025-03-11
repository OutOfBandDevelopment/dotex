namespace OoBDev.Data.Vectors.Hosting;

public class EmbeddingSentenceTransformerQueueReaderOptions
{
    public const string ConfigSection = nameof(EmbeddingSentenceTransformerQueueReaderOptions);

    public int MaximumReadLength { get; init; } = 100;
    public TimeSpan ReadWaitTimeout { get; init; } = new(0, 5, 0);
}
