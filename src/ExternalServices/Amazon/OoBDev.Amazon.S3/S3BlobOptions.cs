namespace OoBDev.Amazon.S3;

/// <summary>Settings of the S3 blob container provider, bound from the <c>S3BlobOptions</c> section. A container name is the bucket name.</summary>
public sealed class S3BlobOptions
{
    /// <summary>Gets or sets the AWS region system name; the provider is not offered while this and <see cref="ServiceUrl"/> are both empty.</summary>
    public string? Region { get; set; }

    /// <summary>Gets or sets the access key id; empty uses the AWS credential chain.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Gets or sets the secret access key.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Gets or sets an endpoint override for an emulator such as Moto; leave empty for AWS.</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>Gets or sets whether a missing bucket is created on first use.</summary>
    public bool EnsureContainerExists { get; set; }
}
