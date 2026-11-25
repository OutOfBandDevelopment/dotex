namespace OoBDev.DevTerm.Remote;

/// <summary>Where a remote DevTerm connection points: a DevTerm web host and one of its saved connections.</summary>
public sealed class RemoteDeviceOptions
{
    /// <summary>Gets or sets the host's base address, for example <c>https://bench:5000</c>.</summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>Gets or sets the name of the saved connection on the host whose device is used.</summary>
    public string Connection { get; set; } = string.Empty;

    /// <summary>Gets or sets the host's shared key; exchanged for a short-lived session token before each connect.</summary>
    public string Key { get; set; } = string.Empty;
}
