using Amazon.S3;

namespace OoBDev.Amazon.S3;

/// <summary>Creates the S3 client the container providers share.</summary>
public interface IS3ClientFactory
{
    /// <summary>Creates a client from <see cref="S3BlobOptions"/>.</summary>
    /// <returns>The client.</returns>
    IAmazonS3 Create();
}
