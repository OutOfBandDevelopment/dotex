using Qdrant.Client.Grpc;

namespace OoBDev.Qdrant;

/// <summary>
/// Interface for a factory that creates instances of <see cref="QdrantGrpcClient"/>.
/// </summary>
public interface IQdrantGrpcClientFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="QdrantGrpcClient"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="QdrantGrpcClient"/>.</returns>
    QdrantGrpcClient Create();
}
