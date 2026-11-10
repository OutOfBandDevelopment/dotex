namespace OoBDev.Spike.PluginSandbox;

/// <summary>Why <see cref="PluginHost"/> would not start a plugin.</summary>
public enum HostRefusal
{
    /// <summary>Started.</summary>
    None = 0,

    /// <summary>The package failed signature or file verification.</summary>
    PackageRejected,

    /// <summary>No owner approval is on record for the plugin.</summary>
    NoGrant,

    /// <summary>The stored grant is wider than what this version of the manifest requests.</summary>
    GrantExceedsManifest,

    /// <summary>Another process holds the shared instance.</summary>
    InUseElsewhere,
}
