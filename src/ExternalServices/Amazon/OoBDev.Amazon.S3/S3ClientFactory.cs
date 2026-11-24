using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;

namespace OoBDev.Amazon.S3;

/// <summary>Creates an S3 client from <see cref="S3BlobOptions"/>.</summary>
/// <param name="options">The options.</param>
public sealed class S3ClientFactory(IOptions<S3BlobOptions> options) : IS3ClientFactory
{
    /// <inheritdoc />
    public IAmazonS3 Create()
    {
        var value = options.Value;
        var config = new AmazonS3Config();
        if (!string.IsNullOrEmpty(value.Region))
        {
            config.RegionEndpoint = RegionEndpoint.GetBySystemName(value.Region);
        }

        if (!string.IsNullOrEmpty(value.ServiceUrl))
        {
            config.ServiceURL = value.ServiceUrl;
            config.ForcePathStyle = true;
            if (!string.IsNullOrEmpty(value.Region))
            {
                config.AuthenticationRegion = value.Region;
            }
        }

        return string.IsNullOrEmpty(value.AccessKeyId)
            ? new AmazonS3Client(config)
            : new AmazonS3Client(value.AccessKeyId, value.SecretAccessKey, config);
    }
}
