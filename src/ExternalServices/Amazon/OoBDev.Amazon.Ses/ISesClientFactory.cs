using Amazon.SimpleEmailV2;

namespace OoBDev.Amazon.Ses;

/// <summary>Creates the SES v2 client.</summary>
public interface ISesClientFactory
{
    /// <summary>Creates a client from the options.</summary>
    /// <returns>The client; the caller disposes it.</returns>
    IAmazonSimpleEmailServiceV2 Create();
}
