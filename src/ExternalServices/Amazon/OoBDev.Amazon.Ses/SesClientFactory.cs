using Amazon;
using Amazon.SimpleEmailV2;
using Microsoft.Extensions.Options;

namespace OoBDev.Amazon.Ses;

/// <summary>Creates an SES v2 client from <see cref="SesOptions"/>.</summary>
/// <param name="options">The options.</param>
public sealed class SesClientFactory(IOptions<SesOptions> options) : ISesClientFactory
{
    /// <inheritdoc />
    public IAmazonSimpleEmailServiceV2 Create()
    {
        var value = options.Value;
        var region = RegionEndpoint.GetBySystemName(value.Region);
        var config = new AmazonSimpleEmailServiceV2Config { RegionEndpoint = region };
        if (!string.IsNullOrEmpty(value.ServiceUrl))
        {
            config.ServiceURL = value.ServiceUrl;
            config.AuthenticationRegion = region.SystemName;
        }

        return string.IsNullOrEmpty(value.AccessKeyId)
            ? new AmazonSimpleEmailServiceV2Client(config)
            : new AmazonSimpleEmailServiceV2Client(value.AccessKeyId, value.SecretAccessKey, config);
    }
}
