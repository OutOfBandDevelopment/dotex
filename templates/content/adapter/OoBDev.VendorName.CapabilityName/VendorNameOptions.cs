namespace OoBDev.VendorName.CapabilityName;

/// <summary>
/// VendorName connection settings. The adapter registers only when <see cref="AccountId"/> is configured.
/// </summary>
public class VendorNameOptions
{
    /// <summary>
    /// Account identifier; presence of this key enables the adapter.
    /// </summary>
    public string? AccountId { get; set; }
}
