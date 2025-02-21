using Microsoft.Extensions.Logging;
using Qdrant.Client.Grpc;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Qdrant;

/// <summary>
/// Extension methods for Qdrant gRPC client.
/// </summary>
public static class QdrantGrpcClientExtensions
{
    // https://github.com/qdrant/qdrant-dotnet

    /// <summary>
    /// Ensures that the specified collection exists in Qdrant. If it doesn't exist, creates the collection with the specified parameters.
    /// </summary>
    /// <param name="qdrant">The Qdrant gRPC client.</param>
    /// <param name="collectionName">The name of the collection to ensure exists.</param>
    /// <param name="vectorSize">The size of the vectors in the collection.</param>
    /// <param name="distanceCalculation">The distance calculation method for the collection.</param>
    /// <param name="logger">The logger to use for logging messages.</param>
    /// <returns>The Qdrant gRPC client.</returns>
    public static async Task<QdrantGrpcClient> EnsureCollectionExistsAsync(
        this QdrantGrpcClient qdrant,
        string collectionName,
        ulong vectorSize = 4096,
        Distance distanceCalculation = Distance.Cosine,
        ILogger? logger = null
        )
    {
        var collectionsResponse = await qdrant.Collections.ListAsync(new() { });
        foreach (var collection in collectionsResponse.Collections)
            logger?.LogInformation("List: {collection}", collection);
        if (!collectionsResponse.Collections.Any(d => d.Name == collectionName))
        {
            logger?.LogInformation("create {collection} on qdrant", collectionName);
            await qdrant.Collections.CreateAsync(
                new()
                {
                    CollectionName = collectionName,
                    VectorsConfig = new()
                    {
                        Params = new()
                        {
                            Size = vectorSize,
                            Distance = distanceCalculation,
                        },
                    }
                });
            logger?.LogInformation("created {collection} on qdrant", collectionName);
        }

        return qdrant;
    }
}
