namespace OoBDev.Data.Vectors.Hosting;
public interface IEmbeddingSentenceTransformerQueueReader
{
    Task RunAsync(CancellationToken cancellationToken);
}
