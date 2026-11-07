namespace OoBDev.Amazon.Ses;

/// <summary>Settings of the SES client, bound from the <c>SesOptions</c> section.</summary>
public sealed class SesOptions
{
    /// <summary>Gets or sets the AWS region system name.</summary>
    public string Region { get; set; } = "us-east-1";

    /// <summary>Gets or sets the access key id; empty uses the AWS credential chain.</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>Gets or sets the secret access key.</summary>
    public string? SecretAccessKey { get; set; }

    /// <summary>Gets or sets an endpoint override for an emulator such as Moto; leave empty for AWS.</summary>
    public string? ServiceUrl { get; set; }

    /// <summary>Gets or sets the sender used when a message has none.</summary>
    public string? DefaultFromAddress { get; set; }

    /// <summary>Gets or sets the SES configuration set, which routes delivery events to the SNS topic.</summary>
    public string? ConfigurationSetName { get; set; }

    /// <summary>Gets or sets the SNS topic ARNs whose notifications are accepted; empty accepts any topic that passes the signature check.</summary>
    public string[] AllowedTopicArns { get; set; } = [];
}
